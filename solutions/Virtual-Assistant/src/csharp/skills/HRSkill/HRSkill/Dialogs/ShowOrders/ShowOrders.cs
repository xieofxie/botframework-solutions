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
using HRSkill.Dialogs.ShowOrders.Resources;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace HRSkill.Dialogs.ShowOrders
{
    public class ShowOrdersDialog : SkillDialogBase
    {
        public ShowOrdersDialog(
            SkillConfigurationBase services,
            ResponseManager responseManager,
            IStatePropertyAccessor<SkillConversationState> conversationStateAccessor,
            IStatePropertyAccessor<SkillUserState> userStateAccessor,
            IServiceManager serviceManager,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ShowOrdersDialog), services, responseManager, conversationStateAccessor, userStateAccessor, serviceManager, telemetryClient)
        {
            var sample = new WaterfallStep[]
            {
                // NOTE: Uncomment these lines to include authentication steps to this dialog
                // GetAuthToken,
                // AfterGetAuthToken,
                ShowOrders,
                ContactCustomer,
                NewProducts,
                CheckEquipment,
                CheckContact,
                Contact,
                End
            };

            AddDialog(new WaterfallDialog(nameof(ShowOrdersDialog), sample));
            AddDialog(new ChoicePrompt(DialogIds.DelayedOrderPrompt, DelayedOrderPromptValidator));
            AddDialog(new ChoicePrompt(DialogIds.ContactCustomerPrompt, ContactCustomerPromptValidator));
            AddDialog(new ChoicePrompt(DialogIds.NewProductsPrompt, NewProductsPromptValidator) { Style = ListStyle.SuggestedAction, ChoiceOptions = new ChoiceFactoryOptions { IncludeNumbers = false } });
            AddDialog(new ConfirmPrompt(DialogIds.ContactCustomerConfirm, ContactCustomerConfirmPromptValidator));
            AddDialog(new TextPrompt(DialogIds.NamePrompt));
            AddDialog(new ConfirmPrompt(DialogIds.NotificationPrompt));

            InitialDialogId = nameof(ShowOrdersDialog);
        }

        private async Task<DialogTurnResult> ShowOrders(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             var state = await ConversationStateAccessor.GetAsync(stepContext.Context);
             var intent = state.LuisResult.TopIntent().intent;
             var entities = state.LuisResult.Entities;

            // Show orders and get company name
            if (entities.company == null)
            {
                var replyMessage = ResponseManager.GetCardResponse(
                    ShowOrderResponses.ShowOrdersPrompt,
                    new Card("OrderStatus"),
                    null);

                await stepContext.Context.SendActivityAsync(replyMessage);

                return await stepContext.EndDialogAsync();
            }
            else
            {
                // We have a company name
                string company = entities.company[0];

                var options = new PromptOptions()
                {
                    Choices = new List<Choice>(),
                };

                options.Choices.Add(new Choice("Contact Customer"));
                options.Choices.Add(new Choice("Contact Delivery"));
                options.Choices.Add(new Choice("Go To Thirsty"));

                string response = $"I see that the Order to {company} in Decatur is delayed. You may want to:";
                var replyMessage = stepContext.Context.Activity.CreateReply(response);

                state.Company = company;

                return await stepContext.PromptAsync(DialogIds.DelayedOrderPrompt, new PromptOptions { Prompt = replyMessage, Choices = options.Choices }, cancellationToken);
            }
        }

        private async Task<bool> DelayedOrderPromptValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(promptContext.Context);

            if (promptContext.Recognized.Succeeded == true)
            {
                if (promptContext.Recognized.Value.Value == "Contact Customer")
                {
                    state.DelayedOrderAction = "ContactCustomer";
                    return true;
                }
                else
                {
                    state.DelayedOrderAction = "Unsupported";

                    var replyMessage = promptContext.Context.Activity.CreateReply("This option isn't supported in this demo");
                    await promptContext.Context.SendActivityAsync(replyMessage);

                    return true;
                }
            }

            return false;
        }

        private async Task<DialogTurnResult> ContactCustomer(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(stepContext.Context);

            if (state.DelayedOrderAction == "ContactCustomer")
            {

                var options = new PromptOptions()
                {
                    Choices = new List<Choice>(),
                };

                options.Choices.Add(new Choice("Email"));
                options.Choices.Add(new Choice("Call"));

                var replyMessage = ResponseManager.GetCardResponse(
                  ShowOrderResponses.CustomerDetailsPrompt,
                  new Card("CustomerDetails"),
                  null);

                return await stepContext.PromptAsync(DialogIds.ContactCustomerPrompt, new PromptOptions { Prompt = replyMessage, Choices = options.Choices }, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<bool> ContactCustomerPromptValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(promptContext.Context);
            return true;
        }

        private async Task<DialogTurnResult> NewProducts(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(stepContext.Context);        

            var options = new PromptOptions()
            {
                Choices = new List<Choice>(),
            };

            var choice = new Choice("Check customers equipment");
            choice.Synonyms = new List<string>();
            choice.Synonyms.Add("check");
            choice.Synonyms.Add("check equipment");
            choice.Synonyms.Add("check customer equipment");

            options.Choices.Add(choice);

            var replyMessage = ResponseManager.GetCardResponse(
                ShowOrderResponses.NewProductsPrompt,
                new Card("NewProduct"),
                null);

            return await stepContext.PromptAsync(DialogIds.NewProductsPrompt, new PromptOptions { Prompt = replyMessage, Choices = options.Choices }, cancellationToken);
        }

        private async Task<bool> NewProductsPromptValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(promptContext.Context);

            return true;
        }

        private async Task<DialogTurnResult> CheckEquipment(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(stepContext.Context);

            var message = $"This {state.Company} store is equipped with the Freestyle Self-Serve 9000. It is compatible with Coca Cola Orange Vanilla cartridges which are yet to be ordered.";
            var replyMessage = stepContext.Context.Activity.CreateReply(message);
            await stepContext.Context.SendActivityAsync(replyMessage);

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> CheckContact(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(stepContext.Context);

            var message = $"Ok, would you still like to contact this customer?";
            var replyMessage = stepContext.Context.Activity.CreateReply(message);

            return await stepContext.PromptAsync(DialogIds.ContactCustomerConfirm, new PromptOptions { Prompt = replyMessage });
        }

        private async Task<bool> ContactCustomerConfirmPromptValidator(PromptValidatorContext<bool> promptContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(promptContext.Context);

            if (promptContext.Recognized.Succeeded)
            {
                state.ContactCustomer = promptContext.Recognized.Value;
                return true;
            }

            return false;
        }

        private async Task<DialogTurnResult> Contact(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await ConversationStateAccessor.GetAsync(stepContext.Context);

            if (state.ContactCustomer)
            {
                List<CardAction> cardActions = new List<CardAction>();
                cardActions.Add(new CardAction("call", "+1 425 123 456",null, "+1 425 123 456",null, "+1 425 123 456"));

                var attachment = new HeroCard()
                {
                    Title = "Contact Customer",
                    Buttons = cardActions 
                }.ToAttachment();

                var replyMessage = stepContext.Context.Activity.CreateReply("Here are the contact details");
                replyMessage.Attachments.Add(attachment);

                await stepContext.Context.SendActivityAsync(replyMessage);
            }

            return await stepContext.NextAsync();
        }

        //private async Task<DialogTurnResult> PromptForNotification(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    //var tokens = new StringDictionary
        //    //{
        //    //    { "Name", stepContext.Result.ToString() },
        //    //};


        //    //var prompt = ResponseManager.GetResponse(ChangeAddressResponses.NotificationPrompt);
        //    //return await stepContext.PromptAsync(DialogIds.NotificationPrompt, new PromptOptions { Prompt = prompt });

        //    //var response = ResponseManager.GetResponse(ChangeAddressResponses.HaveNameMessage);
        //    //await stepContext.Context.SendActivityAsync(response);

        //    //return await stepContext.NextAsync();
        //}

        private Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string NamePrompt = "namePrompt";
            public const string NotificationPrompt = "notificationPrompt";
            public const string DelayedOrderPrompt = "delayedOrderPrompt";
            public const string ContactCustomerPrompt = "contactCustomerPrompt";
            public const string NewProductsPrompt = "newProductsPrompt";
            public const string ContactCustomerConfirm = "contactCustomerConfirm";
        }
    }
}
