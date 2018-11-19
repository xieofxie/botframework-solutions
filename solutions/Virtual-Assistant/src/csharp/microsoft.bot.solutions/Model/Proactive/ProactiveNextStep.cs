using System.Dynamic;

namespace Microsoft.Bot.Solutions.Model.Proactive
{
    public class ProactiveNextStep
    {
        public ProactiveNextStepActionType Action { get; set; }

        public string Parameters { get; set; }
    }
}