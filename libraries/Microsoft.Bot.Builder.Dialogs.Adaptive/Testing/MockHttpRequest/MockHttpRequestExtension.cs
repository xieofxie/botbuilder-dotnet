// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.MockHttpRequest
{
    public static class MockHttpRequestExtension
    {
        public static readonly string Resources = "mockhttprequest:resources";

        public static BotAdapter UseMockHttpRequest(this BotAdapter botAdapter, IConfiguration configuration)
        {
            var resources = configuration.GetValue<string>(Resources);
            if (string.IsNullOrEmpty(resources))
            {
                return botAdapter;
            }

            DeclarativeTypeLoader.AddComponent(new MockHttpRequestComponentRegistration());

            var handler = new MockHttpRequestHandler();
            var files = Directory.GetFiles(Path.Combine(configuration.GetValue<string>(Resources), MockHttpRequest.DeclarativeType), "*.json");
            foreach (var file in files)
            {
                var data = File.ReadAllText(file);
                handler.Add(JsonConvert.DeserializeObject<MockHttpRequestData>(data));
            }

            botAdapter.Use(new RegisterClassMiddleware<MockHttpRequestHandler>(handler));
            return botAdapter;
        }

        public static IConfigurationBuilder UseMockHttpRequest(this IConfigurationBuilder builder, string directory)
        {
            return builder.AddInMemoryCollection(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(Resources, directory)
                });
        }
    }
}
