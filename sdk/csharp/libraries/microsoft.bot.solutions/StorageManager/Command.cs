// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Solutions.StorageManager
{
    public class Command
    {
        [JsonProperty("command")]
        public CommandType Type { get; set; } = CommandType.Read;

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
