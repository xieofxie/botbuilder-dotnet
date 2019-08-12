using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Remote
{
    /// <summary>
    /// Interface that represents remove invocation behavior.
    /// </summary>
    public interface IRemoteUserTokenProvider
    {
        Task SendRemoteTokenRequestEventAsync(ITurnContext turnContext, CancellationToken cancellationToken);
    }
}
