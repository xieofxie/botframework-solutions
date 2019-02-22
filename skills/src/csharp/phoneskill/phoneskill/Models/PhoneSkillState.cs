using Luis;
using Microsoft.Bot.Builder.Dialogs;

namespace PhoneSkill.Models
{
    public class PhoneSkillState : DialogState
    {
        public PhoneSkillState()
        {
            Clear();
        }

        /// <summary>
        /// Gets the authentication token needed for getting the user's contact list.
        /// </summary>
        /// <value>
        /// The authentication token needed for getting the user's contact list.
        /// </value>
        public string Token { get; internal set; }

        /// <summary>
        /// Gets or sets the source of the user's contact list.
        /// </summary>
        /// <value>
        /// The source of the user's contact list.
        /// </value>
        public ContactSource? SourceOfContacts { get; set; }

        /// <summary>
        /// Gets or sets the most recent LUIS result.
        /// </summary>
        /// <value>
        /// The most recent LUIS result.
        /// </value>
        public PhoneLuis LuisResult { get; set; }

        /// <summary>
        /// Gets or sets the result of the contact search (if one was performed).
        /// </summary>
        /// <value>
        /// The result of the contact search (if one was performed).
        /// </value>
        public ContactSearchResult ContactResult { get; set; }

        /// <summary>
        /// Gets or sets the final phone number to call.
        /// </summary>
        /// <value>
        /// The final phone number to call.
        /// </value>
        public string PhoneNumber { get; set; }

        public void Clear()
        {
            Token = string.Empty;
            SourceOfContacts = null;
            LuisResult = new PhoneLuis();
            ContactResult = new ContactSearchResult();
            PhoneNumber = string.Empty;
        }
    }
}
