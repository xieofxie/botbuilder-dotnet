// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Values and Constants used for Authentication and Authorization by the Bot Framework Protocol to China DataCenters.
    /// </summary>
    public static class ChinaAuthenticationConstants
    {
        /// <summary>
        /// Channel Service property value.
        /// </summary>
        public const string ChannelService = "https://botframework.azure.cn";

        /// <summary>
        /// TO CHINA CHANNEL FROM BOT: Login URL.
        /// </summary>
        public const string ToChannelFromBotLoginUrl = "https://login.partner.microsoftonline.cn/0b4a31a2-c1a0-475d-b363-5f26668660a3";

        /// <summary>
        /// TO CHINA CHANNEL FROM BOT: OAuth scope to request.
        /// </summary>
        public const string ToChannelFromBotOAuthScope = "https://api.botframework.azure.cn";
    }
}
