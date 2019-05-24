using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Luis;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.PhoneticMatching.Matchers.ContactMatcher;
using PhoneSkill.Models;

namespace PhoneSkill.Common
{
    /// <summary>
    /// Business logic for filtering contacts.
    /// </summary>
    public class ContactFilter
    {
        private static readonly Regex DigitSequence = new Regex("[0-9]+", RegexOptions.Compiled);

        /// <summary>
        /// Filters the user's contact list repeatedly based on the user's input to determine the right contact and phone number to call.
        /// </summary>
        /// <param name="state">The current conversation state. This will be modified.</param>
        /// <param name="contactProvider">The provider for the user's contact list. This may be null if the contact list is not to be used.</param>
        public async void Filter(PhoneSkillState state, IContactProvider contactProvider)
        {
            var entities = state.LuisResult.Entities;
            var entitiesForSearch = new List<InstanceData>();
            if (entities._instance.contactName != null)
            {
                entitiesForSearch.AddRange(entities._instance.contactName);
            }

            if (entities._instance.contactRelation != null)
            {
                entitiesForSearch.AddRange(entities._instance.contactRelation);
            }

            entitiesForSearch = SortAndRemoveOverlappingEntities(entitiesForSearch);
            var searchQuery = string.Join(" ", entitiesForSearch.Select(entity => entity.Text));

            if (searchQuery.Any())
            {
                IList<ContactCandidate> contacts;
                if (state.ContactResult.Matches.Any())
                {
                    contacts = state.ContactResult.Matches;
                }
                else if (contactProvider != null)
                {
                    contacts = await contactProvider.GetContacts();
                }
                else
                {
                    contacts = new List<ContactCandidate>();
                }

                if (contacts.Any())
                {
                    // TODO Adjust max number of returned contacts?
                    var matcher = new EnContactMatcher<ContactCandidate>(contacts, ExtractContactFields);
                    var matches = matcher.FindByName(searchQuery);

                    if (!state.ContactResult.RequestedPhoneNumberType.Any())
                    {
                        // TODO Get requested phone number type from LUIS result.
                    }

                    // TODO Filter by requested phone number type.

                    state.ContactResult.SearchQuery = searchQuery;
                    state.ContactResult.Matches = matches;
                }
            }

            if (!state.PhoneNumber.Any())
            {
                if (state.ContactResult.Matches.Count == 1 && state.ContactResult.Matches[0].PhoneNumbers.Count == 1)
                {
                    state.PhoneNumber = state.ContactResult.Matches[0].PhoneNumbers[0].Number;
                }
                else if (entities.phoneNumber != null && entities.phoneNumber.Any())
                {
                    state.PhoneNumber = string.Join(" ", entities.phoneNumber);
                }
            }
        }

        /// <summary>
        /// Returns whether the contact has been completely disambiguated.
        /// </summary>
        /// <param name="state">The current state.</param>
        /// <returns>Whether the contact has been completely disambiguated.</returns>
        public bool IsContactDisambiguated(PhoneSkillState state)
        {
            return state.ContactResult.Matches.Count == 1 || state.PhoneNumber.Any();
        }

        /// <summary>
        /// Override the entities on the state with the ones from the given LUIS result.
        /// </summary>
        /// <param name="state">The current state. This will be modified.</param>
        /// <param name="contactSelectionResult">The LUIS result.</param>
        public void OverrideEntities(PhoneSkillState state, ContactSelectionLuis contactSelectionResult)
        {
            state.LuisResult.Entities.contactName = contactSelectionResult.Entities.contactName;
            state.LuisResult.Entities._instance.contactName = contactSelectionResult.Entities._instance.contactName;

            state.LuisResult.Entities.contactRelation = new string[0][];
            state.LuisResult.Entities._instance.contactRelation = new InstanceData[0];

            state.LuisResult.Entities.phoneNumber = new string[0];
            state.LuisResult.Entities._instance.contactRelation = new InstanceData[0];

            state.LuisResult.Entities.phoneNumberSpelledOut = new string[0];
            state.LuisResult.Entities._instance.contactRelation = new InstanceData[0];
        }

        private List<InstanceData> SortAndRemoveOverlappingEntities(List<InstanceData> entities)
        {
            // TODO implement
            return entities;
        }

        private ContactFields ExtractContactFields(ContactCandidate contact)
        {
            return new ContactFields
            {
                Name = contact.Name,
            };
        }
    }
}
