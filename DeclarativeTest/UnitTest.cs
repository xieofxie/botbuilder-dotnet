// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeclarativeTest
{
    [TestClass]
    public class UnitTest
    {
        private static int debugPort = -1;

        private static ResourceExplorer resourceExplorer;

        private static IConfigurationRoot config;

        public static IEnumerable<object[]> Tests { get; set; }

        [ClassInitialize]
        public static void TestClassInitialize(TestContext context)
        {
            //foreach (var key in context.Properties.Keys)
            //{
            //    Console.WriteLine($"{key}: {context.Properties[key]}");
            //}

            Func<string, bool, bool> getBool = (string name, bool defaultValue) =>
            {
                if (context.Properties[name] is string value)
                {
                    return bool.Parse(value);
                }

                return defaultValue;
            };

            Func<string, int, int> getInt = (string name, int defaultValue) =>
            {
                if (context.Properties[name] is string value)
                {
                    return int.Parse(value);
                }

                return defaultValue;
            };

            string botFolder = (string)context.Properties["botFolder"];
            string testFolder = (string)context.Properties["testFolder"];
            string luisKey = (string)context.Properties["luisKey"] ?? "00000000-0000-0000-0000-000000000000";
            string endpoint = (string)context.Properties["endpoint"] ?? "https://westus.api.cognitive.microsoft.com";

            bool clearLuisCache = getBool("clearLuisCache", false);
            string testPattern = (string)context.Properties["testPattern"] ?? "*.test.dialog";
            string testSubFolder = (string)context.Properties["testSubFolder"];
            bool outputDebug = getBool("outputDebug", false);
            debugPort = getInt("debugPort", -1);
            bool autoDetect = getBool("autoDetect", false);

            if (outputDebug)
            {
                // TODO not working
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
                    return;
                }

                if (string.IsNullOrEmpty(testFolder))
                {
                    if (string.IsNullOrEmpty(testSubFolder))
                    {
                        return;
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
                                return;
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
                                return;
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

            string settings = null;
            var di = new DirectoryInfo(Path.Join(botFolder, "generated"));
            if (di.Exists)
            {
                foreach (var file in di.GetFiles($"luis.settings.*", SearchOption.AllDirectories))
                {
                    settings = file.FullName;
                    break;
                }
            }

            if (settings == null)
            {
                di = new DirectoryInfo(Path.Join(testFolder, "generated"));
                foreach (var file in di.GetFiles($"luis.settings.*", SearchOption.AllDirectories))
                {
                    settings = file.FullName;
                    break;
                }
            }

            var appsettings = Path.Join(botFolder, "settings", "appsettings.json");
            if (clearLuisCache)
            {
                var luisCache = Path.Join(testFolder, "cachedResponses");
                Directory.Delete(luisCache, true);
            }

            config = new ConfigurationBuilder()
                .AddJsonFile(appsettings, optional: false)
                .AddJsonFile(settings, optional: false)
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

            resourceExplorer = new ResourceExplorer()
                .AddFolder(botFolder, monitorChanges: false)
                .AddFolder(testFolder, monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(config));

            Tests = Directory.GetFiles(string.IsNullOrEmpty(testSubFolder) ? testFolder : testSubFolder, testPattern, SearchOption.AllDirectories).Select(test => new object[] { test });
        }

        [DataTestMethod]
        [DynamicData(nameof(Tests))]
        public void TestMethod(string test)
        {
            var testFileName = Path.GetFileName(test);
            var testName = testFileName;
            if (debugPort >= 0)
            {
                // TODO must set this before LoadType
                DebugSupport.SourceMap = new DebuggerSourceMap(new CodeModel());
            }

            var script = resourceExplorer.LoadType<TestScript>(testFileName);
            script.Configuration = config;
            script.ExecuteAsync(testName: testName, resourceExplorer: resourceExplorer, debugPort: debugPort).Wait();
        }
    }
}
