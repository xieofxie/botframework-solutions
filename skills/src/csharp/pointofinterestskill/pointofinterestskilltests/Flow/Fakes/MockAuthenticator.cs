// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Skills.Auth;

namespace PointOfInterestSkillTests.Flow.Fakes
{
    public class MockAuthenticator : IAuthenticator
    {
        public async Task<ClaimsIdentity> Authenticate(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            return new ClaimsIdentity(new Claim[] { new Claim("ver", "1.0"), new Claim("appid", "appId") });
        }
    }
}
