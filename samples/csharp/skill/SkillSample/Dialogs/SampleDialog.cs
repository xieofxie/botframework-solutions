// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace SkillSample.Dialogs
{
    public class SampleDialog : SkillDialogBase
    {
        public SampleDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(SampleDialog), serviceProvider, telemetryClient)
        {
            var sample = new WaterfallStep[]
            {
                // NOTE: Uncomment these lines to include authentication steps to this dialog
                // GetAuthToken,
                // AfterGetAuthToken,
                PromptForName,
                GreetUser,
                End,
            };

            AddDialog(new WaterfallDialog(nameof(SampleDialog), sample));
            AddDialog(new TextPrompt(DialogIds.NamePrompt));
            AddDialog(new NumberPrompt<int>(DialogIds.TestPrompt));

            InitialDialogId = nameof(SampleDialog);
        }

        private async Task<DialogTurnResult> PromptForName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // NOTE: Uncomment the following lines to access LUIS result for this turn.
            // var luisResult = stepContext.Context.TurnState.Get<LuisResult>(StateProperties.SkillLuisResult);
            var prompt = TemplateEngine.GenerateActivityForLocale("TestPrompt");
            return await stepContext.PromptAsync(DialogIds.TestPrompt, new PromptOptions { Prompt = prompt });
        }

        private async Task<DialogTurnResult> GreetUser(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            int index = (int)stepContext.Result;
            var text = "Do you want to see tomorrow's schedule?";
            Activity prompt = null;
            if (index == 1)
            {
                prompt = TemplateEngine.GenerateActivityForLocale("MessageWithCard", CreateCardInput(stepContext.Context, "SelectAction"));
            }
            else if (index == 2)
            {
                prompt = TemplateEngine.GenerateActivityForLocale("SuggestAction");
            }
            else if (index == 3)
            {
                prompt = TemplateEngine.GenerateActivityForLocale("HeroCardForSuggestAction");
            }
            else if (index == 4)
            {
                prompt = TemplateEngine.GenerateActivityForLocale("MessageWithCard", CreateCardInput(stepContext.Context, "Action", text));
            }
            else if (index == 5)
            {
                prompt = TemplateEngine.GenerateActivityForLocale("MessageWithCard", CreateCardInput(stepContext.Context, "ActionSet", text));
            }
            else if (index == 6)
            {
                prompt = TemplateEngine.GenerateActivityForLocale("MessageWithCard", CreateCardInput(stepContext.Context, "ToggleChoiceSet"));
            }
            else if (index == 7)
            {
                prompt = TemplateEngine.GenerateActivityForLocale("MessageWithCard", CreateCardInput(stepContext.Context, "ToggleVisibility"));
            }

            await stepContext.Context.SendActivityAsync(prompt);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.ReplaceDialogAsync(nameof(SampleDialog));
        }

        private object CreateCardInput(ITurnContext context, string name, string text = "")
        {
            if (context.Activity.ChannelId == Channels.Msteams)
            {
                return new { Text = text, Card = name + ".Teams.json" };
            }
            else
            {
                return new { Text = text, Card = name + ".json" };
            }
        }

        private class DialogIds
        {
            public const string NamePrompt = "namePrompt";
            public const string TestPrompt = "testPrompt";
        }
    }
}
