using Microsoft.Bot.StreamingExtensions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Remote.Protocol
{
    public class RouteContext
    {
        public ReceiveRequest Request { get; set; }

        public dynamic RouteData { get; set; }

        public RouteAction Action { get; set; }
    }
}
