using Luis;
using Microsoft.Bot.Builder.Dialogs;

namespace HospitalitySkill
{
    public class SkillConversationState : DialogState
    {
        public SkillConversationState()
        {
        }

        public string Token { get; internal set; }

        public HospitalitySkillLU LuisResult { get; set; }

        public void Clear()
        {
        }
    }
}