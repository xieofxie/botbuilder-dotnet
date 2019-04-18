using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.TestBot.Middleware.EventHub
{
    internal static class EventHubHelper
    {
        public const string EventHubConnectionString = "eventHub.connectionString";
        public const string EventHubName = "eventHub.name";

        public static EventHubClient GetEventHubClient(IConfiguration eventHubConfig)
        {
            var eventHubConnectionString = eventHubConfig[EventHubConnectionString];
            var eventHubName = eventHubConfig[EventHubName];
            var connStrBuilder = new EventHubsConnectionStringBuilder(eventHubConnectionString)
            {
                EntityPath = eventHubName,
            };

            var eventHubClient = EventHubClient.CreateFromConnectionString(connStrBuilder.ToString());
            return eventHubClient;
        }
    }
}
