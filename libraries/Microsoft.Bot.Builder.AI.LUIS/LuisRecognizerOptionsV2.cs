﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Options for <see cref="LuisRecognizerOptionsV2"/>.
    /// </summary>
    public class LuisRecognizerOptionsV2 : LuisRecognizerOptions
    {
        /// <summary>
        /// The context label for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceLabel = "Luis Trace";

        /// <summary>
        /// The value type for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizerOptionsV2"/> class.
        /// </summary>
        /// <param name="application">The LUIS application to use to recognize text.</param>
        public LuisRecognizerOptionsV2(LuisApplication application)
        : base(application) 
        {
        }

        /// <summary>
        /// Gets or sets the Luis Prediction Options for the V2 endpoint.
        /// </summary>
        /// <value> This settings will be used to call Luis.</value>
        public LuisPredictionOptions PredictionOptions { get; set; } = new LuisPredictionOptions();

        internal override async Task<RecognizerResult> RecognizeInternalAsync(DialogContext context, HttpClient httpClient, CancellationToken cancellationToken)
            => await RecognizeInternalAsync(context.Context, httpClient, cancellationToken).ConfigureAwait(false);

        internal override async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, HttpClient httpClient, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);

            if (turnContext.Activity == null || turnContext.Activity.Type != ActivityTypes.Message)
            {
                return null;
            }

            var utterance = turnContext.Activity?.AsMessageActivity()?.Text;
            RecognizerResult recognizerResult;
            LuisResult luisResult = null;

            if (string.IsNullOrWhiteSpace(utterance))
            {
                recognizerResult = new RecognizerResult { Text = utterance };
            }
            else
            {
                var credentials = new ApiKeyServiceClientCredentials(Application.EndpointKey);
                using (var runtime = new LUISRuntimeClient(credentials, httpClient, false) { Endpoint = Application.Endpoint })
                {
                    luisResult = await runtime.Prediction.ResolveAsync(
                        Application.ApplicationId,
                        utterance,
                        timezoneOffset: PredictionOptions.TimezoneOffset,
                        verbose: PredictionOptions.IncludeAllIntents,
                        staging: PredictionOptions.Staging,
                        spellCheck: PredictionOptions.SpellCheck,
                        bingSpellCheckSubscriptionKey: PredictionOptions.BingSpellCheckSubscriptionKey,
                        log: PredictionOptions.Log ?? true,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }

                recognizerResult = new RecognizerResult
                {
                    Text = utterance,
                    AlteredText = luisResult.AlteredQuery,
                    Intents = LuisUtil.GetIntents(luisResult),
                    Entities = LuisUtil.ExtractEntitiesAndMetadata(luisResult.Entities, luisResult.CompositeEntities, PredictionOptions.IncludeInstanceData ?? true, utterance),
                };
                LuisUtil.AddProperties(luisResult, recognizerResult);
                if (IncludeAPIResults)
                {
                    recognizerResult.Properties.Add("luisResult", luisResult);
                }
            }

            var traceInfo = JObject.FromObject(
                new
                {
                    recognizerResult,
                    luisModel = new
                    {
                        ModelID = Application.ApplicationId,
                    },
                    luisOptions = PredictionOptions,
                    luisResult,
                });

            await turnContext.TraceActivityAsync("LuisRecognizer", traceInfo, LuisTraceType, LuisTraceLabel, cancellationToken).ConfigureAwait(false);
            return recognizerResult;
        }
    }
}
