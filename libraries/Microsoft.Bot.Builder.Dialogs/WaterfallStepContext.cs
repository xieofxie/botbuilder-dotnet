﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class WaterfallStepContext : DialogContext
    {
        private readonly WaterfallDialog _parentWaterfall;
        private bool _nextCalled;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaterfallStepContext"/> class.
        /// Provides context for a turn of a waterfall dialog. Contains ITurnContext as property 'Context'.
        /// </summary>
        /// <param name= "parent">The parent of the waterfall dialog.</param>
        /// <param name= "dc">The dialog's context.</param>
        /// <param name= "options">Any options to call the waterfall dialog with.</param>
        /// <param name= "values">A dictionary of values which will be persisted across all waterfall steps.</param>
        /// <param name= "index">The index of the current waterfall to execute.</param>
        /// <param name= "reason">The reason the waterfall step is being executed.</param>
        /// <param name= "result">Results returned by a dialog called in the previous waterfall step.</param>
        internal WaterfallStepContext(WaterfallDialog parentWaterfall, DialogContext dc, object options, IDictionary<string, object> values, int index, DialogReason reason, object result = null)
            : base(dc.Dialogs, turnContext: dc.Context, state: new DialogState(dc.Stack), conversationState: dc.State.Conversation, userState: dc.State.User, settings: dc.State.Settings)
        {
            _parentWaterfall = parentWaterfall;
            _nextCalled = false;
            Parent = dc.Parent;
            Index = index;
            Options = options;
            Reason = reason;
            Result = result;
            Values = values;
        }

        /// <summary>
        /// Gets the index of the current waterfall step being executed.
        /// </summary>
        /// <value>
        /// The index of the current waterfall step being executed.
        /// </value>
        public int Index { get; }

        /// <summary>
        /// Gets any options the waterfall dialog was called with.
        /// </summary>
        /// <value>
        /// Any options the waterfall dialog was called with.
        /// </value>
        public object Options { get; }

        /// <summary>
        /// Gets the reason the waterfall step is being executed.
        /// </summary>
        /// <value>
        /// The reason the waterfall step is being executed.
        /// </value>
        public DialogReason Reason { get; }

        /// <summary>
        /// Gets results returned by a dialog called in the previous waterfall step.
        /// </summary>
        /// <value>
        /// Results returned by a dialog called in the previous waterfall step.
        /// </value>
        public object Result { get; }

        /// <summary>
        /// Gets a dictionary of values which will be persisted across all waterfall actions.
        /// </summary>
        /// <value>
        /// A dictionary of values which will be persisted across all waterfall steps.
        /// </value>
        public IDictionary<string, object> Values { get; }

        /// <summary>
        /// Used to skip to the next waterfall step.
        /// </summary>
        /// <param name="result">(Optional) result to pass to the next step of the current waterfall dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>In the next step of the waterfall, the <see cref="Result"/> property of the
        /// waterfall step context will contain the value of the <paramref name="result"/>.</remarks>
        public async Task<DialogTurnResult> NextAsync(object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            // Ensure next hasn't been called
            if (_nextCalled)
            {
                throw new Exception($"WaterfallStepContext.NextAsync(): method already called for dialog and step '{_parentWaterfall.Id}[{Index}]'.");
            }

            // Trigger next step
            _nextCalled = true;
            return await _parentWaterfall.ResumeDialogAsync(this, DialogReason.NextCalled, result).ConfigureAwait(false);
        }
    }
}
