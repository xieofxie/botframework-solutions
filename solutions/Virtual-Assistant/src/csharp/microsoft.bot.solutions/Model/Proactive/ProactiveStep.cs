using System.Collections.Generic;
using Microsoft.Bot.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Solutions.Model.Proactive
{
    public class ProactiveStep : ConnectedService
    {
        public ProactiveStep()
            : base("proactiveStep")
        {
        }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("action")]
        public ProactiveNextStepActionType Action { get; set; }

        [JsonProperty("skillId")]
        public string SkillId { get; set; }

        [JsonProperty("parameters")]
        public Dictionary<string, string> Parameters { get; set; }
    }
}