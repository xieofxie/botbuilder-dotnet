// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Schema
{
    public enum FailureStrategy
    {
        /// <summary>
        /// No failure handling.
        /// </summary>
        None,

        /// <summary>
        /// Cache previous result and if missed, behave as None.
        /// </summary>
        Cache,

        /// <summary>
        /// Use its specific fallback.
        /// </summary>
        Fallback,

        /// <summary>
        /// Cache first then fallback.
        /// </summary>
        CacheAndFallback
    }
}
