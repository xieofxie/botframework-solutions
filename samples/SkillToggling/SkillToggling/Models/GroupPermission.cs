using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillToggling.Models
{
    public class GroupPermission
    {
        public string Id { get; set; }

        public List<SkillPermission> SkillPermissions { get; set; }
    }
}
