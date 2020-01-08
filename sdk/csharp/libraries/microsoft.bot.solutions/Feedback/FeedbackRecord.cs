﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Solutions.Feedback
{
    internal class FeedbackRecord
    {
        /// <summary>
        /// Gets or sets the activity for which feedback was requested.
        /// </summary>
        /// <value>
        /// The activity for which feedback was requested.
        /// </value>
        public Activity Request { get; set; }

        /// <summary>
        /// Gets or sets feedback submitted by user.
        /// </summary>
        /// <value>
        /// Feedback submitted by user.
        /// </value>
        public CardAction Feedback { get; set; }

        /// <summary>
        /// Gets or sets free-form comment submitted by user.
        /// </summary>
        /// <value>
        /// Free-form comment submitted by user.
        /// </value>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets tag for categorizing feedback.
        /// </summary>
        /// <value>
        /// Tag for categorizing feedback.
        /// </value>
        public string Tag { get; set; }
    }
}
