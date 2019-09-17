// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Solutions;
using Microsoft.Bot.Builder.Solutions.Dialogs;
using Microsoft.Bot.Builder.Solutions.Feedback;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using VirtualAssistantSample.Dialogs;
using VirtualAssistantSample.Models;
using VirtualAssistantSample.Responses.Cancel;
using VirtualAssistantSample.Responses.Main;
using VirtualAssistantSample.Services;
using VirtualAssistantSample.Tests.Mocks;

namespace VirtualAssistantSample.Tests.Mocks
{
    public class MockMainDialog : MainDialog
    {
        private BotSettings _settings;
        private BotServices _services;

        public MockMainDialog(
            BotSettings settings,
            BotServices services,
            OnboardingDialog onboardingDialog,
            EscalateDialog escalateDialog,
            CancelDialog cancelDialog,
            List<SkillDialog> skillDialogs,
            IBotTelemetryClient telemetryClient,
            UserState userState)
            : base(settings, services, onboardingDialog, escalateDialog, cancelDialog, skillDialogs, telemetryClient, userState)
        {
            _settings = settings;
            _services = services;
        }

        protected override async Task RouteAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get cognitive models for locale
            var locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var cognitiveModels = _services.CognitiveModelSets[locale];

            // Check dispatch result
            var dispatchResult = await cognitiveModels.DispatchService.RecognizeAsync<MockDispatchLuis>(dc.Context, CancellationToken.None);
            var intent = dispatchResult.TopIntent().intent;

            // Identify if the dispatch intent matches any Action within a Skill if so, we pass to the appropriate SkillDialog to hand-off
            var identifiedSkill = SkillRouter.IsSkill(_settings.Skills, intent.ToString());

            if (identifiedSkill != null)
            {
                // We have identiifed a skill so initialize the skill connection with the target skill
                var result = await dc.BeginDialogAsync(identifiedSkill.Id);

                if (result.Status == DialogTurnStatus.Complete)
                {
                    await CompleteAsync(dc);
                }
            }
            else
            {
                await base.RouteAsync(dc, cancellationToken);
            }
        }
    }
}
