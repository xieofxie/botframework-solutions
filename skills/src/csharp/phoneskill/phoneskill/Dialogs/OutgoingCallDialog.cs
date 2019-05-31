using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Builder.Solutions.Util;
using Microsoft.Bot.Schema;
using PhoneSkill.Common;
using PhoneSkill.Models;
using PhoneSkill.Responses.OutgoingCall;
using PhoneSkill.Services;
using PhoneSkill.Services.Luis;

namespace PhoneSkill.Dialogs
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
            AddDialog(new TextPrompt(DialogIds.RecipientPrompt, ValidateRecipient));
            AddDialog(new ChoicePrompt(DialogIds.ContactSelection, ValidateContactChoice)
            {
                Style = ListStyle.List,
            });
            AddDialog(new ChoicePrompt(DialogIds.PhoneNumberSelection, ValidatePhoneNumberChoice)
            {
                Style = ListStyle.List,
            });

            InitialDialogId = nameof(OutgoingCallDialog);

            contactFilter = new ContactFilter();
        }

        private async Task<DialogTurnResult> PromptForRecipient(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var state = await PhoneStateAccessor.GetAsync(stepContext.Context);
                var contactProvider = GetContactProvider(state);
                await contactFilter.Filter(state, contactProvider);

                if (state.ContactResult.Matches.Count != 0 || state.PhoneNumber.Any())
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

        private async Task<bool> ValidateRecipient(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var state = await PhoneStateAccessor.GetAsync(promptContext.Context);
            var contactProvider = GetContactProvider(state);
            await contactFilter.Filter(state, contactProvider);

            return contactFilter.HasRecipient(state);
        }

        private async Task<DialogTurnResult> AskToSelectContact(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var state = await PhoneStateAccessor.GetAsync(stepContext.Context);
                await contactFilter.Filter(state, contactProvider: null);

                if (contactFilter.IsContactDisambiguated(state))
                {
                    return await stepContext.NextAsync();
                }

                var options = new PromptOptions();
                UpdateContactSelectionPromptOptions(options, state);

                return await stepContext.PromptAsync(DialogIds.ContactSelection, options);
            }
            catch (Exception ex)
            {
                await HandleDialogExceptions(stepContext, ex);

                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        private async Task<bool> ValidateContactChoice(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            var state = await PhoneStateAccessor.GetAsync(promptContext.Context);
            if (contactFilter.IsContactDisambiguated(state))
            {
                return true;
            }

            var contactSelectionResult = await RunLuis<ContactSelectionLuis>(promptContext.Context, "contactSelection");
            contactFilter.OverrideEntities(state, contactSelectionResult);
            var isFiltered = await contactFilter.Filter(state, contactProvider: null);
            if (contactFilter.IsContactDisambiguated(state))
            {
                return true;
            }
            else if (isFiltered)
            {
                UpdateContactSelectionPromptOptions(promptContext.Options, state);
                return false;
            }

            if (promptContext.Recognized.Value != null
                && promptContext.Recognized.Value.Index >= 0
                && promptContext.Recognized.Value.Index < state.ContactResult.Matches.Count)
            {
                state.ContactResult.Matches = new List<ContactCandidate>() { state.ContactResult.Matches[promptContext.Recognized.Value.Index] };
            }

            return contactFilter.IsContactDisambiguated(state);
        }

        private async Task<DialogTurnResult> AskToSelectPhoneNumber(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var state = await PhoneStateAccessor.GetAsync(stepContext.Context);
                await contactFilter.Filter(state, contactProvider: null);

                if (contactFilter.IsPhoneNumberDisambiguated(state))
                {
                    return await stepContext.NextAsync();
                }

                var options = new PromptOptions();
                UpdatePhoneNumberSelectionPromptOptions(options, state);

                return await stepContext.PromptAsync(DialogIds.PhoneNumberSelection, options);
            }
            catch (Exception ex)
            {
                await HandleDialogExceptions(stepContext, ex);

                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        private async Task<bool> ValidatePhoneNumberChoice(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            var state = await PhoneStateAccessor.GetAsync(promptContext.Context);
            if (contactFilter.IsPhoneNumberDisambiguated(state))
            {
                return true;
            }

            var phoneNumberSelectionResult = await RunLuis<PhoneNumberSelectionLuis>(promptContext.Context, "phoneNumberSelection");
            contactFilter.OverrideEntities(state, phoneNumberSelectionResult);
            var isFiltered = await contactFilter.Filter(state, contactProvider: null);
            if (contactFilter.IsPhoneNumberDisambiguated(state))
            {
                return true;
            }
            else if (isFiltered)
            {
                UpdatePhoneNumberSelectionPromptOptions(promptContext.Options, state);
                return false;
            }

            var phoneNumberList = state.ContactResult.Matches[0].PhoneNumbers;
            if (promptContext.Recognized.Value != null
                && promptContext.Recognized.Value.Index >= 0
                && promptContext.Recognized.Value.Index < phoneNumberList.Count)
            {
                state.ContactResult.Matches[0].PhoneNumbers = new List<PhoneNumber>() { phoneNumberList[promptContext.Recognized.Value.Index] };
            }

            return contactFilter.IsPhoneNumberDisambiguated(state);
        }

        private async Task<DialogTurnResult> ExecuteCall(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var state = await PhoneStateAccessor.GetAsync(stepContext.Context);
                await contactFilter.Filter(state, contactProvider: null);

                var templateId = OutgoingCallResponses.ExecuteCall;
                var tokens = new StringDictionary();
                var outgoingCall = new OutgoingCall
                {
                    Number = state.PhoneNumber,
                };
                if (state.ContactResult.Matches.Count == 1)
                {
                    tokens["contactOrPhoneNumber"] = state.ContactResult.Matches[0].Name;
                    outgoingCall.Contact = state.ContactResult.Matches[0];
                }
                else
                {
                    tokens["contactOrPhoneNumber"] = state.PhoneNumber;
                }

                if (state.ContactResult.RequestedPhoneNumberType.Any()
                    && state.ContactResult.Matches.Count == 1
                    && state.ContactResult.Matches[0].PhoneNumbers.Count == 1)
                {
                    templateId = OutgoingCallResponses.ExecuteCallWithPhoneNumberType;
                    tokens["phoneNumberType"] = GetSpeakablePhoneNumberType(state.ContactResult.Matches[0].PhoneNumbers[0].Type);
                }

                var response = ResponseManager.GetResponse(templateId, tokens);
                await stepContext.Context.SendActivityAsync(response);

                await SendEvent(stepContext, outgoingCall);

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

        private void UpdateContactSelectionPromptOptions(PromptOptions options, PhoneSkillState state)
        {
            var tokens = new StringDictionary
            {
                // TODO only fill if confident enough about ASR result
                { "contactName", state.ContactResult.SearchQuery },
            };
            options.Prompt = ResponseManager.GetResponse(OutgoingCallResponses.ContactSelection, tokens);

            options.Choices = new List<Choice>();
            for (var i = 0; i < state.ContactResult.Matches.Count; ++i)
            {
                var item = state.ContactResult.Matches[i].Name;
                var synonyms = new List<string>
                {
                    item,
                    (i + 1).ToString(),
                };
                var choice = new Choice()
                {
                    Value = item,
                    Synonyms = synonyms,
                };
                options.Choices.Add(choice);
            }
        }

        private void UpdatePhoneNumberSelectionPromptOptions(PromptOptions options, PhoneSkillState state)
        {
            var tokens = new StringDictionary
            {
                { "contact", state.ContactResult.Matches[0].Name },
            };

            var prompt = ResponseManager.GetResponse(OutgoingCallResponses.PhoneNumberSelection, tokens);

            options.Prompt = prompt;
            options.Choices = new List<Choice>();

            var phoneNumberList = state.ContactResult.Matches[0].PhoneNumbers;
            for (var i = 0; i < phoneNumberList.Count; ++i)
            {
                var phoneNumber = phoneNumberList[i];
                var speakableType = $"{GetSpeakablePhoneNumberType(phoneNumber.Type)}: {phoneNumber.Number}";
                var synonyms = new List<string>
                {
                    speakableType,
                    phoneNumber.Type.FreeForm,
                    phoneNumber.Number,
                    (i + 1).ToString(),
                };
                var choice = new Choice()
                {
                    Value = speakableType,
                    Synonyms = synonyms,
                };
                options.Choices.Add(choice);
            }
        }

        private string GetSpeakablePhoneNumberType(PhoneNumberType phoneNumberType)
        {
            string speakableType;
            switch (phoneNumberType.Standardized)
            {
                case PhoneNumberType.StandardType.BUSINESS:
                    speakableType = "Business";
                    break;
                case PhoneNumberType.StandardType.HOME:
                    speakableType = "Home";
                    break;
                case PhoneNumberType.StandardType.MOBILE:
                    speakableType = "Mobile";
                    break;
                case PhoneNumberType.StandardType.NONE:
                default:
                    speakableType = phoneNumberType.FreeForm;
                    break;
            }

            return speakableType;
        }

        /// <summary>
        /// Send an event activity to communicate to the client which phone number to call.
        /// This event is meant to be processed by client code rather than shown to the user.
        /// </summary>
        /// <param name="stepContext">The WaterfallStepContext.</param>
        /// <param name="outgoingCall">The phone call to make.</param>
        /// <returns>A Task.</returns>
        private async Task SendEvent(WaterfallStepContext stepContext, OutgoingCall outgoingCall)
        {
            var actionEvent = stepContext.Context.Activity.CreateReply();
            actionEvent.Type = ActivityTypes.Event;

            actionEvent.Name = "PhoneSkill.OutgoingCall";
            actionEvent.Value = outgoingCall;

            await stepContext.Context.SendActivityAsync(actionEvent);
        }

        private class DialogIds
        {
            public const string RecipientPrompt = "RecipientPrompt";
            public const string ContactSelection = "ContactSelection";
            public const string PhoneNumberSelection = "PhoneNumberSelection";
        }
    }
}
