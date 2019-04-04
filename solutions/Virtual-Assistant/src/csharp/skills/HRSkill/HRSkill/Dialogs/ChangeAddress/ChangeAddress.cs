using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Solutions.Extensions;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Builder.Solutions.Skills;
using HRSkill.Dialogs.ChangeAddress.Resources;
using HRSkill.Dialogs.Shared;
using HRSkill.ServiceClients;

namespace HRSkill.Dialogs.ChangeAddress
{
    public class ChangeAddressDialog : SkillDialogBase
    {
        public ChangeAddressDialog(
            SkillConfigurationBase services,
            ResponseManager responseManager,
            IStatePropertyAccessor<SkillConversationState> conversationStateAccessor,
            IStatePropertyAccessor<SkillUserState> userStateAccessor,
            IServiceManager serviceManager,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ChangeAddressDialog), services, responseManager, conversationStateAccessor, userStateAccessor, serviceManager, telemetryClient)
        {
            var sample = new WaterfallStep[]
            {
                // NOTE: Uncomment these lines to include authentication steps to this dialog
                // GetAuthToken,
                // AfterGetAuthToken,
                PromptForNewAddress,
                PromptForNotification,
                End,
            };

            AddDialog(new WaterfallDialog(nameof(ChangeAddress), sample));
            AddDialog(new TextPrompt(DialogIds.NamePrompt));
            AddDialog(new ConfirmPrompt(DialogIds.NotificationPrompt));

            InitialDialogId = nameof(ChangeAddress);
        }

        private async Task<DialogTurnResult> PromptForNewAddress(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // NOTE: Uncomment the following lines to access LUIS result for this turn.
            // var state = await ConversationStateAccessor.GetAsync(stepContext.Context);
            // var intent = state.LuisResult.TopIntent().intent;
            // var entities = state.LuisResult.Entities;

            await stepContext.Context.SendActivityAsync("Sure, I can help you with changing your address. Just checking your HR profile.");
            await stepContext.Context.SendActivityAsync("Okay. Here is your current address in Workday: 1 Coca Cola Plz NW, Atlanta 30313.");

            var prompt = ResponseManager.GetResponse(ChangeAddressResponses.NewAddressPrompt);
            return await stepContext.PromptAsync(DialogIds.NamePrompt, new PromptOptions { Prompt = prompt});
        }

        private async Task<DialogTurnResult> PromptForNotification(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var tokens = new StringDictionary
            //{
            //    { "Name", stepContext.Result.ToString() },
            //};


            var prompt = ResponseManager.GetResponse(ChangeAddressResponses.NotificationPrompt);
            return await stepContext.PromptAsync(DialogIds.NotificationPrompt, new PromptOptions { Prompt = prompt });

            //var response = ResponseManager.GetResponse(ChangeAddressResponses.HaveNameMessage);
            //await stepContext.Context.SendActivityAsync(response);

            //return await stepContext.NextAsync();
        }

        private Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string NamePrompt = "namePrompt";
            public const string NotificationPrompt = "notificationPrompt";
        }
    }
}
