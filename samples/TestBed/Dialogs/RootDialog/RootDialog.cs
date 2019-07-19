using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using TestBed.Dialogs.UserProfileDialog;

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
                    new IntentRule()
                    {
                        Intent = "Greeting",
                        Steps = new List<IDialog>()
                        {

                        }
                    },
                    new IntentRule()
                    {
                        Intent = "Help",
                        Steps = new List<IDialog>()
                        {

                        }
                    },
                    new IntentRule()
                    {
                        Intent = "ResetProfile",
                        Steps = new List<IDialog>()
                        {

                        }
                    },
                    new IntentRule()
                    {
                        Intent = "Cancel",
                        Steps = new List<IDialog>()
                        {

                        }
                    }
                },
                Generator = new TemplateEngineLanguageGenerator(new TemplateEngine().AddFile(Path.Combine(".", "Dialogs", "RootDialog", "RootDialog.lg")))
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);
            AddDialog(new UserProfileDialog());

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
