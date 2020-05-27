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
using Microsoft.Bot.Builder.Teams;
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
            storage = null; // serviceProvider.GetService<IStorage>();
            botSettings = serviceProvider.GetService<BotSettings>();
            key = $"{botSettings.MicrosoftAppId}/{nameof(UserReferenceState)}";

            if (storage != null)
            {
                var values = storage.ReadAsync(new string[] { key }).Result;
                values.TryGetValue(key, out object value);
                if (value is Dictionary<string, UserReference> references)
                {
                    References = references;
                }
            }
        }

        public Dictionary<string, UserReference> References { get; set; } = new Dictionary<string, UserReference>();

        public bool Update(ITurnContext turnContext)
        {
            lock (this)
            {
                var name = GetName(turnContext);
                References.TryGetValue(name, out UserReference previous);
                if (previous == null)
                {
                    References.Add(name, new UserReference(turnContext));
                }
                else
                {
                    previous.Update(turnContext);
                }

                if (storage != null)
                {
                    var changes = new Dictionary<string, object> { { key, References } };
                    storage.WriteAsync(changes).Wait();
                }

                return previous == null;
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

        public bool StartPollNotification(ITurnContext turnContext, string token)
        {
            var name = GetName(turnContext);
            UserReference reference = GetLock(name);
            if (reference != null)
            {
                reference.Cancel();

                reference.CTS = new CancellationTokenSource();

                // do not await task - we want this to run in the background and we will cancel it when its done
                var task = Task.Run(() => PollNotification(name, token, reference.CTS), reference.CTS.Token);
                return true;
            }

            return false;
        }

        public bool StopPollNotification(ITurnContext turnContext)
        {
            var name = GetName(turnContext);
            UserReference reference = GetLock(name);
            if (reference != null)
            {
                return reference.Cancel();
            }

            return false;
        }

        private async Task PollNotification(string name, string token, CancellationTokenSource CTS)
        {
            var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(botSettings.MicrosoftAppId));
            client.Credentials = new Octokit.Credentials(token);
            string lastETag = string.Empty;

            try
            {
                while (!CTS.Token.IsCancellationRequested)
                {
                    int delayInMill = 100 * 1000;
                    try
                    {
                        var results = await client.Activity.Notifications.GetAllForCurrent();
                        var info = client.GetLastApiInfo();

                        // TODO: this doesn't change. Use a naive etag.
                        var etag = info.Etag;

                        etag = results.Count.ToString();
                        foreach (var result in results)
                        {
                            etag += result.Id.Length > 4 ? result.Id.Substring(result.Id.Length - 4) : result.Id;
                        }

                        if (results.Count > 0 && etag != lastETag)
                        {
                            var activity = (Activity)Activity.CreateMessageActivity();
                            activity.Text = $"You got {results.Count} unread notification(s).";
                            await Send(name, activity);
                            lastETag = etag;
                        }

                        if (info.RateLimit != null)
                        {
                            delayInMill = Math.Max(delayInMill, 3600 * 1000 / info.RateLimit.Limit);
                        }
                    }
                    catch (Exception ex)
                    {
                        var activity = (Activity)Activity.CreateMessageActivity();
                        activity.Text = ex.Message;
                        await Send(name, activity);
                        activity = (Activity)Activity.CreateMessageActivity();
                        activity.Text = "Please start notification again";
                        await Send(name, activity);
                        CTS.Cancel();
                        break;
                    }

                    // if we happen to cancel when in the delay we will get a TaskCanceledException
                    await Task.Delay(delayInMill, CTS.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        private string GetName(ITurnContext turnContext)
        {
            return turnContext.Activity.From.Id;
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
