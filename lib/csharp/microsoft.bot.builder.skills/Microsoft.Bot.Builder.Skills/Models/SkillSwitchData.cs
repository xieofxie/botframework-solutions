using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Skills.Models
{
    /// <summary>
    /// Skill id to skill switches set. Only present keys (skill id) will be routed to.
    /// If HashSet<string> is null or empty, for old skill, it means it is supported with full functions and for new skill, it means no switches are set.
    /// </summary>
    public class SkillSwitchData : Dictionary<string, HashSet<string>>
    {
    }
}
