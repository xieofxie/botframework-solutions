// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Solutions.StorageManager
{
    public class ActivityOnlyTurnContext : ITurnContext
    {
        public BotAdapter Adapter => throw new NotImplementedException();

        public TurnContextStateCollection TurnState => throw new NotImplementedException();

        public Activity Activity { get; set; } = new Activity();

        public bool Responded => throw new NotImplementedException();

        public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
