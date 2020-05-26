// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualAssistantSample.Models
{
    public class UserReferenceState
    {
        public Dictionary<string, UserReference> References { get; set; } = new Dictionary<string, UserReference>();
    }
}
