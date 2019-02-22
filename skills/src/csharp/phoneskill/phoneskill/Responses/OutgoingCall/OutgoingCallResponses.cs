// https://docs.microsoft.com/en-us/visualstudio/modeling/t4-include-directive?view=vs-2017
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Solutions.Responses;

namespace PhoneSkill.Responses.OutgoingCall
{
    /// <summary>
    /// Contains bot responses.
    /// </summary>
    public class OutgoingCallResponses : IResponseIdCollection
    {
        // Generated accessors
        public const string RecipientPrompt = "RecipientPrompt";
        public const string ContactNotFound = "ContactNotFound";
        public const string ContactHasNoPhoneNumber = "ContactHasNoPhoneNumber";
        public const string ContactSelection = "ContactSelection";
        public const string PhoneNumberSelection = "PhoneNumberSelection";
        public const string ExecuteCall = "ExecuteCall";
    }
}