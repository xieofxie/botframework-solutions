using HospitalitySkill.Dialogs.Shared;
using HospitalitySkill.ServiceClients;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalitySkill.Dialogs.RequestServices
{
    public class RequestServicesDialog : SkillDialogBase
    {
        public RequestServicesDialog(
          SkillConfigurationBase services,
          ResponseManager responseManager,
          IStatePropertyAccessor<SkillConversationState> conversationStateAccessor,
          IStatePropertyAccessor<SkillUserState> userStateAccessor,
          IServiceManager serviceManager,
          IBotTelemetryClient telemetryClient)
          : base(nameof(RequestServicesDialog), services, responseManager, conversationStateAccessor, userStateAccessor, serviceManager, telemetryClient)
        {
            var waterfallStepArray = new WaterfallStep[]
            {
                HandleRequest,
                EndDialog
            };

            InitialDialogId = nameof(RequestServicesDialog);
            AddDialog(new WaterfallDialog(nameof(RequestServicesDialog), waterfallStepArray));
        }

        public async Task<DialogTurnResult> HandleRequest(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var requestServiceDialog = this;
            var resourceResponse = await stepContext.Context.SendActivityAsync(requestServiceDialog.ResponseManager.GetResponse(RequestServicesDialogResponses.RequestSent, new StringDictionary() { { "Item", "towels" } }));
            return await stepContext.NextAsync();
        }

        public async Task<DialogTurnResult> EndDialog(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }
    }
}
