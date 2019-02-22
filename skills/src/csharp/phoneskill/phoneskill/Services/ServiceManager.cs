// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using PhoneSkill.Common;
using PhoneSkill.Models;
using PhoneSkill.ServiceClients.GoogleAPI;
using PhoneSkill.ServiceClients.MSGraphAPI;
using PhoneSkill.Services;

namespace PhoneSkill.ServiceClients
{
    public class ServiceManager : IServiceManager
    {
        private BotSettings _settings;

        public ServiceManager(BotSettings settings)
        {
            _settings = settings;
        }

        public IContactProvider GetContactProvider(string token, ContactSource source)
        {
            switch (source)
            {
                case ContactSource.Microsoft:
                    var serviceClient = GraphClient.GetAuthenticatedClient(token);
                    return new GraphContactProvider(serviceClient);
                case ContactSource.Google:
                    var googleClient = new GoogleClient(_settings, token);
                    return new GoogleContactProvider(googleClient);
                default:
                    throw new Exception($"ContactSource not covered in switch statement: {source.ToString()}");
            }
        }
    }
}
