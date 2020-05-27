// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace VirtualAssistantSample.Models
{
    public class UserReference
    {
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
    }
}
