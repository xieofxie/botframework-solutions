// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Solutions.Models.Proactive;
using Microsoft.Bot.Solutions.Skills;
using ToDoSkill.ServiceClients;
using static ToDoSkill.ServiceProviderTypes;

namespace ToDoSkill
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class ToDoSkill : IBot
    {
        private readonly ISkillConfiguration _services;
        private readonly EndpointService _endpointService;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly ProactiveState _proactiveState;
        private readonly IBotTelemetryClient _telemetryClient;
        private ITaskService _serviceManager;
        private DialogSet _dialogs;
        private bool _skillMode;

        public ToDoSkill(ISkillConfiguration services, EndpointService endpointService, ConversationState conversationState, UserState userState, ProactiveState proactiveState, IBotTelemetryClient telemetryClient, ITaskService serviceManager = null, bool skillMode = false)
        {
            _skillMode = skillMode;
            _endpointService = endpointService ?? throw new ArgumentNullException(nameof(endpointService));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _proactiveState = proactiveState ?? throw new ArgumentNullException(nameof(proactiveState));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));

            var isOutlookProvider = _services.Properties.ContainsKey("TaskServiceProvider")
                && _services.Properties["TaskServiceProvider"].ToString().Equals(ProviderTypes.Outlook.ToString(), StringComparison.InvariantCultureIgnoreCase);
            ITaskService taskService = new OneNoteService();
            if (isOutlookProvider)
            {
                taskService = new OutlookService();
            }

            _serviceManager = serviceManager ?? taskService;

            _dialogs = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(DialogState)));
            _dialogs.Add(new MainDialog(_services, _conversationState, _userState, _telemetryClient, _serviceManager, _skillMode));
        }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="TaskItem"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dc = await _dialogs.CreateContextAsync(turnContext);

            if (dc.ActiveDialog != null)
            {
                var result = await dc.ContinueDialogAsync();
            }
            else
            {
                await dc.BeginDialogAsync(nameof(MainDialog));
            }
        }
    }
}