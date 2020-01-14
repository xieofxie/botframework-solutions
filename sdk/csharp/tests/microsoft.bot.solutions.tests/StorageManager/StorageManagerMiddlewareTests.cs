// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Solutions.StorageManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Solutions.Tests.StorageManager
{
    [TestClass]
    public class StorageManagerMiddlewareTests
    {
        private static readonly Func<ITurnContext, string, Task> HandleWriteFinished = (ITurnContext context, string name) =>
        {
            return Task.CompletedTask;
        };

        [TestMethod]
        public async Task TestConversationState()
        {
            int counter = 0;
            var storage = new MemoryStorage();
            var convState = new ConversationState(storage);
            var options = new StorageManagerOptions()
            {
                Storage = storage,
                GetName = (ITurnContext context) =>
                {
                    counter++;
                    return Task.FromResult(counter.ToString());
                },
                HandleWriteFinished = HandleWriteFinished,
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("Name"))
                .Use(new StorageManagerMiddleware(storage, new BotState[] { convState }, options));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                var accessor = convState.CreateProperty<int>("data");
                var savedData = await accessor.GetAsync(context, () => 0, cancellationToken);
                savedData += 1;
                await accessor.SetAsync(context, savedData);
                await context.SendActivityAsync(savedData.ToString());

                await convState.SaveChangesAsync(context, false, cancellationToken);
            })
                .Send("foo")
                .AssertReply("1")
                .Send("foo")
                .AssertReply("2")
                .Send("/storage:{id:'1'}")
                .AssertReply($"Read successfully for 1.")
                .Send("foo")
                .AssertReply("2")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestUserState()
        {
            int counter = 0;
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var options = new StorageManagerOptions()
            {
                Storage = storage,
                GetName = (ITurnContext context) =>
                {
                    counter++;
                    return Task.FromResult(counter.ToString());
                },
                HandleWriteFinished = HandleWriteFinished,
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("Name"))
                .Use(new StorageManagerMiddleware(storage, new BotState[] { userState }, options));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                var accessor = userState.CreateProperty<int>("data");
                var savedData = await accessor.GetAsync(context, () => 0, cancellationToken);
                savedData += 1;
                await accessor.SetAsync(context, savedData);
                await context.SendActivityAsync(savedData.ToString());

                await userState.SaveChangesAsync(context, false, cancellationToken);
            })
                .Send("foo")
                .AssertReply("1")
                .Send("foo")
                .AssertReply("2")
                .Send("/storage:{id:'1'}")
                .AssertReply($"Read successfully for 1.")
                .Send("foo")
                .AssertReply("2")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestDeleteState()
        {
            int counter = 0;
            var storage = new MemoryStorage();
            var convState = new ConversationState(storage);
            var userState = new UserState(storage);
            var options = new StorageManagerOptions()
            {
                Storage = storage,
                GetName = (ITurnContext context) =>
                {
                    counter++;
                    return Task.FromResult(counter.ToString());
                },
                HandleWriteFinished = HandleWriteFinished,
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("Name"))
                .Use(new StorageManagerMiddleware(storage, new BotState[] { convState, userState }, options));

            var userString = "bar";

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                var accessor = convState.CreateProperty<int>("data");
                var savedData = await accessor.GetAsync(context, () => 0, cancellationToken);
                savedData += 1;
                await accessor.SetAsync(context, savedData);

                await context.SendActivityAsync(savedData.ToString());

                var userAccessor = userState.CreateProperty<string>("data");
                if (savedData == 3)
                {
                    await userAccessor.SetAsync(context, userString);
                }

                var userData = await userAccessor.GetAsync(context, () => null, cancellationToken);
                if (!string.IsNullOrEmpty(userData))
                {
                    await context.SendActivityAsync(userData.ToString());
                }

                await convState.SaveChangesAsync(context, false, cancellationToken);
            })
                .Send("foo")
                .AssertReply("1")
                .Send("foo")
                .AssertReply("2")
                .Send("foo")
                .AssertReply("3")
                .AssertReply(userString)
                .Send("/storage:{id:'1'}")
                .AssertReply($"Read successfully for 1.")
                .Send("foo")
                .AssertReply("2")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestReadingFailed()
        {
            int counter = 0;
            var storage = new MemoryStorage();
            var convState = new ConversationState(storage);
            var options = new StorageManagerOptions()
            {
                Storage = storage,
                GetName = (ITurnContext context) =>
                {
                    counter++;
                    return Task.FromResult(counter.ToString());
                },
                HandleWriteFinished = HandleWriteFinished,
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("Name"))
                .Use(new StorageManagerMiddleware(storage, new BotState[] { convState }, options));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                var accessor = convState.CreateProperty<int>("data");
                var savedData = await accessor.GetAsync(context, () => 0, cancellationToken);
                savedData += 1;
                await accessor.SetAsync(context, savedData);
                await context.SendActivityAsync(savedData.ToString());

                await convState.SaveChangesAsync(context, false, cancellationToken);
            })
                .Send("foo")
                .AssertReply("1")
                .Send("foo")
                .AssertReply("2")
                .Send("/storage:{id:'3'}")
                .AssertReply($"Read failed for 3.")
                .Send("/storage:{id:'1'}")
                .AssertReply($"Read successfully for 1.")
                .Send("foo")
                .AssertReply("2")
                .StartTestAsync();
        }
    }
}
