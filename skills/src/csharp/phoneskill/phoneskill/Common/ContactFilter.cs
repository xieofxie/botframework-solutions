using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        /// Selects the correct contact candidate by index.
        /// </summary>
        /// <param name="state">The current state. This will be modified.</param>
        /// <param name="indexEntityValues">The extracted index entity values.</param>
        public void SelectContactByIndex(PhoneSkillState state, string[] indexEntityValues)
        {
            if (!state.ContactResult.Matches.Any() || !indexEntityValues.Any() || IsContactDisambiguated(state))
            {
                return;
            }

            var index = ParseIndex(indexEntityValues[0]);
            if (index == null || index > state.ContactResult.Matches.Count || index < -state.ContactResult.Matches.Count)
            {
                return;
            }

            ContactCandidate selectedCandidate;
            if (index >= 0)
            {
                selectedCandidate = state.ContactResult.Matches[index.Value];
            }
            else
            {
                selectedCandidate = state.ContactResult.Matches[state.ContactResult.Matches.Count - index.Value];
            }

            state.ContactResult.Matches = new List<ContactCandidate>() { selectedCandidate };
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

        /// <summary>
        /// Parse the extracted index entity into a machine-readable index.
        /// </summary>
        /// <param name="indexEntityValue">The entity value to parse.</param>
        /// <returns>The zero-based index to select in the list. Negative indices are to be counted from the end of the list with -1 referring to the last element of the list. If parsing fails, null is retruned.</returns>
        private int? ParseIndex(string indexEntityValue)
        {
            var match = DigitSequence.Match(indexEntityValue);
            if (match.Success)
            {
                try
                {
                    // Users tend to speak in one-based indices, but we need zero-based indices.
                    return int.Parse(match.Value) - 1;
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
            }

            // TODO number normalization

            return null;
        }
    }
}
