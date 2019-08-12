using System;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Remote.Protocol
{
    public class RouteAction
    {
        public Func<ReceiveRequest, dynamic, Task<object>> Action { get; set; }
    }
}
