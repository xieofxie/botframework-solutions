using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Skills.Switch
{
    public static class ActivityEx
    {
        /// <summary>
        /// Return switches in NameInActivity.
        /// If NameInActivity not exist in Activity, should function fully as version without skill switch
        /// </summary>
        /// <param name="activity">Current activity.</param>
        /// <returns>If has.</returns>
        public static HashSet<string> GetSwitches(this Activity activity)
        {
            if (activity.SemanticAction == null || !activity.SemanticAction.Entities.ContainsKey(NameInActivity))
            {
                return null;
            }

            return new HashSet<string>((activity.SemanticAction.Entities[NameInActivity].Properties as IEnumerable<KeyValuePair<string, JToken>>).Select(pair => pair.Key));
        }

        public static string NameInActivity = "skillSwitchData";
    }
}
