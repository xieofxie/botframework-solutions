using HospitalitySkill.Dialogs.Shared;
using HospitalitySkill.ServiceClients;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalitySkill.Dialogs.CheckOut
{
    public class CheckOutDialog : SkillDialogBase
    {
        public CheckOutDialog(
          SkillConfigurationBase services,
          ResponseManager responseManager,
          IStatePropertyAccessor<SkillConversationState> conversationStateAccessor,
          IStatePropertyAccessor<SkillUserState> userStateAccessor,
          IServiceManager serviceManager,
          IBotTelemetryClient telemetryClient)
          : base(nameof(CheckOutDialog), services, responseManager, conversationStateAccessor, userStateAccessor, serviceManager, telemetryClient)
        {
            var waterfallStepArray = new WaterfallStep[]
            {
                PromptForEmail,
                EndDialog
            };
            InitialDialogId = nameof(CheckOutDialog);
            AddDialog(new WaterfallDialog(nameof(CheckOutDialog), waterfallStepArray));
            AddDialog(new TextPrompt(CheckOutDialogResponses.EmailPrompt));
        }

        public async Task<DialogTurnResult> PromptForEmail(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var options = new PromptOptions
            {
                Prompt = ResponseManager.GetResponse(CheckOutDialogResponses.EmailPrompt),
                RetryPrompt = ResponseManager.GetResponse(CheckOutDialogResponses.InvalidEmailPrompt)
            };
            return await stepContext.PromptAsync(CheckOutDialogResponses.EmailPrompt, options);
        }

        public async Task<DialogTurnResult> EndDialog(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var result = (string)stepContext.Result;
            await stepContext.Context.SendActivityAsync(ResponseManager.GetResponse(CheckOutDialogResponses.HaveEmailMessage, new StringDictionary(){ { "Email", result } }));
            await stepContext.Context.SendActivityAsync(ResponseManager.GetResponse(CheckOutDialogResponses.CheckOutSuccessful));
            return await stepContext.EndDialogAsync();
        }
    }
}