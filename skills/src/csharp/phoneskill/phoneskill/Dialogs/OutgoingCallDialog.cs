using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Builder.Solutions.Util;
using PhoneSkill.Common;
using PhoneSkill.Dialogs.Shared;
using PhoneSkill.Models;
using PhoneSkill.Responses.OutgoingCall;
using PhoneSkill.ServiceClients;
using PhoneSkill.Services;

namespace PhoneSkill.Dialogs.OutgoingCall
{
    public class OutgoingCallDialog : PhoneSkillDialogBase
    {
        private ContactFilter contactFilter;

        public OutgoingCallDialog(
            BotSettings settings,
            BotServices services,
            ResponseManager responseManager,
            ConversationState conversationState,
            IServiceManager serviceManager,
            IBotTelemetryClient telemetryClient)
            : base(nameof(OutgoingCallDialog), settings, services, responseManager, conversationState, serviceManager, telemetryClient)
        {
            TelemetryClient = telemetryClient;

            var outgoingCall = new WaterfallStep[]
            {
                GetAuthToken,
                AfterGetAuthToken,
                PromptForRecipient,
                AskToSelectContact,
                AskToSelectPhoneNumber,
                ExecuteCall,
            };

            AddDialog(new WaterfallDialog(nameof(OutgoingCallDialog), outgoingCall));
            AddDialog(new TextPrompt(DialogIds.RecipientPrompt));
            AddDialog(new ChoicePrompt(DialogIds.ContactSelection, ValidateContactChoice));
            AddDialog(new ChoicePrompt(DialogIds.PhoneNumberSelection, ValidatePhoneNumberChoice));

            InitialDialogId = nameof(OutgoingCallDialog);

            contactFilter = new ContactFilter();
        }

        private async Task<DialogTurnResult> PromptForRecipient(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var state = await PhoneStateAccessor.GetAsync(stepContext.Context);
                var contactProvider = GetContactProvider(state);
                contactFilter.Filter(state, contactProvider);

                if (state.ContactResult.Matches.Count != 0 || !string.IsNullOrEmpty(state.PhoneNumber))
                {
                    return await stepContext.NextAsync();
                }

                var prompt = ResponseManager.GetResponse(OutgoingCallResponses.RecipientPrompt);
                return await stepContext.PromptAsync(DialogIds.RecipientPrompt, new PromptOptions { Prompt = prompt });
            }
            catch (Exception ex)
            {
                await HandleDialogExceptions(stepContext, ex);

                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        private async Task<DialogTurnResult> AskToSelectContact(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                return await stepContext.NextAsync();
            }
            catch (Exception ex)
            {
                await HandleDialogExceptions(stepContext, ex);

                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        private async Task<bool> ValidateContactChoice(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return false;
        }

        private async Task<DialogTurnResult> AskToSelectPhoneNumber(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                return await stepContext.NextAsync();
            }
            catch (Exception ex)
            {
                await HandleDialogExceptions(stepContext, ex);

                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        private async Task<bool> ValidatePhoneNumberChoice(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return false;
        }

        private async Task<DialogTurnResult> ExecuteCall(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var state = await PhoneStateAccessor.GetAsync(stepContext.Context);
                var contactProvider = GetContactProvider(state);
                contactFilter.Filter(state, contactProvider);

                string contactOrPhoneNumber;
                if (state.ContactResult.Matches.Count == 1)
                {
                    contactOrPhoneNumber = state.ContactResult.Matches[0].Name;
                }
                else
                {
                    contactOrPhoneNumber = state.PhoneNumber;
                }

                var tokens = new StringDictionary
                {
                    { "contactOrPhoneNumber", contactOrPhoneNumber },
                };

                var response = ResponseManager.GetResponse(OutgoingCallResponses.ExecuteCall, tokens);
                await stepContext.Context.SendActivityAsync(response);

                state.Clear();

                return await stepContext.EndDialogAsync();
            }
            catch (Exception ex)
            {
                await HandleDialogExceptions(stepContext, ex);

                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        private IContactProvider GetContactProvider(PhoneSkillState state)
        {
            if (state.SourceOfContacts == null)
            {
                // TODO Better error message to tell the bot developer where to specify the source.
                throw new Exception("Cannot retrieve contact list because no contact source specified.");
            }

            return ServiceManager.GetContactProvider(state.Token, state.SourceOfContacts.Value);
        }

        private class DialogIds
        {
            public const string RecipientPrompt = "RecipientPrompt";
            public const string ContactSelection = "ContactSelection";
            public const string PhoneNumberSelection = "PhoneNumberSelection";
        }
    }
}
