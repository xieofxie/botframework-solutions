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

        public string DelayedOrderAction { get; set; }

        public bool ContactCustomer { get; set; }

        public string Company { get; set; }

        public void Clear()
        {
            Company = null;
            DelayedOrderAction = null;
            LuisResult = null;
            ContactCustomer = false;
        }
    }
}