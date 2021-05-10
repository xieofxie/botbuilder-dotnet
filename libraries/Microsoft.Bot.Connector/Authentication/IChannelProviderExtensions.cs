// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Extention methods for the <see cref="IChannelProvider"/> class.
    /// </summary>
    public static class IChannelProviderExtensions
    {
        /// <summary>
        /// This method returns the correct Bot Framework OAuthScope for AppCredentials.
        /// </summary>
        /// <param name="channelProvider">The Channel Provider.</param>
        /// <returns>Scope.</returns>
        public static string GetToChannelFromBotOAuthScope(this IChannelProvider channelProvider)
        {
            if (channelProvider != null)
            {
                if (channelProvider.IsGovernment())
                {
                    return GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope;
                }

                if (channelProvider.IsChinaAzure())
                {
                    return ChinaAuthenticationConstants.ToChannelFromBotOAuthScope;
                }
            }

            return AuthenticationConstants.ToChannelFromBotOAuthScope;
        }

        /// <summary>
        /// This method returns the correct ToBotFromEmulatorOpenIdMetadataUrl for channel provider.
        /// </summary>
        /// <param name="channelProvider">The Channel Provider.</param>
        /// <returns>Scope.</returns>
#pragma warning disable CA1055 // URI return values should not be strings.
        public static string GetToBotFromEmulatorOpenIdMetadataUrl(this IChannelProvider channelProvider)
#pragma warning restore CA1055 // URI return values should not be strings.
        {
            if (channelProvider != null)
            {
                if (channelProvider.IsGovernment())
                {
                    return GovernmentAuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl;
                }

                if (channelProvider.IsChinaAzure())
                {
                    throw new NotImplementedException();
                }
            }

            return AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl;
        }
    }
}
