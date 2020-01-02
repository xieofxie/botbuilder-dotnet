// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.MockHttpRequest
{
    public class MockHttpRequestComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            yield return new TypeRegistration<MockHttpRequest>(MockHttpRequest.DeclarativeType);
        }
    }
}
