using HospitalitySkill.Dialogs.Shared;
using HospitalitySkill.ServiceClients;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalitySkill.Dialogs.CheckIn
{
    public class CheckInDialog : SkillDialogBase
    {
        public CheckInDialog(
          SkillConfigurationBase services,
          ResponseManager responseManager,
          IStatePropertyAccessor<SkillConversationState> conversationStateAccessor,
          IStatePropertyAccessor<SkillUserState> userStateAccessor,
          IServiceManager serviceManager,
          IBotTelemetryClient telemetryClient)
          : base(nameof(CheckInDialog), services, responseManager, conversationStateAccessor, userStateAccessor, serviceManager, telemetryClient)
        {
            var waterfallStepArray = new WaterfallStep[]
            {
                PromptForCard,
                EndDialog
            };

            InitialDialogId = nameof(CheckInDialog);
            AddDialog(new WaterfallDialog(nameof(CheckInDialog), waterfallStepArray));
            AddDialog(new ConfirmPrompt(CheckInDialogResponses.CardNumberPrompt));
        }

        public async Task<DialogTurnResult> PromptForCard(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var resourceResponse = await stepContext.Context.SendActivityAsync(ResponseManager.GetResponse(CheckInDialogResponses.RoomInfoMessage, new StringDictionary() { { "RoomNumber", "405" } }));

            var waterfallStepContext = stepContext;
            var options = new PromptOptions
            {
                Prompt = ResponseManager.GetResponse(CheckInDialogResponses.CardNumberPrompt)
            };
            return await waterfallStepContext.PromptAsync(CheckInDialogResponses.CardNumberPrompt, options);
        }

        public async Task<DialogTurnResult> EndDialog(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var checkInDialog = this;
            if ((bool)stepContext.Result)
            {
                var resourceResponse1 = await stepContext.Context.SendActivityAsync(checkInDialog.ResponseManager.GetResponse(CheckInDialogResponses.CheckInSuccessfulMessage));
            }
            else
            {
                var resourceResponse2 = await stepContext.Context.SendActivityAsync(checkInDialog.ResponseManager.GetResponse(CheckInDialogResponses.CheckInFailedMessage));
            }
            return await stepContext.EndDialogAsync();
        }
    }
}