using Autofac;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Solutions.Middleware.Telemetry;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Testing;
using Microsoft.Bot.Solutions.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HospitalitySkillTests.Flow.LuisTestUtils;
using HospitalitySkill.Dialogs.Main.Resources;
using HospitalitySkill.Dialogs.Sample.Resources;
using HospitalitySkill.Dialogs.Shared.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HospitalitySkillTests.Flow
{
    public class HospitalitySkillTestBase : BotTestBase
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
                    { "HospitalitySkill", HospitalitySkillTestUtil.CreateRecognizer() }
                }
            });

            builder.RegisterInstance(new BotStateSet(UserState, ConversationState));
            Container = builder.Build();

            ResponseManager = new ResponseManager(
            new IResponseIdCollection[]
            {
                    new MainResponses(),
                    new SharedResponses(),
                    new SampleResponses()
            }, Services.LocaleConfigurations.Keys.ToArray());
        }

        public TestFlow GetTestFlow()
        {
            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(ConversationState));

            var testFlow = new TestFlow(adapter, async (context, token) =>
            {
                var bot = BuildBot() as HospitalitySkill.HospitalitySkill;
                await bot.OnTurnAsync(context, CancellationToken.None);
            });

            return testFlow;
        }

        public override IBot BuildBot()
        {
            return new HospitalitySkill.HospitalitySkill(Services, ConversationState, UserState, TelemetryClient, false, ResponseManager, null);
        }
    }
}