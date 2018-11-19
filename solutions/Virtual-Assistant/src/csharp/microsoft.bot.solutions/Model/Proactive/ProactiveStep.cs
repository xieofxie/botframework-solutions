namespace Microsoft.Bot.Solutions.Model.Proactive
{
    public class ProactiveStep
    {
        public string Event { get; set; }

        public ProactiveNextStep Next { get; set; }
    }
}