using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.PICL.Utilities.Serializers;
using Picl.Core.Items;
using Picl.Core.Labels;
using Picl.CrfTrainers;
using Picl.Serialization;
using Picl.Standard.Models;
using Picl.TextItems.Items;
using Picl.TextItems.Mappers;

namespace Microsoft.Bot.Builder.TestBot.Json.Recognizers
{
    public class PiclRecognizer : IRecognizer
    {
        private PackagedPredictionModel<string, EntireItem, string, EntireItem, HcModeledItem> predictionModel;

        public string Model { get; set; }

        public PiclRecognizer()
        {
        }

        public PiclRecognizer(string model) : this()
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var result = new RecognizerResult();
            var activity = turnContext.Activity;

            if (activity.Type == ActivityTypes.Message)
            {
                result.Text = HackItUp(activity.Text);

                // Ensure model has been loaded. Lazy and cached loading, so will be a no-op 
                // most of the times
                LoadModel();

                // Call PICL model
                result.Intents = RunClassificationModel(result.Text);
            }

            return Task.FromResult(result);
        }

        private string HackItUp(string text)
        {
            if (text.Contains("quote", StringComparison.OrdinalIgnoreCase))
            {
                return "pirate";
            }

            return text;
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            throw new NotImplementedException();
        }

        private void LoadModel()
        {
            if (predictionModel == null)
            {
                if (string.IsNullOrEmpty(Model))
                {
                    throw new ArgumentNullException(nameof(Model));
                }

                var contents = File.ReadAllText(Model);
                var serializer = new Picl.Serialization.PiclSerializer();
                predictionModel = serializer.Deserialize<PackagedPredictionModel<string, EntireItem, string, EntireItem, HcModeledItem>>(contents);
            }
        }

        private Dictionary<string, IntentScore> RunClassificationModel(string example)
        {
            // Run model on example and return a "modeled item" which provides prediction functions.
            var modeledItem = predictionModel.Transform(example);

            var schema = predictionModel.Schema;

            var prediction = predictionModel.GetPrediction(example);
            var mostSpecificPrediction = prediction.Predictions.Last();
            var conceptId = mostSpecificPrediction.ConceptId;
            var conceptName = schema.Concepts[conceptId];

            var intentProbabilities = new Dictionary<string, IntentScore>();
            intentProbabilities.Add(conceptName.Name, new IntentScore() { Score = 1.0 });

            // Use modeled item to calculate per-concept probabilities for the example.
            
            foreach (var concept in schema.Concepts)
            {
                // Calculate the probability the example "is" this concept and display result.
                var isConceptLabel = new LabelList<EntireItem, NoBoundary>(new[]
                {
                    new Label<EntireItem, NoBoundary>(LabelKind.Is, concept.Key)
                });

                var probabilityIsConcept = modeledItem.GetProbability(isConceptLabel);

                //intentProbabilities.Add(concept.Value.Name, new IntentScore() { Score = probabilityIsConcept });

                Console.WriteLine($"{concept.Value.Name}: {probabilityIsConcept * 100:0.0}%");
            }

            return intentProbabilities;
        }
    }
}
