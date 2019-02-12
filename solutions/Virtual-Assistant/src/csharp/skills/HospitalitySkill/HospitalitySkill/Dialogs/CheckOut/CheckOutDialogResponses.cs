// https://docs.microsoft.com/en-us/visualstudio/modeling/t4-include-directive?view=vs-2017
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Solutions.Responses;

namespace HospitalitySkill.Dialogs.CheckOut
{
    /// <summary>
    /// Contains bot responses.
    /// </summary>
    public class CheckOutDialogResponses : IResponseIdCollection
    {
        // Generated accessors
		public const string EmailPrompt = "EmailPrompt";
		public const string InvalidEmailPrompt = "InvalidEmailPrompt";
		public const string HaveEmailMessage = "HaveEmailMessage";
		public const string CheckOutSuccessful = "CheckOutSuccessful";

    }
}