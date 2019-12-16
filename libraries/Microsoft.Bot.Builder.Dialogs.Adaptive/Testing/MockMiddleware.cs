// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    public class MockMiddleware : IMiddleware
    {
        private Dictionary<string, object> mockData = new Dictionary<string, object>();

        public MockMiddleware(List<TestMock> mocks)
        {
            foreach (var mock in mocks)
            {
                object data;
                mockData.TryGetValue(mock.Key, out data);
                mock.Setup(ref data);
                mockData[mock.Key] = data;
            }
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            foreach (var mock in mockData)
            {
                turnContext.TurnState[mock.Key] = mock.Value;
            }

            await nextTurn(cancellationToken).ConfigureAwait(false);
        }
    }
}
