// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;
using VirtualAssistantSample.Services;

namespace VirtualAssistantSample.Utilities
{
    public class AppState : BotState
    {
        private string appId;

        public AppState(IStorage storage, IServiceProvider serviceProvider)
            : base(storage, nameof(AppState))
        {
            var botSettings = serviceProvider.GetService<BotSettings>();
            appId = botSettings.MicrosoftAppId;
        }

        protected override string GetStorageKey(ITurnContext turnContext)
        {
            return appId;
        }
    }
}
