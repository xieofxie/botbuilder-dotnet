using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.HandoffEx
{
    /// <summary>
    /// This class will be merged with Microsoft.Bot.Builder.ActivityHandler.
    /// </summary>
    public class ActivityHandler : Microsoft.Bot.Builder.ActivityHandler
    {
        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Handoff)
            {
                return OnHandoffActivityAsync(new DelegatingTurnContext<Microsoft.Bot.Builder.HandoffEx.IHandoffActivity>(turnContext), cancellationToken);
            }

            return base.OnTurnAsync(turnContext, cancellationToken);
        }

        protected virtual Task OnHandoffActivityAsync(ITurnContext<Microsoft.Bot.Builder.HandoffEx.IHandoffActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
