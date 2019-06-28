// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Builder.TestBot.Json.Recognizers;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Dialogs.Debugging.Source;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBot : ActivityHandler
    {
        private IStatePropertyAccessor<DialogState> dialogStateAccessor;
        private DialogManager dialogManager;
        private readonly ResourceExplorer resourceExplorer;

        public TestBot(ConversationState conversationState, ResourceExplorer resourceExplorer)
        {
            this.dialogStateAccessor = conversationState.CreateProperty<DialogState>("RootDialogState");
            this.resourceExplorer = resourceExplorer;

            LoadDialogs();
        }


        private void LoadDialogs()
        {
            System.Diagnostics.Trace.TraceInformation("Loading resources...");

            var rootDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new PiclRecognizer("AdaptiveDemo.picl-model"),
                Rules = new List<IRule>()
                {
                    new IntentRule()
                    {
                        Intent="Greet",
                        Steps = new List<IDialog>()
                        {
                            new IfCondition()
                            {
                                Condition = new ExpressionEngine().Parse("user.name == null"),
                                Steps = new List<IDialog>()
                                {
                                    new TextInput()
                                    {
                                        Prompt = new ActivityTemplate("Hello, what is your name?"),
                                        Property = "user.name"
                                    }
                                }
                            },
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        }
                    },
                    new IntentRule()
                    {
                        Intent="Tom",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Here are some quotes:"),
                            new SendActivity("Type Cancel anytime if you want to go back to the main menu, or ask for more to see more quotes"),
                            new SendActivity("'Last time I wrote C#, I ended up being hospitalized' Steve Ickman"),
                            new EndTurn(),
                            new SendActivity("'Arrrrrrrrrr' Tom"),
                            new EndTurn(),
                            new SendActivity("'I rewrote it last weekend' Steve"),
                            new EndTurn(),
                            new SendActivity("'If you have a request, put it in a $50 bill and leave it on my desk.' Scott"),
                            new SendActivity("We ran out of quotes, come back soon!")
                        }
                    },
                    new IntentRule()
                    {
                        Intent="Cancel",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Hope you enjoyed those! "),
                            new SendActivity("I can provide help, greet you, and read quotes from FUSE Labs until you get tired"),
                            new CancelAllDialogs()
                        }
                    },
                    new IntentRule()
                    {
                        Intent="Help",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("This is the Adaptive + Picl Recognizer demo."),
                            new SendActivity("I can provide help, greet you, and read quotes from FUSE Labs until you get tired")
                        }
                    },
                },
            };

            this.dialogManager = new DialogManager(rootDialog);

            System.Diagnostics.Trace.TraceInformation("Done loading resources.");
        }

        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken);
        }
    }
}
