using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Bot.Builder.TestBot.Middleware.EventHub
{
    public interface IEventHubSender
    {
        Task SendAsync(EventDataBatch events);

        EventDataBatch CreateBatch();
    }
}
