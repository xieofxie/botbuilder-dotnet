// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Moq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.MockHttpRequest
{
    public class MockHttpRequestData
    {
        public string RequestContent { get; set; }

        public string Method { get; set; }

        public string Uri { get; set; }

        public string ResponseContent { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ReasonPhrase { get; set; }
    }
}
