// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.MockHttpRequest
{
    public class MockHttpRequest : HttpRequest
    {
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var handler = dc.Context.TurnState.Get<MockHttpRequestHandler>();
            return await base.BeginDialogAsync(dc, new HttpClient(handler.Object), cancellationToken);
        }
    }
}
