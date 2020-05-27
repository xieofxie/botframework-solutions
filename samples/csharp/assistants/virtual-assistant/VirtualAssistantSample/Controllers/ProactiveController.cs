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
            var activity = (Activity)Activity.CreateMessageActivity();
            activity.Text = "Proactive Message";
            var response = _userReferenceState.Send(name, activity).Result;
            if (response != null)
            {
                return $"Should send to {name} proactively. {response.Id}";
            }
            else
            {
                return $"{name} not found!";
            }
        }
    }
}
