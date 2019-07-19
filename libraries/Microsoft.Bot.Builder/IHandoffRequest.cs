// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The interface allows callers to track the completion of the handoff request.
    /// </summary>
    public interface IHandoffRequest
    {
        Task<bool> IsCompletedAsync();
    }
}
