using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Events;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;

namespace Microsoft.BotBuilderSamples
{
    public class RootDialog : ComponentDialog
    {
        public RootDialog()
            : base(nameof(RootDialog))
        {
            var dialog1 = new AdaptiveDialog("dialog1")
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Events = new List<IOnEvent> ()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<IDialog>()
                        {
                            new SendActivity("In Dialog1"),
                            new BeginDialog()
                            {
                                DialogId = "dialog2"
                            },
                            new EndDialog()
                        }
                    }
                }
            };

            var dialog2 = new AdaptiveDialog("dialog2")
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Events = new List<IOnEvent>()
                {
                   new OnBeginDialog()
                   {
                       Actions = new List<IDialog>()
                       {
                           new SendActivity("In Dialog2"),
                           new BeginDialog()
                           {
                               DialogId = "dialog1"
                           },
                           new EndDialog()
                       }
                   }
                }
            };
            // Create instance of adaptive dialog. 
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>() {
                       
                        { "Start", "(?i)start" },
                        { "why", "(?i)why" },
                        { "confirm", "(?i)(no|yes)(.*)" }
                       
                    }
                },
                //AutoEndDialog = false,
                Events = new List<IOnEvent>()
                {
                    // Do not use beginDialog in root. If you must, ensure AutoEndDialog is set to false.
                    //new OnBeginDialog()
                    //{
                    //    Actions = new List<IDialog>()
                    //    {
                    //        new SendActivity("Hello, welcome to the demo bot!")
                    //    }
                    //},
                    new OnConversationUpdateActivity()
                    {
                        Constraint = "toLower(turn.Activity.membersAdded[0].name) != 'bot'",
                        Actions = new List<IDialog>()
                        {
                            new SendActivity("Welcome! \\n \\[suggestions=start\\]")
                        }
                    },
                    new OnIntent() {
                        Intent = "Start",
                        Actions = new List<IDialog>() {
                            new TextInput() {
                                Prompt = new ActivityTemplate("What is your name?"),
                                Property = "user.name",
                                AllowInterruptions = AllowInterruptions.Always,
                                MaxTurnCount = 3,
                                DefaultValue = "'Human'",
                                Validations = new List<string>()
                                {
                                    "length(turn.value) > 2",
                                    "length(turn.value) <= 300"
                                },
                                InvalidPrompt = new ActivityTemplate("Sorry, '{turn.value}' does not work. Give me something between 2-300 character in length. What is your name?")
                            },
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Always,
                                MaxTurnCount = 3,
                                DefaultValue = "30",
                                Validations = new List<string>()
                                {
                                    "int(turn.value) >= 1",
                                    "int(turn.value) <= 150"
                                },
                                InvalidPrompt = new ActivityTemplate("Sorry, '{turn.value}' does not work. Give me something between 1-150. What is your age?")
                            },
                            new ConfirmInput()
                            {
                                Prompt = new ActivityTemplate("I have {user.name} as your name and {user.age} as your age. Does this look good to you?"),
                                Property = "turn.confirm",
                                AllowInterruptions = AllowInterruptions.Always
                            },
                            new IfCondition()
                            {
                                Condition = "turn.confirm == true",
                                Actions = new List<IDialog>()
                                {
                                    new SendActivity("Sure. you are all set!")
                                }, 
                                ElseActions = new List<IDialog>()
                                {
                                    new SendActivity("Ok. Here's what I have.."),
                                    new SendActivity("I have {user.name} as your name and {user.age} as your age")
                                }
                            },
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "why",
                        Actions = new List<IDialog>()
                        {
                            new SendActivity("I need the information to address you correctly")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "confirm",
                        Actions = new List<IDialog>()
                        {
                            new SendActivity("In confirm..."),
                            new SetProperty()
                            {
                                Property = "user.age",
                                Value = "30"
                            }
                            //},
                            //new SetProperty()
                            //{
                            //    Property = "turn.processInput",
                            //    Value = "true"
                            //}
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(dialog1);
            AddDialog(dialog2);

            // The initial child Dialog to run.
            InitialDialogId = "dialog1";
        }
        private static List<IDialog> WelcomeUserAction()
        {
            return new List<IDialog>()
            {
                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach()
                {
                    ListProperty = "turn.activity.membersAdded",
                    ValueProperty = "turn.memberAdded",
                    Actions = new List<IDialog>()
                    {
                        // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                        // Filter cases where the bot itself is the recipient of the message. 
                        new IfCondition()
                        {
                            Condition = "turn.memberAdded.name != turn.activity.recipient.name",
                            Actions = new List<IDialog>()
                            {
                                new SendActivity("[WelcomeUser]")
                            }
                        }
                    }
                }
            };

        }
    }
}
