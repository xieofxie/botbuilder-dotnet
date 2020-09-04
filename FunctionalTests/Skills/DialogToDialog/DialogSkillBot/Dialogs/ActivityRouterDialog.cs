﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.DialogSkillBot.CognitiveModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.DialogSkillBot.Dialogs
{
    /// <summary>
    /// A root dialog that can route activities sent to the skill to different sub-dialogs.
    /// </summary>
    public class ActivityRouterDialog : ComponentDialog
    {
        private readonly DialogSkillBotRecognizer _luisRecognizer;

        public ActivityRouterDialog(DialogSkillBotRecognizer luisRecognizer, ConversationState conversationState, SkillConversationIdFactoryBase conversationIdFactory, SkillHttpClient skillClient, IConfiguration configuration)
            : base(nameof(ActivityRouterDialog))
        {
            _luisRecognizer = luisRecognizer;

            AddDialog(new BookingDialog());
            AddDialog(new OAuthTestDialog(configuration));

            // SkillDialog used to call EchoSkill
            var echoSkillDialog = CreateEchoSkillDialog(conversationState, conversationIdFactory, skillClient, configuration);
            AddDialog(echoSkillDialog);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { ProcessActivityAsync }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static SkillDialog CreateEchoSkillDialog(ConversationState conversationState, SkillConversationIdFactoryBase conversationIdFactory, SkillHttpClient skillClient, IConfiguration configuration)
        {
            var botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            if (string.IsNullOrWhiteSpace(botId))
            {
                throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is not in configuration");
            }

            var skillHostEndpoint = configuration.GetSection("SkillHostEndpoint")?.Value;
            if (string.IsNullOrWhiteSpace(botId))
            {
                throw new ArgumentException("SkillHostEndpoint is not in configuration");
            }

            var skillInfo = configuration.GetSection("EchoSkillInfo").Get<BotFrameworkSkill>() ?? throw new ArgumentException("EchoSkillInfo is not set in configuration");

            var skillDialogOptions = new SkillDialogOptions
            {
                BotId = botId,
                ConversationIdFactory = conversationIdFactory,
                SkillClient = skillClient,
                SkillHostEndpoint = new Uri(skillHostEndpoint),
                ConversationState = conversationState,
                Skill = skillInfo
            };
            var echoSkillDialog = new SkillDialog(skillDialogOptions);
            return echoSkillDialog;
        }

        private async Task<DialogTurnResult> ProcessActivityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // A skill can send trace activities, if needed.
            await stepContext.Context.TraceActivityAsync($"{GetType().Name}.ProcessActivityAsync()", label: $"Got ActivityType: {stepContext.Context.Activity.Type}", cancellationToken: cancellationToken);

            switch (stepContext.Context.Activity.Type)
            {
                case ActivityTypes.Event:
                    return await OnEventActivityAsync(stepContext, cancellationToken);

                case ActivityTypes.Message:
                    return await OnMessageActivityAsync(stepContext, cancellationToken);

                case ActivityTypes.Invoke:
                    return await OnInvokeActivityAsync(stepContext, cancellationToken);

                default:
                    // We didn't get an activity type we can handle.
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Unrecognized ActivityType: \"{stepContext.Context.Activity.Type}\".", inputHint: InputHints.IgnoringInput), cancellationToken);
                    return new DialogTurnResult(DialogTurnStatus.Complete);
            }
        }

        // This method performs different tasks based on the event name.
        private async Task<DialogTurnResult> OnEventActivityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;
            await stepContext.Context.TraceActivityAsync($"{GetType().Name}.OnEventActivityAsync()", label: $"Name: {activity.Name}. Value: {GetObjectAsJsonString(activity.Value)}", cancellationToken: cancellationToken);

            // Resolve what to execute based on the event name.
            switch (activity.Name)
            {
                case "BookFlight":
                    return await BeginBookFlight(stepContext, cancellationToken);

                case "GetWeather":
                    return await BeginGetWeather(stepContext, cancellationToken);

                case "OAuthTest":
                    // Start the OAuthTestDialog
                    var oAuthDialog = FindDialog(nameof(OAuthTestDialog));
                    return await stepContext.BeginDialogAsync(oAuthDialog.Id, null, cancellationToken);

                case "EchoSkill":
                    // Start the EchoSkillBot
                    var echoSkill = FindDialog(nameof(SkillDialog));
                    var messageActivity = MessageFactory.Text("Hello echo");
                    return await stepContext.BeginDialogAsync(echoSkill.Id, new BeginSkillDialogOptions { Activity = messageActivity }, cancellationToken);

                default:
                    // We didn't get an event name we can handle.
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Unrecognized EventName: \"{activity.Name}\".", inputHint: InputHints.IgnoringInput), cancellationToken);
                    return new DialogTurnResult(DialogTurnStatus.Complete);
            }
        }

        // This method just gets a message activity and runs it through LUIS. 
        private async Task<DialogTurnResult> OnMessageActivityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;
            await stepContext.Context.TraceActivityAsync($"{GetType().Name}.OnMessageActivityAsync()", label: $"Text: \"{activity.Text}\". Value: {GetObjectAsJsonString(activity.Value)}", cancellationToken: cancellationToken);

            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
            }
            else
            {
                // Call LUIS with the utterance.
                var luisResult = await _luisRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);

                // Create a message showing the LUIS results.
                var sb = new StringBuilder();
                sb.AppendLine($"LUIS results for \"{activity.Text}\":");
                var (intent, intentScore) = luisResult.Intents.FirstOrDefault(x => x.Value.Equals(luisResult.Intents.Values.Max()));
                sb.AppendLine($"Intent: \"{intent}\" Score: {intentScore.Score}");

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(sb.ToString(), inputHint: InputHints.IgnoringInput), cancellationToken);

                // Start a dialog if we recognize the intent.
                switch (luisResult.TopIntent().intent)
                {
                    case FlightBooking.Intent.BookFlight:
                        return await BeginBookFlight(stepContext, cancellationToken);

                    case FlightBooking.Intent.GetWeather:
                        return await BeginGetWeather(stepContext, cancellationToken);

                    default:
                        // Catch all for unhandled intents.
                        var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                        break;
                }
            }

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }
        
        private async Task<DialogTurnResult> OnInvokeActivityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;
            await stepContext.Context.TraceActivityAsync($"{GetType().Name}.OnMessageActivityAsync()", label: $"Text: \"{activity.Text}\". Value: {GetObjectAsJsonString(activity.Value)}", cancellationToken: cancellationToken);

            // Resolve what to execute based on the invoke activity name.
            switch (activity.Name)
            {
                case "InvokeForAStartTest":
                    await stepContext.Context.SendActivityAsync("Maybe here I would start a dialog.", cancellationToken: cancellationToken);
                    var invokeResponseNoEoc = new InvokeResponse { Status = (int)HttpStatusCode.OK, Body = JObject.Parse("{ \"origin\": \"New York\", \"destination\": \"Seattle\"}") };
                    await stepContext.Context.SendActivityAsync(new Activity { Value = invokeResponseNoEoc, Type = ActivityTypesEx.InvokeResponse }, cancellationToken).ConfigureAwait(false);
                    return EndOfTurn;

                case "InvokeWithEoc":
                    var invokeResponse = new InvokeResponse { Status = (int)HttpStatusCode.OK, Body = JObject.Parse("{ \"origin\": \"New York\", \"destination\": \"Seattle\"}") };
                    await stepContext.Context.SendActivityAsync(new Activity { Value = invokeResponse, Type = ActivityTypesEx.InvokeResponse }, cancellationToken).ConfigureAwait(false);
                    return new DialogTurnResult(DialogTurnStatus.Complete);

                default:
                    // We didn't get an event name we can handle.
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Unrecognized InvokeName: \"{activity.Name}\".", inputHint: InputHints.IgnoringInput), cancellationToken);
                    return new DialogTurnResult(DialogTurnStatus.Complete);
            }
        }

        private async Task<DialogTurnResult> BeginGetWeather(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;
            var location = new Location();
            if (activity.Value != null)
            {
                location = JsonConvert.DeserializeObject<Location>(JsonConvert.SerializeObject(activity.Value));
            }

            // We haven't implemented the GetWeatherDialog so we just display a TODO message.
            var getWeatherMessageText = $"TODO: get weather for here (lat: {location.Latitude}, long: {location.Longitude}";
            var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        private async Task<DialogTurnResult> BeginBookFlight(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;
            var bookingDetails = new BookingDetails();
            if (activity.Value != null)
            {
                bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(JsonConvert.SerializeObject(activity.Value));
            }

            // Start the booking dialog.
            var bookingDialog = FindDialog(nameof(BookingDialog));
            return await stepContext.BeginDialogAsync(bookingDialog.Id, bookingDetails, cancellationToken);
        }

        private string GetObjectAsJsonString(object value) => value == null ? string.Empty : JsonConvert.SerializeObject(value);
    }
}
