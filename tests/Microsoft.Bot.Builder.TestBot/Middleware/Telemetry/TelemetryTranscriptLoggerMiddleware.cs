using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Activity = Microsoft.Bot.Schema.Activity;

namespace Microsoft.Bot.Builder.TestBot.Middleware.Telemetry
{
    public class TelemetryTranscriptLoggerMiddleware : TelemetryLoggerMiddleware, ITranscriptLogger
    {
        public const string LogPersonalInformationFlag = "telemetry.logUserName";

        private string _eventName = string.Empty;

        private Dictionary<string, string> _properties = new Dictionary<string, string>();

        private readonly IBotTelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryTranscriptLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="telemetryClient">The IBotTelemetryClient that logs to Application Insights.</param>
        /// <param name="configuration"> The IConfiguration to read a few options, e.g., `LogUserName`.</param>
        public TelemetryTranscriptLoggerMiddleware(IBotTelemetryClient telemetryClient, IConfiguration configuration)
            : base(telemetryClient, configuration.GetValue<bool>(LogPersonalInformationFlag))
        {
        }

        public Task LogActivityAsync(IActivity activity)
        {
            _properties[TelemetryConstants.Transcript] = ActivityHelper.ToJson(activity);
            TelemetryClient.TrackEvent(_eventName, _properties);
            return Task.CompletedTask;
        }

        protected override async Task OnReceiveActivityAsync(Activity activity, CancellationToken cancellation)
        {
            if (activity == null)
            {
                return;
            }

            if (activity.From == null)
            {
                activity.From = new ChannelAccount();
            }

            if (string.IsNullOrEmpty((string)activity.From.Properties["role"]))
            {
                activity.From.Properties["role"] = "user";
            }

            _properties = await FillReceiveEventPropertiesAsync(activity).ConfigureAwait(false);
            _eventName = TelemetryLoggerConstants.BotMsgReceiveEvent;
            await LogActivityAsync(activity);
        }

        protected override async Task OnSendActivityAsync(Activity activity, CancellationToken cancellation)
        {
            _properties = await FillSendEventPropertiesAsync(activity).ConfigureAwait(false);
            _eventName = TelemetryLoggerConstants.BotMsgSendEvent;
            await LogActivityAsync(activity);
        }

        protected override async Task OnUpdateActivityAsync(Activity activity, CancellationToken cancellation)
        {
            var updateActivity = ActivityHelper.Clone(activity);
            updateActivity.Type = ActivityTypes.MessageUpdate;
            _properties = await FillUpdateEventPropertiesAsync(updateActivity).ConfigureAwait(false);
            _eventName = TelemetryLoggerConstants.BotMsgUpdateEvent;
            await LogActivityAsync(updateActivity);
        }

        protected override async Task OnDeleteActivityAsync(Activity activity, CancellationToken cancellation)
        {
            _properties = await FillDeleteEventPropertiesAsync(activity).ConfigureAwait(false);
            _eventName = TelemetryLoggerConstants.BotMsgDeleteEvent;
            await LogActivityAsync(activity);
        }
    }
}
