﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks
{
    public class MockHttpRequestMiddleware : IMiddleware
    {
        public const string HttpClientKey = "MockHttpRequestMiddleware.HttpClient";

        private readonly HttpClient _httpClient; 

        public MockHttpRequestMiddleware(List<HttpRequestMock> httpRequestMocks)
        {
            var handler = new MockHttpMessageHandler();
            foreach (var mock in httpRequestMocks)
            {
                mock.Setup(handler);
            }

            var client = handler.ToHttpClient();
            _httpClient = client;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.TurnState.Set(HttpClientKey, _httpClient);
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
