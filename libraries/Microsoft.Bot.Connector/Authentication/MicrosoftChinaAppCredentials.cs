// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// MicrosoftChinaAppCredentials auth implementation.
    /// </summary>
    public class MicrosoftChinaAppCredentials : MicrosoftAppCredentials
    {
        /// <summary>
        /// An empty set of credentials.
        /// </summary>
        public static new readonly MicrosoftChinaAppCredentials Empty = new MicrosoftChinaAppCredentials(null, null, null, null, ChinaAuthenticationConstants.ToChannelFromBotOAuthScope);

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftChinaAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token (defaults to <see cref="ChinaAuthenticationConstants.ToChannelFromBotOAuthScope"/> if null).</param>
        public MicrosoftChinaAppCredentials(string appId, string password, HttpClient customHttpClient, ILogger logger, string oAuthScope = null)
            : base(appId, password, customHttpClient, logger, oAuthScope ?? ChinaAuthenticationConstants.ToChannelFromBotOAuthScope)
        {
        }

        /// <summary>
        /// Gets the OAuth endpoint to use.
        /// </summary>
        /// <value>
        /// The OAuth endpoint to use.
        /// </value>
        public override string OAuthEndpoint
        {
            get { return ChinaAuthenticationConstants.ToChannelFromBotLoginUrl; }
        }
    }
}
