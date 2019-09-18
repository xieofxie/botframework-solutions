// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailSkill.Adapters;
using EmailSkill.Bots;
using EmailSkill.Dialogs;
using EmailSkill.Responses.DeleteEmail;
using EmailSkill.Responses.FindContact;
using EmailSkill.Responses.ForwardEmail;
using EmailSkill.Responses.Main;
using EmailSkill.Responses.ReplyEmail;
using EmailSkill.Responses.SendEmail;
using EmailSkill.Responses.Shared;
using EmailSkill.Responses.ShowEmail;
using EmailSkill.Services;
using EmailSkillTest.Flow.Fakes;
using EmailSkillTest.Flow.Utterances;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Skills.Auth;
using Microsoft.Bot.Builder.Solutions;
using Microsoft.Bot.Builder.Solutions.Authentication;
using Microsoft.Bot.Builder.Solutions.Proactive;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Builder.Solutions.TaskExtensions;
using Microsoft.Bot.Builder.Solutions.Testing;
using Microsoft.Bot.Builder.Solutions.Util;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillTest;

namespace EmailSkillTest.Flow
{
    public class EmailBotStartup : MockSkillStartupBase
    {
        protected override void ConfigureServices(IServiceCollection services, string id, string password)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);

            // Load settings
            var settings = new BotSettings();
            settings.MicrosoftAppId = id;
            settings.MicrosoftAppPassword = password;
            settings.OAuthConnections = new List<OAuthConnection>()
            {
                new OAuthConnection() { Name = AuthenticationProvider, Provider = AuthenticationProvider }
            };
            services.AddSingleton<BotSettings>(settings);
            services.AddSingleton<BotSettingsBase>(settings);

            // Configure credentials
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton(new MicrosoftAppCredentials(settings.MicrosoftAppId, settings.MicrosoftAppPassword));

            // Configure bot state
            services.AddSingleton<IStorage>(new MemoryStorage());
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();
            services.AddSingleton(sp =>
            {
                var userState = sp.GetService<UserState>();
                var conversationState = sp.GetService<ConversationState>();
                return new BotStateSet(userState, conversationState);
            });

            // Configure telemetry
            services.AddApplicationInsightsTelemetry();
            var telemetryClient = new NullBotTelemetryClient();
            services.AddSingleton<IBotTelemetryClient>(telemetryClient);
            //Services.AddBotApplicationInsights(telemetryClient);

            // Configure bot services
            services.AddSingleton(new BotServices()
            {
                CognitiveModelSets = new Dictionary<string, CognitiveModelSet>
                {
                    {
                        "en", new CognitiveModelSet()
                        {
                            LuisServices = new Dictionary<string, ITelemetryRecognizer>
                            {
                                { "General", new MockGeneralLuisRecognizer() },
                                {
                                    "Email", new MockEmailLuisRecognizer(
                                        new ForwardEmailUtterances(),
                                        new ReplyEmailUtterances(),
                                        new DeleteEmailUtterances(),
                                        new SendEmailUtterances(),
                                        new ShowEmailUtterances())
                                }
                            }
                        }
                    }
                }
            });

            // Configure proactive
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<QueuedHostedService>();

            // Configure service manager
            services.AddSingleton<IServiceManager, MockServiceManager>();

            // Configure responses
            services.AddSingleton(sp => new ResponseManager(
                new string[] { "en", "de", "es", "fr", "it", "zh" },
                new FindContactResponses(),
                new DeleteEmailResponses(),
                new ForwardEmailResponses(),
                new EmailMainResponses(),
                new ReplyEmailResponses(),
                new SendEmailResponses(),
                new EmailSharedResponses(),
                new ShowEmailResponses()));

            // register dialogs
            services.AddTransient<MainDialog>();
            services.AddTransient<DeleteEmailDialog>();
            services.AddTransient<FindContactDialog>();
            services.AddTransient<ForwardEmailDialog>();
            services.AddTransient<ReplyEmailDialog>();
            services.AddTransient<SendEmailDialog>();
            services.AddTransient<ShowEmailDialog>();

            // Configure adapters
            //services.AddTransient<IBotFrameworkHttpAdapter, DefaultAdapter>();
            services.AddTransient<SkillWebSocketBotAdapter, EmailSkillWebSocketBotAdapter>();
            services.AddTransient<SkillWebSocketAdapter, MockSkillWebSocketAdapter>();

            // Register WhiteListAuthProvider
            services.AddSingleton<IWhitelistAuthenticationProvider, WhitelistAuthenticationProvider>();

            // Configure bot
            services.AddTransient<MainDialog>();
            services.AddTransient<IBot, DialogBot<MainDialog>>();

            ConfigData.GetInstance().MaxDisplaySize = 3;
            ConfigData.GetInstance().MaxReadSize = 3;
        }
    }
}
