using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SkillToggling.Services
{
    public class MSGraphAPI
    {
        private readonly string accessToken;
        public MSGraphAPI(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public async Task<IList<string>> GetGroupIds()
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Utc.Id + "\"");
                        await Task.CompletedTask;
                    }));

            var groups = await graphClient.Me.MemberOf.Request().GetAsync();
            var result = new List<string>();
            foreach(var group in groups)
            {
                if (group is Group)
                {
                    result.Add(group.Id);
                }
            }
            return result;
        }
    }
}
