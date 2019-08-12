using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Remote.Authentication
{
    public class ProviderTokenResponse
    {
        public OAuthProvider AuthenticationProvider { get; set; }

        public TokenResponse TokenResponse { get; set; }
    }
}
