// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Solutions.Model.Proactive
{
    public enum ProactiveNextStepActionType
    {
        /// <summary>
        /// Send a message to service bus
        /// </summary>
        ServiceBus,

        /// <summary>
        /// Show a dialog
        /// </summary>
        ShowDialog
    }
}