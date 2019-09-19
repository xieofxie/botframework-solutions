// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace VirtualAssistantSample.Bots
{
    public class DialogBot<T> : IBot
        where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly IBotTelemetryClient _telemetryClient;

        public DialogBot(IServiceProvider serviceProvider, T dialog)
        {
            _dialog = dialog;
            _conversationState = serviceProvider.GetService<ConversationState>();
            _userState = serviceProvider.GetService<UserState>();
            _telemetryClient = serviceProvider.GetService<IBotTelemetryClient>();
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Client notifying this bot took to long to respond (timed out)
            if (turnContext.Activity.Code == EndOfConversationCodes.BotTimedOut)
            {
                _telemetryClient.TrackTrace($"Timeout in {turnContext.Activity.ChannelId} channel: Bot took too long to respond.", Severity.Information, null);
                return;
            }

            do
            {
                Activity activity = turnContext.Activity;

                // Case 0. Not message
                if (activity.Type != ActivityTypes.Message || string.IsNullOrEmpty(activity.Text))
                {
                    break;
                }

                // Case 1. No entities.
                if (activity.Entities == null || activity.Entities.Count == 0)
                {
                    break;
                }

                IEnumerable<Entity> mentionEntities = activity.Entities.Where(entity => entity.Type.Equals("mention", StringComparison.OrdinalIgnoreCase));

                // Case 2. No Mention entities.
                if (!mentionEntities.Any())
                {
                    break;
                }

                // Case 3. Mention entities.
                string strippedText = activity.Text;

                mentionEntities.ToList()
                    .ForEach(entity =>
                    {
                        strippedText = strippedText.Replace(entity.GetAs<Mention>().Text, string.Empty);
                    });

                activity.Text = strippedText.Trim();
            } while (false);

            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}