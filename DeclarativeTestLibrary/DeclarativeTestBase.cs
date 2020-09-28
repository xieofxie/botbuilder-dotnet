// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeclarativeTestLibrary
{
    public class DeclarativeTestBase
    {
        private delegate int HandlerDelegate(string botFolder, string testFolder, string luisKey, string endpoint, bool clearLuisCache, string testPattern, string testSubFolder, bool outputDebug, int debugPort, bool autoDetect, string outputResult);

        public static int HandleMain(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<string>(
                    new string[] { "--botFolder", "--bot" },
                    description: "Folder contains bot"),
                new Option<string>(
                    new string[] { "--testFolder", "--test" },
                    description: "Folder contains test"),
                new Option<string>(
                    new string[] { "--luisKey", "--luis" },
                    getDefaultValue: () => { return "00000000-0000-0000-0000-000000000000"; },
                    description: "Luis key for generating cache"),
                new Option<string>(
                    new string[] { "--endpoint" },
                    getDefaultValue: () => { return "https://westus.api.cognitive.microsoft.com"; },
                    description: "Luis endpoint"),
                new Option<bool>(
                    new string[] { "--clearLuisCache", "--clear" },
                    description: "Clear luis cache"),
                new Option<string>(
                    new string[] { "--testPattern", "--pattern" },
                    getDefaultValue: () => { return "*.test.dialog"; },
                    description: "Test files to search."),
                new Option<string>(
                    new string[] { "--testSubFolder", "--sub" },
                    description: "Specify a sub folder/file to test. If autoDetect, will search above recursively for testFolder/botFolder if not set."),
                new Option<bool>(
                    new string[] { "--outputDebug", "--debug" },
                    description: "Output debug to console."),
                new Option<int>(
                    new string[] { "--debugPort", "--port" },
                    getDefaultValue: () => { return -1; },
                    description: "Debug port."),
                new Option<bool>(
                    new string[] { "--autoDetect", "--auto" },
                    description: "Find config.json for testFolder, botFolder."),
                new Option<string>(
                    new string[] { "--outputResult", "--result" },
                    description: "File for saving xUnit v2 xml result."),
            };

            rootCommand.Description = "Run declarative tests";

            HandlerDelegate handlerDelegate = Handler;

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create(handlerDelegate);

            return rootCommand.InvokeAsync(args).Result;
        }

        private static int Handler(string botFolder, string testFolder, string luisKey, string endpoint, bool clearLuisCache, string testPattern, string testSubFolder, bool outputDebug, int debugPort, bool autoDetect, string outputResult)
        {
            if (outputDebug)
            {
                Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }

            if (!string.IsNullOrEmpty(testSubFolder))
            {
                if (!Path.IsPathRooted(testSubFolder))
                {
                    testSubFolder = Path.Join(Directory.GetCurrentDirectory(), testSubFolder);
                }

                if (File.Exists(testSubFolder))
                {
                    testPattern = Path.GetFileName(testSubFolder);
                    testSubFolder = Directory.GetParent(testSubFolder).FullName;
                }
            }

            if (string.IsNullOrEmpty(botFolder))
            {
                if (!autoDetect)
                {
                    return 1;
                }

                if (string.IsNullOrEmpty(testFolder))
                {
                    if (string.IsNullOrEmpty(testSubFolder))
                    {
                        return 1;
                    }

                    testFolder = testSubFolder;

                    while (true)
                    {
                        var file = Path.Join(testFolder, "config.json");
                        if (File.Exists(file))
                        {
                            var configJson = JObject.Parse(File.ReadAllText(file));
                            botFolder = configJson["botFolder"].ToString();
                            if (string.IsNullOrEmpty(botFolder))
                            {
                                return 1;
                            }

                            if (!Path.IsPathRooted(botFolder))
                            {
                                botFolder = Path.Join(testFolder, botFolder);
                            }

                            break;
                        }
                        else
                        {
                            var parent = Directory.GetParent(testFolder).FullName;
                            if (parent == testFolder)
                            {
                                return 1;
                            }

                            testFolder = parent;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(testFolder))
            {
                testFolder = botFolder;
            }

            string luisSettings = null;
            var di = new DirectoryInfo(Path.Join(botFolder, "generated"));
            if (di.Exists)
            {
                foreach (var file in di.GetFiles($"luis.settings.*", SearchOption.AllDirectories))
                {
                    luisSettings = file.FullName;
                    break;
                }
            }

            if (luisSettings == null)
            {
                di = new DirectoryInfo(Path.Join(testFolder, "generated"));
                foreach (var file in di.GetFiles($"luis.settings.*", SearchOption.AllDirectories))
                {
                    luisSettings = file.FullName;
                    break;
                }
            }

            var appsettings = Path.Join(botFolder, "settings", "appsettings.json");
            if (clearLuisCache)
            {
                var luisCache = Path.Join(testFolder, "cachedResponses");
                Directory.Delete(luisCache, true);
            }

            var config = new ConfigurationBuilder()
                .AddJsonFile(appsettings, optional: false)
                .AddJsonFile(luisSettings, optional: false)
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "luis:endpoint", endpoint },
                        { "luis:resources", testFolder },
                        { "luis:endpointKey", luisKey }
                    })
                .Build();

            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new LuisComponentRegistration());

            var resourceExplorer = new ResourceExplorer()
                .AddFolder(botFolder, monitorChanges: false)
                .AddFolder(testFolder, monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(config));

            var tests = Directory.GetFiles(string.IsNullOrEmpty(testSubFolder) ? testFolder : testSubFolder, testPattern, SearchOption.AllDirectories);
            var passed = new List<string>();
            var failed = new List<string>();

            var xmlDocument = new XmlDocument();
            var assembliesNode = xmlDocument.CreateElement("assemblies");
            xmlDocument.AppendChild(assembliesNode);

            var assemblyNode = xmlDocument.CreateElement("assembly");
            assemblyNode.SetAttribute("name", botFolder);
            assemblyNode.SetAttribute("config-file", string.Empty);
            assemblyNode.SetAttribute("test-framework", "DeclarativeUT");
            assemblyNode.SetAttribute("environment", "Windows");
            var now = DateTime.Now;

            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
            assemblyNode.SetAttribute("run-date", now.ToString("yyyy-MM-dd"));
            assemblyNode.SetAttribute("run-time", now.ToString("HH:mm:ss"));
            assembliesNode.AppendChild(assemblyNode);

            var collectionNode = xmlDocument.CreateElement("collection");
            collectionNode.SetAttribute("name", testFolder);
            assemblyNode.AppendChild(collectionNode);

            var stopWatch = new Stopwatch();
            var totalTime = 0.0;

            foreach (var test in tests)
            {
                var testFileName = Path.GetFileName(test);
                var testName = testFileName;

                var script = resourceExplorer.LoadType<TestScript>(testFileName);
                script.Configuration = config;

                Exception exception = null;
                stopWatch.Start();
                try
                {
                    script.ExecuteAsync(testName: testName, resourceExplorer: resourceExplorer, debugPort: debugPort).Wait();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                stopWatch.Stop();
                totalTime += stopWatch.ElapsedMilliseconds;

                var testNode = xmlDocument.CreateElement("test");
                testNode.SetAttribute("name", testName);
                testNode.SetAttribute("type", testName);
                testNode.SetAttribute("method", testName);
                testNode.SetAttribute("time", (stopWatch.ElapsedMilliseconds / 1000.0).ToString());
                collectionNode.AppendChild(testNode);

                if (exception == null)
                {
                    passed.Add(testName);
                    testNode.SetAttribute("result", "Pass");
                }
                else
                {
                    failed.Add(testName);
                    testNode.SetAttribute("result", "Fail");

                    Console.WriteLine(exception.GetType().Name);
                    Console.WriteLine(exception.Message);
                    Console.WriteLine(exception.StackTrace);

                    var failureNode = xmlDocument.CreateElement("failure");
                    failureNode.SetAttribute("exception-type", exception.GetType().Name);
                    testNode.AppendChild(failureNode);

                    var messageNode = xmlDocument.CreateElement("message");
                    failureNode.AppendChild(messageNode);

                    var cd = xmlDocument.CreateCDataSection(exception.Message);
                    messageNode.AppendChild(cd);

                    var stacktraceNode = xmlDocument.CreateElement("stack-trace");
                    failureNode.AppendChild(stacktraceNode);

                    cd = xmlDocument.CreateCDataSection(exception.StackTrace);
                    stacktraceNode.AppendChild(cd);
                }
            }

            Console.WriteLine($"Total {tests.Length}.");
            Console.WriteLine($"Passed {passed.Count}: {string.Join(',', passed)}.");
            Console.WriteLine($"Failed {failed.Count}: {string.Join(',', failed)}.");

            assemblyNode.SetAttribute("time", (totalTime / 1000.0).ToString());
            assemblyNode.SetAttribute("total", tests.Length.ToString());
            assemblyNode.SetAttribute("passed", passed.Count.ToString());
            assemblyNode.SetAttribute("failed", failed.Count.ToString());
            assemblyNode.SetAttribute("skipped", "0");
            assemblyNode.SetAttribute("errors", "0");

            collectionNode.SetAttribute("time", (totalTime / 1000.0).ToString());
            collectionNode.SetAttribute("total", tests.Length.ToString());
            collectionNode.SetAttribute("passed", passed.Count.ToString());
            collectionNode.SetAttribute("failed", failed.Count.ToString());
            collectionNode.SetAttribute("skipped", "0");

            if (!string.IsNullOrEmpty(outputResult))
            {
                xmlDocument.Save(outputResult);
            }

            return failed.Count == 0 ? 0 : 1;
        }
    }
}
