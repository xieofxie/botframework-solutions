using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Skills.Models.Manifest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkillToggling.Models;
using System.Collections;

namespace SkillToggling.Services
{
    public class GroupPermissionAPI
    {
        private readonly IDictionary<string, List<SkillPermission>> groupPermissions;
        private readonly IList<SkillManifest> skills;


        public GroupPermissionAPI(BotSettings settings)
        {
            groupPermissions = new Dictionary<string, List<SkillPermission>>(settings.GroupPermissions.Select(permission => new KeyValuePair<string, List<SkillPermission>>(permission.Id, permission.SkillPermissions)));
            skills = settings.Skills;
        }

        public IDictionary<string, HashSet<string>> DecodePermission(IList<string> groups)
        {
            var result = new Dictionary<string, HashSet<string>>();
            foreach (var group in groups)
            {
                if (groupPermissions.ContainsKey(group))
                {
                    foreach (var skillPermission in groupPermissions[group])
                    {
                        if (!result.ContainsKey(skillPermission.Id))
                        {
                            result.Add(skillPermission.Id, new HashSet<string>());
                        }

                        foreach (var permission in skillPermission.Permissions)
                        {
                            result[skillPermission.Id].Add(permission);
                        }
                    }
                }
            }
            return result;
        }

        public void Update(SkillContext skillContext, IDictionary<string, HashSet<string>> permissions)
        {
            foreach(var skill in skills)
            {
                if (skillContext.ContainsKey(skill.Id))
                {
                    skillContext.Remove(skill.Id);
                }
            }

            foreach(var permission in permissions)
            {
                if (!skillContext.ContainsKey(permission.Key))
                {
                    skillContext.Add(permission.Key, new JObject());
                }

                foreach (var action in permission.Value)
                {
                    skillContext[permission.Key].Add(action, JToken.FromObject(string.Empty));
                }
            }
        }
    }
}
