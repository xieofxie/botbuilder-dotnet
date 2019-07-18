using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;

namespace Microsoft.BotBuilderSamples
{
    public class RootDialog : ComponentDialog
    {
        public RootDialog()
            : base(nameof(RootDialog))
        {
            // Create instance of adaptive dialog. 
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>() {
                        { "Greeting", "(?i)hi" },
                        { "Start", "(?i)start" },
                        { "Why", "(?i)why" },
                        { "NoAge", "(?i)no age" },
                        { "NoName", "(?i)no name" },
                        { "Reset", "(?i)reset" }
                    }
                },
                Rules = new List<IRule>()
                {
                    new ConversationUpdateActivityRule()
                    {
                        Steps = WelcomeUserSteps()
                    },
                    new IntentRule() {
                        Intent = "Start",
                        Steps = new List<IDialog>() {
                            new NumberInput() {
                                Prompt = new ActivityTemplate("What is your age? You can give me your age or say 'why' or 'no age' or type anything else."),
                                Property = "user.age",
                                AllowInterruptions = true
                            },
                            new SendActivity("Thanks. I have your age as {user.age}"),
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("What is your name? You can give me your name or say 'why' or 'no name' or type anything else."),
                                Property = "user.name",
                                AllowInterruptions = true
                            },
                            new SendActivity("Hello {user.name}, I have your age as {user.age}. You can say 'reset' to update this.")
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "Reset",
                        Steps = new List<IDialog>()
                        {
                            new EditSteps()
                            {
                                ChangeType = StepChangeTypes.ReplaceSequence,
                                Steps = new List<IDialog>()
                                {
                                    new SendActivity("Resetting profile"),
                                    new DeleteProperty()
                                    {
                                        Property = "user.name"
                                    },
                                    new DeleteProperty()
                                    {
                                        Property = "user.age"
                                    },
                                    new SendActivity("I have reset your profile. You can say 'start' to get started.")
                                }
                            }
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "Why",
                        Steps = new List<IDialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "exists(user.age)",
                                Steps = new List<IDialog>()
                                {
                                    new SendActivity("I need your name to address you correctly!")
                                },
                                ElseSteps = new List<IDialog>()
                                {
                                    new SendActivity("I need your age to be able to give you appropriate product recommendations.")
                                }
                            }
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "NoAge",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("No worries. I'll assume your age to be 30."),
                            new SendActivity("You can always say 'reset' to reset this"),
                            new SetProperty()
                            {
                                Property = "user.age",
                                Value = "30"
                            }
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "NoName",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("No worries. I'll assume your name to be 'Human'"),
                            new SendActivity("You can always say 'reset' to reset this"),
                            new SetProperty()
                            {
                                Property = "user.name",
                                Value = "Human"
                            }
                        }
                    },
                    new IntentRule() {
                        // This is the functional equivalent of a turn.N intent as well.
                        Intent = "None",
                        Steps = new List<IDialog>() {
                            // short circuiting Interruption so consultation is terminated. 
                            // indicate that the text needs to be processed for recognition
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            },
                            new SendActivity("In None...")
                        }
                    },
                    new IntentRule() {
                        Intent = "Greeting",
                        Steps = new List<IDialog>() {
                            new SendActivity("Hi, I'm the test bot! You can say 'start' to get started.")
                        }
                    }
                },
                Generator = new TemplateEngineLanguageGenerator(new TemplateEngine().AddFile(Path.Combine(".", "Dialogs", "RootDialog", "RootDialog.lg")))
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }
        private static List<IDialog> WelcomeUserSteps()
        {
            return new List<IDialog>()
            {
                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach()
                {
                    ListProperty = "turn.activity.membersAdded",
                    ValueProperty = "turn.memberAdded",
                    Steps = new List<IDialog>()
                    {
                        // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                        // Filter cases where the bot itself is the recipient of the message. 
                        new IfCondition()
                        {
                            Condition = "turn.memberAdded.name != turn.activity.recipient.name",
                            Steps = new List<IDialog>()
                            {
                                new SendActivity("Hi, I'm the test bot! \n \\[Suggestions = start\\]")
                            }
                        }
                    }
                }
            };

        }
    }
}
