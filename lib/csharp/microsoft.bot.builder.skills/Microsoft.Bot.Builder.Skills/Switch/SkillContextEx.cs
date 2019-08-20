using Microsoft.Bot.Builder.Skills.Models;
using Microsoft.Bot.Builder.Skills.Models.Manifest;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Skills.Switch
{
    public static class SkillContextEx
    {
        /// <summary>
        /// Set SkillSwitchData in SkillContext with NameInSkillContext.
        /// </summary>
        /// <param name="skillContext">User's SkillContext.</param>
        /// <param name="skillSwitchData">User's SkillSwitchData.</param>
        public static void SetSkillSwitchData(this SkillContext skillContext, SkillSwitchData skillSwitchData)
        {
            var data = new JObject();
            
            if (skillSwitchData != null)
            {
                foreach (var pair in skillSwitchData)
                {
                    var value = new JObject();
                    if (pair.Value != null)
                    {
                        foreach(var sw in pair.Value)
                        {
                            value.Add(sw, null);
                        }
                    }
                    data.Add(pair.Key, value);
                }
            }
            
            skillContext[NameInSkillContext] = data;
        }

        /// <summary>
        /// Set SkillSwitchData from SkillContext to Activity to send.
        /// If NameInSkillContext does not exist in SkillContext, do not set to Activity. Work as version without skill switch.
        /// </summary>
        /// <param name="skillContext">User's SkillContext.</param>
        /// <param name="activity">Activity to send.</param>
        /// <param name="skillManifest">This skill's SkillManifest.</param>
        public static void SetSkillSwitchDataToActivity(this SkillContext skillContext, Activity activity, SkillManifest skillManifest)
        {
            if (skillContext.ContainsKey(NameInSkillContext))
            {
                if (activity.SemanticAction == null)
                {
                    activity.SemanticAction = new SemanticAction
                    {
                        Entities = new Dictionary<string, Entity>()
                    };
                }

                var entity = new Entity();

                if (skillContext[NameInSkillContext].ContainsKey(skillManifest.Id))
                {
                    var switches = skillContext[NameInSkillContext][skillManifest.Id] as JObject;
                    foreach (var sw in skillManifest.Switches)
                    {
                        if (switches.ContainsKey(sw.Id))
                        {
                            entity.Properties.Add(sw.Id, null);
                        }
                    }                    
                }

                activity.SemanticAction.Entities[ActivityEx.NameInActivity] = entity;
            }
        }

        public static string NameInSkillContext = "skillSwitchData";
    }
}
