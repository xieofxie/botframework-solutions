using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillToggling.Models
{
    public class SkillPermission
    {
        public string Id { get; set; }

        public List<string> Permissions { get; set; }
    }
}
