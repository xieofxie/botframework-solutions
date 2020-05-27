// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace VirtualAssistantSample.Models
{
    public class UserReference
    {
        [JsonConstructor]
        public UserReference()
        {
        }

        public UserReference(ITurnContext turnContext)
        {
            Update(turnContext);
        }

        [JsonIgnore]
        public CancellationTokenSource CTS { get; set; }

        public ConversationReference Reference { get; set; }

        public bool Cancel()
        {
            if (CTS != null && !CTS.IsCancellationRequested)
            {
                CTS.Cancel();
                return true;
            }

            return false;
        }

        public void Update(ITurnContext turnContext)
        {
            Reference = turnContext.Activity.GetConversationReference();
        }
    }
}
