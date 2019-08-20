using Microsoft.Bot.Builder.Skills.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Skills.Switch
{
    public interface ISkillSwitchManager
    {
        /// <summary>
        /// Get skill switches for current context.
        /// </summary>
        /// <param name="turnContext">Current context.</param>
        /// <returns>if null, no skills/switches allowed.</returns>
        Task<SkillSwitchData> GetSkillSwitch(ITurnContext turnContext);
    }
}
