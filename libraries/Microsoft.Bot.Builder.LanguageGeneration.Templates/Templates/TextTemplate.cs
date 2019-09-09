﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.LanguageGeneration.Templates
{
    /// <summary>
    /// Defines an text Template where the template expression is local aka "inline"
    /// and processed through registered ILanguageGenerator.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    public class TextTemplate : ITemplate<string>
    {
        // Fixed text constructor for inline template
        public TextTemplate(string template)
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Gets or sets the template to evaluate to create the text.
        /// </summary>
        public string Template { get; set; }

        public virtual async Task<string> BindToData(ITurnContext turnContext, object data)
        {
            if (string.IsNullOrEmpty(this.Template))
            {
                throw new ArgumentNullException(nameof(this.Template));
            }

            ILanguageGenerator languageGenerator = turnContext.TurnState.Get<ILanguageGenerator>();
            if (languageGenerator != null)
            {
                var result = await languageGenerator.Generate(
                    turnContext,
                    template: Template,
                    data: data).ConfigureAwait(false);
                return result.ToString();
            }

            return null;
        }

        public override string ToString()
        {
            return $"{nameof(TextTemplate)}({this.Template})";
        }
    }
}
