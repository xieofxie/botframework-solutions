using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.PhoneticMatching.Matchers.ContactMatcher;
using PhoneSkill.Models;

namespace PhoneSkill.Common
{
    /// <summary>
    /// Filters the user's contact list repeatedly based on the user's input to determine the right contact and phone number to call.
    /// </summary>
    public class ContactFilter
    {
        /// <summary>
        /// Filters the user's contact list repeatedly based on the user's input to determine the right contact and phone number to call.
        /// </summary>
        /// <param name="state">The current conversation state. This will be modified.</param>
        /// <param name="contactProvider">The provider for the user's contact list.</param>
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

            IList<ContactCandidate> contacts;
            if (state.ContactResult.Matches.Any())
            {
                contacts = state.ContactResult.Matches;
            }
            else
            {
                contacts = await contactProvider.GetContacts();
            }

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
