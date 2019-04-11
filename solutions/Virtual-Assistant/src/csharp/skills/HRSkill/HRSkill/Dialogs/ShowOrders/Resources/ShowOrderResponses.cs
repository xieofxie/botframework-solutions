// https://docs.microsoft.com/en-us/visualstudio/modeling/t4-include-directive?view=vs-2017
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Solutions.Responses;

namespace HRSkill.Dialogs.ShowOrders.Resources
{
    /// <summary>
    /// Contains bot responses.
    /// </summary>
    public class ShowOrderResponses : IResponseIdCollection
    {
        // Generated accessors
        public const string ShowOrdersPrompt = "ShowOrdersPrompt";
        public const string CustomerDetailsPrompt = "CustomerDetailsPrompt";
        public const string NotificationPrompt = "NotificationPrompt";
        public const string ContactCustomer = "ContactCustomer";
        public const string NewProductsPrompt = "NewProductsPrompt";
    }
}