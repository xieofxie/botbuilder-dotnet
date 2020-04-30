// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Actions.HttpRequest;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    public class HttpResponseMock
    {
        /// <summary>
        /// Gets or sets the response type.
        /// </summary>
        /// <value>
        /// The response type.
        /// </value>
        [JsonProperty("responseType")]
        public ResponseTypes ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [JsonProperty("content")]
        public object Content { get; set; }
    }
}
