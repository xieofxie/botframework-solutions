// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Skills.Auth;
using Microsoft.Bot.Builder.Skills.Models.Manifest;
using Microsoft.Bot.Builder.Solutions;
using Microsoft.Bot.Builder.Solutions.Authentication;
using Microsoft.Bot.Builder.Solutions.Feedback;
using Microsoft.Bot.Builder.Solutions.Middleware;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Builder.Solutions.Testing;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PointOfInterestSkill.Responses.Shared;
using VirtualAssistantSample.Bots;
using VirtualAssistantSample.Dialogs;
using VirtualAssistantSample.Services;
using VirtualAssistantSample.Tests.Mocks;
using VirtualAssistantSample.Tests.Utilities;

namespace VirtualAssistantSample.Tests
{
    public class SkillTestBase : Microsoft.Bot.Builder.Solutions.Testing.BotTestBase
    {
        public IServiceCollection Services { get; set; }

        public List<Tuple<CancellationTokenSource, Task>> Skills { get; set; } = new List<Tuple<CancellationTokenSource, Task>>();

        [TestInitialize]
        public override void Initialize()
        {
            var id = "appId";
            var password = "password";
            var port = 3980;

            Skills.Add(PointOfInterestSkillTests.Flow.PointOfInterestStartup.StartSkill(id, password, port));

            // Follow Startup
            Services = new ServiceCollection();

            // Load settings
            var settings = new BotSettings();
            settings.MicrosoftAppId = id;
            settings.MicrosoftAppPassword = password;
            settings.Skills = new List<SkillManifest>();
            settings.Skills.Add(new SkillManifest
            {
                Id = "poiSkill",
                MSAappId = settings.MicrosoftAppId,
                Endpoint = new Uri($"http://localhost:{port}/api/skill/messages"),
                Actions = new List<Microsoft.Bot.Builder.Skills.Models.Manifest.Action>
                {
                    new Microsoft.Bot.Builder.Skills.Models.Manifest.Action
                    {
                        Id = "action",
                        Definition = new ActionDefinition
                        {
                            Slots = new List<Slot>
                            {
                                new Slot
                                {
                                    Name = "location",
                                    Types = new List<string> { "string" }
                                }
                            }
                        }
                    }
                }
            });
            Services.AddSingleton(settings);

            // Configure credentials
            Services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            var appCredentials = new MicrosoftAppCredentials(settings.MicrosoftAppId, settings.MicrosoftAppPassword);
            Services.AddSingleton(appCredentials);

            // Configure telemetry
            Services.AddApplicationInsightsTelemetry();
            var telemetryClient = new NullBotTelemetryClient();
            Services.AddSingleton<IBotTelemetryClient>(telemetryClient);
            //Services.AddBotApplicationInsights(telemetryClient);

            // Configure bot services
            Services.AddSingleton(new BotServices()
            {
                CognitiveModelSets = new Dictionary<string, CognitiveModelSet>
                {
                    {
                        "en", new CognitiveModelSet
                        {
                            DispatchService = DispatchTestUtil.CreateRecognizer(),
                            LuisServices = new Dictionary<string, ITelemetryRecognizer>
                            {
                                { "General", GeneralTestUtil.CreateRecognizer() }
                            },
                            QnAServices = new Dictionary<string, ITelemetryQnAMaker>
                            {
                                { "Faq", FaqTestUtil.CreateRecognizer() },
                                { "Chitchat", ChitchatTestUtil.CreateRecognizer() }
                            }
                        }
                    }
                }
            });

            // Configure responses
            ResponseManager = new ResponseManager(
                new string[] { "en", "de", "es", "fr", "it", "zh" },
                new POISharedResponses());
            Services.AddSingleton(ResponseManager);

            // Configure storage
            Services.AddSingleton<IStorage, MemoryStorage>();
            Services.AddSingleton<UserState>();
            Services.AddSingleton<ConversationState>();
            Services.AddSingleton(sp =>
            {
                var userState = sp.GetService<UserState>();
                var conversationState = sp.GetService<ConversationState>();
                return new BotStateSet(userState, conversationState);
            });

            // Register dialogs
            Services.AddTransient<CancelDialog>();
            Services.AddTransient<EscalateDialog>();
            Services.AddTransient<MainDialog, MockMainDialog>();
            Services.AddTransient<OnboardingDialog>();

            // Register skill dialogs
            Services.AddTransient(sp =>
            {
                var userState = sp.GetService<UserState>();
                var skillDialogs = new List<SkillDialog>();

                foreach (var skill in settings.Skills)
                {
                    var authDialog = BuildAuthDialog(skill, settings, appCredentials);
                    var credentials = new MockMicrosoftAppCredentialsEx(settings.MicrosoftAppId, settings.MicrosoftAppPassword, skill.MSAappId);
                    skillDialogs.Add(new SkillDialog(skill, credentials, telemetryClient, userState, authDialog));
                }

                return skillDialogs;
            });

            // Configure adapters
            Services.AddSingleton<TestAdapter, DefaultTestAdapter>();

            // Configure bot
            Services.AddTransient<IBot, DialogBot<MainDialog>>();
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            foreach (var tuple in Skills)
            {
                tuple.Item1.Cancel();
                try
                {
                    tuple.Item2.Wait();
                }
                catch (OperationCanceledException e)
                {
                }
                finally
                {
                    tuple.Item1.Dispose();
                }
            }
        }

        public TestFlow GetTestFlow()
        {
            var sp = Services.BuildServiceProvider();
            var adapter = sp.GetService<TestAdapter>()
                .Use(new FeedbackMiddleware(sp.GetService<ConversationState>(), sp.GetService<IBotTelemetryClient>()))
                .Use(new EventDebuggerMiddleware());

            var testFlow = new TestFlow(adapter, async (context, token) =>
            {
                var bot = sp.GetService<IBot>();
                await bot.OnTurnAsync(context, CancellationToken.None);
            });

            return testFlow;
        }

        // This method creates a MultiProviderAuthDialog based on a skill manifest.
        private MultiProviderAuthDialog BuildAuthDialog(SkillManifest skill, BotSettings settings, MicrosoftAppCredentials appCredentials)
        {
            if (skill.AuthenticationConnections?.Count() > 0)
            {
                if (settings.OAuthConnections != null && settings.OAuthConnections.Any(o => skill.AuthenticationConnections.Any(s => s.ServiceProviderId == o.Provider)))
                {
                    var oauthConnections = settings.OAuthConnections.Where(o => skill.AuthenticationConnections.Any(s => s.ServiceProviderId == o.Provider)).ToList();
                    return new MultiProviderAuthDialog(oauthConnections, appCredentials);
                }
                else
                {
                    throw new Exception($"You must configure at least one supported OAuth connection to use this skill: {skill.Name}.");
                }
            }

            return null;
        }
    }
}
