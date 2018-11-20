// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Model.Proactive;
using Microsoft.Bot.Solutions.Skills;

namespace CalendarSkill
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class CalendarSkill : IBot
    {
        private readonly EndpointService _endpointService;
        private readonly SkillConfiguration _services;
        private readonly UserState _userState;
        private readonly ProactiveState _proactiveState;
        private readonly ConversationState _conversationState;
        private readonly IServiceManager _serviceManager;
        private DialogSet _dialogs;
        private bool _skillMode;

        public CalendarSkill(EndpointService endpointService, SkillConfiguration services, ConversationState conversationState, UserState userState, ProactiveState proactiveState, IServiceManager serviceManager = null, bool skillMode = false)
        {
            _endpointService = endpointService;
            _skillMode = skillMode;
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _proactiveState = proactiveState ?? throw new ArgumentNullException(nameof(proactiveState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _serviceManager = serviceManager ?? new ServiceManager(_services);

            _dialogs = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(DialogState)));
            _dialogs.Add(new MainDialog(_endpointService, _services, _conversationState, _userState, _proactiveState, _serviceManager, _skillMode));
        }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dc = await _dialogs.CreateContextAsync(turnContext);
            var result = await dc.ContinueDialogAsync();

            if (result.Status == DialogTurnStatus.Empty)
            {
                if (!_skillMode)
                {
                    // if localMode, check for conversation update from user before starting dialog
                    if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
                    {
                        var activity = turnContext.Activity.AsConversationUpdateActivity();

                        // if conversation update is not from the bot.
                        if (!activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
                        {
                            await dc.BeginDialogAsync(nameof(MainDialog));
                        }
                    }
                }
                else
                {
                    // if skillMode, begin dialog
                    await dc.BeginDialogAsync(nameof(MainDialog));
                }
            }
        }
    }
}