// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestMocks
{
    public class HttpRequestMock : TestMock
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Test.HttpRequestMock";

        [JsonConstructor]
        public HttpRequestMock([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        public override string Key
        {
            get { return DeclarativeType; }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("method")]
        public Adaptive.Actions.HttpRequest.HttpMethod Method { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("responseContent")]
        public string ResponseContent { get; set; }

        public override void Setup(ref object mockData)
        {
            if (mockData == null)
            {
                mockData = new Mock<HttpClientHandler>(MockBehavior.Strict);
            }

            // TODO regex
            var client = mockData as Mock<HttpClientHandler>;
            client.Protected()
               .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().StartsWith(Url)),
               ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(() => new HttpResponseMessage()
               {
                   Content = new StringContent(ResponseContent),
               });
        }
    }
}
