using HospitalitySkill.Dialogs.Shared;
using HospitalitySkill.ServiceClients;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalitySkill.Dialogs.OrderFood
{
    public class OrderFoodDialog : SkillDialogBase
    {
        public OrderFoodDialog(
          SkillConfigurationBase services,
          ResponseManager responseManager,
          IStatePropertyAccessor<SkillConversationState> conversationStateAccessor,
          IStatePropertyAccessor<SkillUserState> userStateAccessor,
          IServiceManager serviceManager,
          IBotTelemetryClient telemetryClient)
          : base(nameof(OrderFoodDialog), services, responseManager, conversationStateAccessor, userStateAccessor, serviceManager, telemetryClient)
        {
            var waterfallStepArray = new WaterfallStep[]
            {
                ShowMenu,
                PromptForOrder,
                PromptForTime,
                EndDialog
            };
            InitialDialogId = nameof(OrderFoodDialog);
            AddDialog(new WaterfallDialog(nameof(OrderFoodDialog), waterfallStepArray));
            AddDialog(new TextPrompt(OrderFoodDialogResponses.PromptForOrder));
            AddDialog(new DateTimePrompt(OrderFoodDialogResponses.PromptForTime, new PromptValidator<IList<DateTimeResolution>>(DeliveryTimeValidator)));
        }

        public async Task<DialogTurnResult> ShowMenu(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var resourceResponse = await stepContext.Context.SendActivityAsync(ResponseManager.GetResponse(OrderFoodDialogResponses.ShowFoodOptionsMessage));
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> PromptForOrder(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var waterfallStepContext = stepContext;
            var options = new PromptOptions
            {
                Prompt = ResponseManager.GetResponse(OrderFoodDialogResponses.PromptForOrder, null)
            };
            return await waterfallStepContext.PromptAsync(nameof(PromptForOrder), options);
        }

        private async Task<DialogTurnResult> PromptForTime(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            stepContext.Values.Add("order", (string)stepContext.Result);
            var waterfallStepContext = stepContext;
            var options = new PromptOptions
            {
                Prompt = ResponseManager.GetResponse(nameof(PromptForTime))
            };
            return await waterfallStepContext.PromptAsync(nameof(PromptForTime), options);
        }

        private Task<bool> DeliveryTimeValidator(
          PromptValidatorContext<IList<DateTimeResolution>> promptContext,
          CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text.ToLower() == "asap" || promptContext.Context.Activity.Text.ToLower() == "as soon as possible")
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(promptContext.Recognized.Succeeded);
        }

        public async Task<DialogTurnResult> EndDialog(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var naturalLanguage = new TimexProperty(((List<DateTimeResolution>)stepContext.Result)[0].Timex).ToNaturalLanguage(DateTime.Today);
            if (string.IsNullOrEmpty(naturalLanguage))
            {
                var dateTime = DateTime.Now;
                dateTime = dateTime.AddMinutes(30.0);
                naturalLanguage = dateTime.ToString("HH:mmtt");
            }

            await stepContext.Context.SendActivityAsync(ResponseManager.GetResponse(OrderFoodDialogResponses.OrderPlacedMessage, new StringDictionary()
            {
                { "Order", (string) stepContext.Values["order"] },
                { "Time", naturalLanguage }
            }));

            return await stepContext.EndDialogAsync();
        }
    }
}