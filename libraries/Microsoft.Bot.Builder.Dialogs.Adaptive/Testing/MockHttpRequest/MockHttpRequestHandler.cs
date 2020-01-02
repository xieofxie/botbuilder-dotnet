// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.MockHttpRequest
{
    public class MockHttpRequestHandler : Mock<HttpMessageHandler>
    {
        public MockHttpRequestHandler()
            : base(MockBehavior.Strict)
        {
        }

        public void Add(MockHttpRequestData data)
        {
            this.Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Content.ReadAsStringAsync().Result.Equals(data.RequestContent) && r.Method.ToString() == data.Method && r.RequestUri.ToString().Equals(data.Uri)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage()
                {
                    Content = new StringContent(data.ResponseContent),
                    StatusCode = data.StatusCode,
                    ReasonPhrase = data.ReasonPhrase
                });
        }
    }
}
