// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Solutions.StorageManager
{
    public class StorageManagerOptions
    {
        public IStorage Storage { get; set; }

        public Func<ITurnContext, Command, Task> HandleReadFailed { get; set; } = async (ITurnContext context, Command command) =>
        {
            await context.SendActivityAsync($"Read failed for {command.Id}.").ConfigureAwait(false);
        };

        public Func<ITurnContext, Command, Task> HandleReadSuccessfully { get; set; } = async (ITurnContext context, Command command) =>
        {
            await context.SendActivityAsync($"Read successfully for {command.Id}.").ConfigureAwait(false);
        };

        public Func<ITurnContext, Task<string>> GetName { get; set; } = (ITurnContext context) =>
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        };

        public Func<ITurnContext, string, Task> HandleWriteFinished { get; set; } = async (ITurnContext context, string name) =>
        {
            await context.SendActivityAsync(new Activity(type: ActivityTypes.Trace, text: $"Write finished for {name}.")).ConfigureAwait(false);
        };
    }
}
