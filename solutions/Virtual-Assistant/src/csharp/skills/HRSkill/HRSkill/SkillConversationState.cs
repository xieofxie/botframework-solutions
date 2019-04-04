using Luis;
using Microsoft.Bot.Builder.Dialogs;

namespace HRSkill
{
    public class SkillConversationState : DialogState
    {
        public SkillConversationState()
        {
        }

        public string Token { get; internal set; }

        public HRSkillLU LuisResult { get; set; }

        public void Clear()
        {
        }
    }
}