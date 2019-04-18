using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.TestBot.Middleware.Telemetry
{
    public class LuisTelemetryConstants
    {
        public const string IntentPrefix = "LuisIntent";  // Application Insights Custom Event name (with Intent)

        public const string IntentProperty = "intent";
        public const string IntentScoreProperty = "intentScore";
        public const string QuestionProperty = "question";
        public const string SentimentLabelProperty = "sentimentLabel";
        public const string SentimentScoreProperty = "sentimentScore";
        public const string DialogId = "dialogId";
    }
}
