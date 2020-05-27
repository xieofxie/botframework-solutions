// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using VirtualAssistantSample.Services;

namespace VirtualAssistantSample.Models
{
    public class UserReferenceState
    {
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly IStorage storage;
        private readonly BotSettings botSettings;
        private readonly string key;

        public UserReferenceState(IServiceProvider serviceProvider)
        {
            var adapter = serviceProvider.GetService<BotFrameworkHttpAdapter>();
            this.adapter = adapter;
            storage = serviceProvider.GetService<IStorage>();
            botSettings = serviceProvider.GetService<BotSettings>();
            key = $"{botSettings.MicrosoftAppId}/{nameof(UserReferenceState)}";

            var values = storage.ReadAsync(new string[] { key }).Result;
            values.TryGetValue(key, out object value);
            if (value is Dictionary<string, UserReference> references)
            {
                References = references;
            }
        }

        public Dictionary<string, UserReference> References { get; set; } = new Dictionary<string, UserReference>();

        public UserReference Update(string oldName, string newName, ITurnContext turnContext)
        {
            lock (this)
            {
                References.TryGetValue(oldName == null ? string.Empty : oldName, out UserReference previous);
                References[newName] = new UserReference
                {
                    Reference = turnContext.Activity.GetConversationReference(),
                };

                var changes = new Dictionary<string, object> { { key, References } };
                storage.WriteAsync(changes).Wait();

                return previous;
            }
        }

        public async Task<ResourceResponse> Send(string name, Activity activity, CancellationToken cancellationToken = default)
        {
            UserReference reference = GetLock(name);

            if (reference != null)
            {
                activity.ApplyConversationReference(reference.Reference);

                ResourceResponse response = null;

                if (reference.Reference.ChannelId == Channels.Msteams)
                {
                    // https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages?tabs=dotnet
                    var client = new ConnectorClient(new Uri(activity.ServiceUrl), botSettings.MicrosoftAppId, botSettings.MicrosoftAppPassword);

                    // Post the message to chat conversation with user
                    return await client.Conversations.SendToConversationAsync(activity.Conversation.Id, activity, cancellationToken);
                }
                else
                {
                    // https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=csharp
                    ((BotAdapter)adapter).ContinueConversationAsync(botSettings.MicrosoftAppId, reference.Reference, async (tc, ct) =>
                    {
                        response = await tc.SendActivityAsync(activity);
                    }, cancellationToken).Wait();

                    return response;
                }
            }

            return null;
        }

        public bool StartPollNotification(string name, string token)
        {
            UserReference reference = GetLock(name);
            if (reference != null)
            {
                reference.Cancel();

                reference.CTS = new CancellationTokenSource();
                var cancellationToken = new CancellationToken();
                cancellationToken.Register(() => reference.CTS.Cancel());

                // do not await task - we want this to run in the background and we will cancel it when its done
                var task = Task.Run(() => PollNotification(token, reference.CTS.Token), cancellationToken);
                return true;
            }

            return false;
        }

        public bool StopPollNotification(string name)
        {
            UserReference reference = GetLock(name);
            if (reference != null)
            {
                return reference.Cancel();
            }

            return false;
        }

        private async Task PollNotification(string token, CancellationToken cancellationToken)
        {
            var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(botSettings.MicrosoftAppId));
            client.Credentials = new Octokit.Credentials(token);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = await client.Activity.Notifications.GetAllForCurrent();
                            var limit = client.GetLastApiInfo();
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    // if we happen to cancel when in the delay we will get a TaskCanceledException
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    break;
                }
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        private UserReference GetLock(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            UserReference reference = null;
            lock (this)
            {
                References.TryGetValue(name, out reference);
            }

            return reference;
        }
    }
}
