// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Solutions.StorageManager
{
    public class StorageManagerMiddleware : IMiddleware
    {
        public const string CommandPrefix = "/storage:";

        private static readonly StorageManagerOptions Options = new StorageManagerOptions()
        {
            Storage = new FileStorage(nameof(FileStorage)),
        };

        private readonly IStorage _storage;
        private readonly IList<BotState> _storages;
        private readonly StorageManagerOptions _options;
        private readonly List<MethodInfo> _getStorageKeys = new List<MethodInfo>();

        public StorageManagerMiddleware(
            IStorage storage,
            IList<BotState> storages,
            StorageManagerOptions options = null)
        {
            _storage = storage ?? throw new ArgumentException(nameof(storage));
            _storages = storages ?? throw new ArgumentException(nameof(storages));
            _options = options ?? Options;

            foreach (var state in _storages)
            {
                // TODO protected method
                _getStorageKeys.Add(state.GetType().GetMethod("GetStorageKey", BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, new Type[] { typeof(ITurnContext) }, null));
            }
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = turnContext.Activity;

            if (activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(activity.Text))
            {
                var text = activity.Text;

                if (!string.IsNullOrEmpty(text) && text.StartsWith(CommandPrefix))
                {
                    var json = text.Split(new string[] { CommandPrefix }, StringSplitOptions.None)[1];
                    var command = JsonConvert.DeserializeObject<Command>(json);
                    if (command.Type == CommandType.Read)
                    {
                        var data = await _options.Storage.ReadAsync(new string[] { command.Id }, cancellationToken).ConfigureAwait(false);
                        if (data.ContainsKey(command.Id))
                        {
                            var sourceData = data[command.Id] as StoredStorage;

                            var newAllData = new Dictionary<string, object>();
                            var newToDelete = new List<string>();

                            var activityOnlyContext = new ActivityOnlyTurnContext();
                            activityOnlyContext.Activity.ApplyConversationReference(sourceData.ConversationReference, true);
                            for (int i = 0; i < _getStorageKeys.Count; ++i)
                            {
                                var loadedKey = (string)_getStorageKeys[i].Invoke(_storages[i], new object[] { activityOnlyContext });
                                var currentKey = (string)_getStorageKeys[i].Invoke(_storages[i], new object[] { turnContext });
                                if (sourceData.AllData.ContainsKey(loadedKey))
                                {
                                    newAllData[currentKey] = sourceData.AllData[loadedKey];
                                }
                                else
                                {
                                    newToDelete.Add(currentKey);
                                }
                            }

                            await _storage.DeleteAsync(newToDelete.ToArray(), cancellationToken).ConfigureAwait(false);
                            await _storage.WriteAsync(newAllData, cancellationToken).ConfigureAwait(false);
                            await _options.HandleReadSuccessfully(turnContext, command).ConfigureAwait(false);
                        }
                        else
                        {
                            await _options.HandleReadFailed(turnContext, command).ConfigureAwait(false);
                        }
                    }

                    return;
                }
            }

            await next(cancellationToken).ConfigureAwait(false);

            var saveData = new StoredStorage
            {
                ConversationReference = turnContext.Activity.GetConversationReference(),
            };
            var keys = new List<string>();
            for (int i = 0; i < _getStorageKeys.Count; ++i)
            {
                keys.Add((string)_getStorageKeys[i].Invoke(_storages[i], new object[] { turnContext }));
            }

            saveData.AllData = await _storage.ReadAsync(keys.ToArray(), cancellationToken).ConfigureAwait(false);

            var name = await _options.GetName(turnContext).ConfigureAwait(false);
            await _options.Storage.WriteAsync(new Dictionary<string, object> { { name, saveData } }, cancellationToken).ConfigureAwait(false);

            await _options.HandleWriteFinished(turnContext, name).ConfigureAwait(false);
        }
    }
}
