// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Solutions.StorageManager
{
    internal class StoredStorage
    {
        public ConversationReference ConversationReference { get; set; }

        public IDictionary<string, object> AllData { get; set; }
    }
}
