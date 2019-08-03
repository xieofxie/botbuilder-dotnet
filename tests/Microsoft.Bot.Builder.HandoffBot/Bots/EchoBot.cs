// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#define USE_ACTIVITYHANDLERWITHHANDOFF

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Handoff;
using System;

namespace Microsoft.BotBuilderSamples.Bots
{
    // Show how to use the ActivityHandlerWithHandoff 
#if USE_ACTIVITYHANDLERWITHHANDOFF
    public class EchoBot : Bot.Builder.HandoffEx.ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.Contains("human"))
            {
                await turnContext.SendActivityAsync("You will be transferred to a human agent. Sit tight.");

                var a1 = MessageFactory.Text($"first message");
                var a2 = MessageFactory.Text($"second message");
                var transcript = new Activity[] { a1, a2 };
                var context = new { Skill = "credit cards", MSCallerId = "CCI" };
                IHandoffRequest request = await turnContext.InitiateHandoffAsync(transcript, context, cancellationToken);

                if (await request.IsCompletedAsync())
                {
                    await turnContext.SendActivityAsync("Handoff request has been completed");
                }
                else
                {
                    await turnContext.SendActivityAsync("Handoff request has NOT been completed");
                }
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Echo6: {turnContext.Activity.Text}"), cancellationToken);
            }
        }

        protected override async Task OnHandoffActivityAsync(ITurnContext<Bot.Builder.HandoffEx.IHandoffActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Received Handoff ack from bot");

            // This blows up with InvalidCastException:
            // Unable to cast object of type 'Microsoft.Bot.Schema.Activity' to type 'Microsoft.Bot.Builder.HandoffEx.IHandoffActivity'
            var activity = turnContext.Activity;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!!"), cancellationToken);
                }
            }
        }
    }

    // Handle handoff inside OnTurnAsync
#else

    public class EchoBot : ActivityHandler
    {
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            switch(turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                {
                    if (turnContext.Activity.Text.Contains("human"))
                    {
                        await turnContext.SendActivityAsync("You will be transferred to a human agent. Sit tight.");

                        var a1 = MessageFactory.Text($"first message");
                        var a2 = MessageFactory.Text($"second message");
                        var transcript = new Activity[] { a1, a2 };
                        var context = new { Skill = "credit cards", MSCallerId = "CCI" };
                        IHandoffRequest request = await turnContext.InitiateHandoffAsync(transcript, context, cancellationToken);

                        if (await request.IsCompletedAsync())
                        {
                            await turnContext.SendActivityAsync("Handoff request has been completed");
                        }
                        else
                        {
                            await turnContext.SendActivityAsync("Handoff request has NOT been completed");
                        }
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
                    }
                    return;
                }
                case ActivityTypes.Handoff:
                {
                    await turnContext.SendActivityAsync("Received Handoff ack from bot");
                    return;
                }
                default:
                    break;
            }

            await base.OnTurnAsync(turnContext, cancellationToken);
            return;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
#endif
}
