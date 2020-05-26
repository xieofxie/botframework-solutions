// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.DependencyInjection;
using VirtualAssistantSample.Models;
using VirtualAssistantSample.Services;

namespace VirtualAssistantSample.Controllers
{
    public class ProactiveController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly BotSettings _botSettings;
        private readonly UserReferenceState _userReferenceState;

        public ProactiveController(IServiceProvider serviceProvider)
        {
            _botSettings = serviceProvider.GetService<BotSettings>();
            var adapter = serviceProvider.GetService<BotFrameworkHttpAdapter>();
            _adapter = adapter;
            _userReferenceState = serviceProvider.GetService<UserReferenceState>();
        }

        [HttpGet, Route("proactive")]
        public string Send(string name)
        {
            name = name == null ? string.Empty : name;
            if (_userReferenceState.References.TryGetValue(name, out UserReference reference))
            {
                var activity = (Activity)Activity.CreateMessageActivity();
                activity.Text = "Proactive Message";
                activity.ApplyConversationReference(reference.Reference);

                ResourceResponse response = null;

                if (reference.Reference.ChannelId == Channels.Msteams)
                {
                    // https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages?tabs=dotnet
                    var client = new ConnectorClient(new Uri(activity.ServiceUrl), _botSettings.MicrosoftAppId, _botSettings.MicrosoftAppPassword);

                    // Post the message to chat conversation with user
                    response = client.Conversations.SendToConversationAsync(activity.Conversation.Id, activity).Result;
                }
                else
                {
                    // https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=csharp
                    ((BotAdapter)_adapter).ContinueConversationAsync(_botSettings.MicrosoftAppId, reference.Reference, async (tc, ct) =>
                    {
                        response = await tc.SendActivityAsync(activity);
                    }, default(CancellationToken)).Wait();
                }

                return $"Should send to {name} proactively. {response?.Id}";
            }
            else
            {
                return $"{name} not found!";
            }
        }
    }
}
