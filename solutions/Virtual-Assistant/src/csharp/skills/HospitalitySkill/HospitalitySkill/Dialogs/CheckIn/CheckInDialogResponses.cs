// https://docs.microsoft.com/en-us/visualstudio/modeling/t4-include-directive?view=vs-2017
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Solutions.Responses;

namespace HospitalitySkill.Dialogs.CheckIn
{
    /// <summary>
    /// Contains bot responses.
    /// </summary>
    public class CheckInDialogResponses : IResponseIdCollection
    {
        // Generated accessors
		public const string RoomInfoMessage = "RoomInfoMessage";
		public const string CardNumberPrompt = "CardNumberPrompt";
		public const string CheckInSuccessfulMessage = "CheckInSuccessfulMessage";
		public const string CheckInFailedMessage = "CheckInFailedMessage";

    }
}