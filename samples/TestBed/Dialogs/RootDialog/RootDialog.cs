using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using TestBed.Dialogs.UserProfileDialog;
using Microsoft.Bot.Builder.AI.Luis;

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
                //Recognizer = new RegexRecognizer()
                //{
                //    Intents = new Dictionary<string, string>() {
                //        { "Greeting", "(?i)hi" },
                //        { "Start", "(?i)start" },
                //        { "Why", "(?i)why" },
                //        { "NoAge", "(?i)no age" },
                //        { "NoName", "(?i)no name" },
                //        { "Reset", "(?i)reset" }
                //    }
                //},
                Recognizer = new LuisRecognizer(new LuisApplication()
                {
                    ApplicationId = "e62cc675-88c6-4673-ad43-384f45b08e34",
                    Endpoint = "https://westus.api.cognitive.microsoft.com",
                    EndpointKey = "a95d07785b374f0a9d7d40700e28a285"
                }),
                Generator = new TemplateEngineLanguageGenerator(new TemplateEngine().AddFile(Path.Combine(".", "Dialogs", "RootDialog", "RootDialog.lg"))),
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
                            new SendActivity("[GreetingReply]")
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "Help",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("[GlobalHelp]")
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "ResetProfile",
                        Steps = new List<IDialog>()
                        {
                            new EditSteps()
                            {
                                ChangeType = StepChangeTypes.ReplaceSequence,
                                Steps = new List<IDialog>()
                                {
                                    new DeleteProperty()
                                    {
                                        Property = "user.profile"
                                    },
                                    new SendActivity("[ResetProfile]")
                                }
                            }
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "Cancel",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("[Cancel]"),
                            new CancelAllDialogs()
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "UserProfile",
                        Steps = new List<IDialog>()
                        {
                            new BeginDialog(nameof(UserProfileDialog)) 
                        }
                    }
                }
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
                                new SendActivity("[WelcomeUser]")
                            }
                        }
                    }
                }
            };

        }
    }
}
