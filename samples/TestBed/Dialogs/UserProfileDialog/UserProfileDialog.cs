using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace TestBed.Dialogs.UserProfileDialog
{
    public class UserProfileDialog : ComponentDialog
    {
        public UserProfileDialog()
            : base(nameof(UserProfileDialog))
        {

            var userProfileDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Recognizer = new LuisRecognizer(new LuisApplication()
                {
                    ApplicationId = "36e59c60-6df9-421a-953c-edc94950220d",
                    Endpoint = "https://westus.api.cognitive.microsoft.com",
                    EndpointKey = "a95d07785b374f0a9d7d40700e28a285"
                }),
                Generator = new TemplateEngineLanguageGenerator(new TemplateEngine().AddFile(Path.Combine(".", "Dialogs", "UserProfileDialog", "UserProfileDialog.lg"))),
                Rules = new List<IRule>()
                {
                    new EventRule() {
                        Events = new List<string>()
                        {
                            AdaptiveEvents.BeginDialog
                        },
                        Steps = new List<IDialog>() {
                            new NumberInput() {
                                Prompt = new ActivityTemplate("[GetAge]"),
                                Property = "user.profile.age",
                                InvalidPrompt = new ActivityTemplate("[GetAge.Invalid]"),
                                Validations = new List<string>()
                                {
                                    "int(turn.value) >= 1",
                                    "int(turn.value) <= 150"
                                },
                                Value = "coalesce(@personName, @userName)",
                                DoNotInterrupt = "#GetUserProfile"
                            },
                            new SendActivity("[AgeReadBack]"),
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("[GetName]"),
                                Property = "user.profile.name",
                                InvalidPrompt = new ActivityTemplate("[GetName.Invalid]"),
                                Validations = new List<string>()
                                {
                                    "length(turn.value) > 1"
                                },
                                Value = "coalesce(@userAge, @userAgeAsNum, @number, @age)",
                                DoNotInterrupt = "#GetUserProfile"
                            },
                            new SendActivity("[ProfileReadBack]")
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "Why",
                        Steps = new List<IDialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "exists(user.profile.age)",
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
                                Property = "user.profile.age",
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
                                Property = "user.profile.name",
                                Value = "Human"
                            }
                        }
                    }
                },
            };


            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(userProfileDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }
    }
}
