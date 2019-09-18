// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Newtonsoft.Json;

namespace VirtualAssistantSample.Tests.Mocks
{
    public class MockDispatchLuis : Luis.DispatchLuis
    {
        public enum Intent
        {
            l_General,
            q_Chitchat,
            q_Faq,
            poiSkill,
            emailSkill,
            None
        };
        [JsonProperty("intents")]
        public Dictionary<Intent, IntentScore> Intents;

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<MockDispatchLuis>(JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}
