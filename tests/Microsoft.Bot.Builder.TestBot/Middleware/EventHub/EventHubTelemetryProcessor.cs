using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.TestBot.Middleware.Telemetry;

namespace Microsoft.Bot.Builder.TestBot.Middleware.EventHub
{
    public class EventHubTelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor _next;
        private ITranscriptQueue _queue;

        public EventHubTelemetryProcessor(ITelemetryProcessor next, ITranscriptQueue queue)
        {
            _queue = queue;

            // Next TelemetryProcessor in the chain
            _next = next;
        }

        public void Process(ITelemetry item)
        {
            var hasTrans = TelemetryHelper.HasTranscript(item);
            if (hasTrans)
            {
                var transItem = TelemetryHelper.GetTranscript(item);

                // Uncomment the following line to view the transcript activity in debugger.
                Console.WriteLine($"eventhub: {transItem}");

                // Queue this for sending to event hub
                _queue.Enqueue(transItem);
            }

            // Remove the original transcript activity for Application Insights.
            var newItem = hasTrans ? TelemetryHelper.RemoveTranscriptForEvent(item) : item;

            // Send the item to the next TelemetryProcessor
            _next.Process(newItem);
        }
    }
}
