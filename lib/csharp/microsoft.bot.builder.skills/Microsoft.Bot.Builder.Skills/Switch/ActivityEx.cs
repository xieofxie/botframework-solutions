using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Skills.Switch
{
    public static class ActivityEx
    {
        /// <summary>
        /// Check if Activity has certain switch.
        /// If NameInActivity not exist in Activity, function fully as version without skill switch
        /// </summary>
        /// <param name="activity">Current activity.</param>
        /// <param name="sw">Switch to check.</param>
        /// <returns>If has.</returns>
        public static bool HasSkillSwitch(this Activity activity, string sw)
        {
            if (activity.SemanticAction == null || !activity.SemanticAction.Entities.ContainsKey(NameInActivity))
            {
                return true;
            }

            return activity.SemanticAction.Entities[NameInActivity].Properties.ContainsKey(sw);
        }

        public static string NameInActivity = "skillSwitchData";
    }
}
