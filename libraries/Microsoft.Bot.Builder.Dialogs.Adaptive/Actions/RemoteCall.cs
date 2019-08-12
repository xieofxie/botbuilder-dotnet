// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Remote;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Remote.Authentication;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Remote.Models.Manifest;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{

    /// <summary>
    /// Action for HttpRequests
    /// </summary>
    public class RemoteCall : DialogAction
    {
        /// <summary>
        /// Property which is bidirectional property for input and output.  Example: user.age will be passed in, and user.age will be set when the dialog completes
        /// </summary>
        public string Property
        {
            get
            {
                return OutputBinding;
            }

            set
            {
                InputBindings[DialogContextState.DIALOG_VALUE] = value;
                OutputBinding = value;
            }
        }

        public SkillManifest Manifest { get; set; }

        public string MicrosoftAppId { get; set; }

        public string MicrosoftAppPassword { get; set; }

        public List<OAuthConnection> OAuthConnections { get; set; }
    
        public RemoteCall()
        {

        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (dc.Dialogs.Find(Manifest.Id) == null)
            {
                var appCredentials = new MicrosoftAppCredentials(MicrosoftAppId, MicrosoftAppPassword);
                var telemetryClient = new BotTelemetryClient(new Microsoft.ApplicationInsights.TelemetryClient());
                var credentials = new MicrosoftAppCredentialsEx(MicrosoftAppId, MicrosoftAppPassword, Manifest.MSAappId);
                var authDialog = BuildAuthDialog(Manifest, appCredentials);

                dc.Dialogs.Add(new SkillDialog(Manifest, credentials, telemetryClient, new UserState(new MemoryStorage()), authDialog));
            }


            return await dc.BeginDialogAsync(Manifest.Id);
        }

        private MultiProviderAuthDialog BuildAuthDialog(SkillManifest skill, MicrosoftAppCredentials appCredentials)
        {
            if (skill.AuthenticationConnections?.Count() > 0)
            {
                if (OAuthConnections != null && OAuthConnections.Any(o => skill.AuthenticationConnections.Any(s => s.ServiceProviderId == o.Provider)))
                {
                    var oauthConnections = OAuthConnections.Where(o => skill.AuthenticationConnections.Any(s => s.ServiceProviderId == o.Provider)).ToList();
                    return new MultiProviderAuthDialog(oauthConnections, appCredentials);
                }
                else
                {
                    throw new Exception($"You must configure at least one supported OAuth connection to use this skill: {skill.Name}.");
                }
            }

            return null;
        }

    }
}
