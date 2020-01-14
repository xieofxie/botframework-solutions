// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace Microsoft.Bot.Solutions.StorageManager
{
    public class FileStorage : IStorage
    {
        private static readonly JsonSerializer StateJsonSerializer = new JsonSerializer()
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
        };

        private readonly JsonSerializer _stateJsonSerializer;
        private string _pathRoot;
        private readonly object _syncroot = new object();

        public FileStorage(
            string pathRoot,
            JsonSerializer jsonSerializer = null)
        {
            _stateJsonSerializer = jsonSerializer ?? StateJsonSerializer;
            _pathRoot = pathRoot ?? throw new ArgumentException(nameof(pathRoot));
        }

        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    File.Delete(Path.Combine(_pathRoot, key));
                }
            }

            return Task.CompletedTask;
        }

        public Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            var storeItems = new Dictionary<string, object>(keys.Length);
            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    var fullPath = Path.Combine(_pathRoot, key);
                    if (File.Exists(fullPath))
                    {
                        using (var file = File.OpenText(fullPath))
                        using (var reader = new JsonTextReader(file))
                        {
                            storeItems.Add(key, _stateJsonSerializer.Deserialize(reader));
                        }
                    }
                }
            }

            return Task.FromResult<IDictionary<string, object>>(storeItems);
        }

        public Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            lock (_syncroot)
            {
                foreach (var item in changes)
                {
                    var fullPath = Path.Combine(_pathRoot, item.Key);
                    using (var file = File.CreateText(fullPath))
                    using (var writer = new JsonTextWriter(file))
                    {
                        _stateJsonSerializer.Serialize(writer, item.Value);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
