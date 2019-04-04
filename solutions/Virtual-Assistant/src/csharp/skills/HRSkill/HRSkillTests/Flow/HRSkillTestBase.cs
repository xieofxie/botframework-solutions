using Autofac;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Solutions.Telemetry;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Testing;
using Microsoft.Bot.Solutions.Testing.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HRSkillTests.Flow.LuisTestUtils;
using HRSkill.Dialogs.Main.Resources;
using HRSkill.Dialogs.Sample.Resources;
using HRSkill.Dialogs.Shared.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HRSkillTests.Flow
{
    public class HRSkillTestBase : BotTestBase
    {
        public ConversationState ConversationState { get; set; }

        public UserState UserState { get; set; }

        public IBotTelemetryClient TelemetryClient { get; set; }

        public SkillConfigurationBase Services { get; set; }

        [TestInitialize]
        public override void Initialize()
        {
            var builder = new ContainerBuilder();

            ConversationState = new ConversationState(new MemoryStorage());
            UserState = new UserState(new MemoryStorage());
            TelemetryClient = new NullBotTelemetryClient();
            Services = new MockSkillConfiguration();

            Services.LocaleConfigurations.Add("en", new LocaleConfiguration()
            {
                Locale = "en-us",
                LuisServices = new Dictionary<string, ITelemetryLuisRecognizer>
                {
                    { "general", GeneralTestUtil.CreateRecognizer() },
                    { "HRSkill", HRSkillTestUtil.CreateRecognizer() }
                }
            });

            builder.RegisterInstance(new BotStateSet(UserState, ConversationState));
            Container = builder.Build();

            ResponseManager = new ResponseManager(
                Services.LocaleConfigurations.Keys.ToArray(),
                new MainResponses(),
                new SharedResponses(),
                new SampleResponses());
        }

        public TestFlow GetTestFlow()
        {
            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(ConversationState));

            var testFlow = new TestFlow(adapter, async (context, token) =>
            {
                var bot = BuildBot() as HRSkill.HRSkill;
                await bot.OnTurnAsync(context, CancellationToken.None);
            });

            return testFlow;
        }

        public override IBot BuildBot()
        {
            return new HRSkill.HRSkill(Services, ConversationState, UserState, TelemetryClient, false, ResponseManager, null);
        }
    }
}