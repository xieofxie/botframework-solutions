using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills.Models;
using Microsoft.Bot.Builder.Skills.Switch;

namespace VirtualAssistantSample.Services
{
    /// <summary>
    /// A simple sample ISkillSwitchManager.
    /// </summary>
    public class SkillSwitchManager : ISkillSwitchManager
    {
        public async Task<SkillSwitchData> GetSkillSwitch(ITurnContext turnContext)
        {
            if (turnContext.Activity.Locale != null && turnContext.Activity.Locale == "en-us")
            {
                return new SkillSwitchData
                {
                    { "sampleSkill", new HashSet<string> { "sample" } }
                };
            }

            return null;
        }
    }
}
