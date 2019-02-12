// https://docs.microsoft.com/en-us/visualstudio/modeling/t4-include-directive?view=vs-2017
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Solutions.Responses;

namespace HospitalitySkill.Dialogs.OrderFood
{
    /// <summary>
    /// Contains bot responses.
    /// </summary>
    public class OrderFoodDialogResponses : IResponseIdCollection
    {
        // Generated accessors
		public const string ShowFoodOptionsMessage = "ShowFoodOptionsMessage";
		public const string PromptForOrder = "PromptForOrder";
		public const string PromptForTime = "PromptForTime";
		public const string OrderPlacedMessage = "OrderPlacedMessage";

    }
}