namespace Microsoft.Bot.Builder.TestBot.Middleware.EventHub
{
    public interface ITranscriptQueue
    {
        void Enqueue(string eventData);

        (bool, string) TryDequeue();

        (bool, string) TryPeek();
    }
}
