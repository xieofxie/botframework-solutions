// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Skills.Auth;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkillTest
{
    public class MockMicrosoftAppCredentialsEx : IServiceClientCredentials
    {
        public string MicrosoftAppId { get; set; }

        public MockMicrosoftAppCredentialsEx(string appId, string password, string oauthScope)
        {
            MicrosoftAppId = appId;
        }

        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            return string.Empty;
        }

        public Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
