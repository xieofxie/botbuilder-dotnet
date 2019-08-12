namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Remote.Protocol
{
    public class RouteTemplate
    {
        public string Method { get; set; }

        public string Path { get; set; }

        public RouteAction Action { get; set; }
    }
}
