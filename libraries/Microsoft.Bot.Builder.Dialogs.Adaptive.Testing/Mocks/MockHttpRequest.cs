// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks
{
    public class MockHttpRequest : HttpRequest
    {
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpClient = dc.Context.TurnState.Get<HttpClient>(MockHttpRequestMiddleware.HttpClientKey);
            return await base.BeginDialogAsync(dc, httpClient, cancellationToken).ConfigureAwait(false);
        }
    }
}
