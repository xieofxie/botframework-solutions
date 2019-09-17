// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.Bot.Builder.Solutions.Proactive;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Builder.Solutions.TaskExtensions;
using Microsoft.Bot.Builder.Solutions.Testing;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PointOfInterestSkill.Adapters;
using PointOfInterestSkill.Bots;
using PointOfInterestSkill.Controllers;
using PointOfInterestSkill.Dialogs;
using PointOfInterestSkill.Responses.CancelRoute;
using PointOfInterestSkill.Responses.FindPointOfInterest;
using PointOfInterestSkill.Responses.Main;
using PointOfInterestSkill.Responses.Route;
using PointOfInterestSkill.Responses.Shared;
using PointOfInterestSkill.Services;
using PointOfInterestSkillTests.API.Fakes;
using PointOfInterestSkillTests.Flow.Utterances;

namespace PointOfInterestSkillTests.Flow
{
    // Follow Startup
    public class PointOfInterestStartup
    {
        public static Tuple<CancellationTokenSource, Task> StartSkill(string id, string password, int port)
        {
            var tokenSource = new CancellationTokenSource();
            var task = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    ConfigureServices(services, id, password);
                })
                .Configure(app =>
                {
                    Configure(app);
                })
                .UseUrls($"http://localhost:{port}")
                .Build()
                .StartAsync(tokenSource.Token);
            return new Tuple<CancellationTokenSource, Task>(tokenSource, task);
        }

        private static void ConfigureServices(IServiceCollection services, string id, string password)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);

            // Load settings
            var settings = new BotSettings();
            settings.MicrosoftAppId = id;
            settings.MicrosoftAppPassword = password;
            settings.AzureMapsKey = MockData.Key;
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

            // Configure bot Services
            services.AddSingleton(new BotServices()
            {
                CognitiveModelSets = new Dictionary<string, CognitiveModelSet>
                {
                    {
                        "en", new CognitiveModelSet()
                        {
                            LuisServices = new Dictionary<string, ITelemetryRecognizer>
                            {
                                { "General", new Fakes.MockGeneralLuisRecognizer() },
                                {
                                    "PointOfInterest", new Fakes.MockPointOfInterestLuisRecognizer(
                                    new FindParkingUtterances(),
                                    new FindPointOfInterestUtterances(),
                                    new RouteFromXToYUtterances())
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

            // Configure HttpContext required for path resolution
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Configure responses
            services.AddSingleton(sp => new ResponseManager(
                new string[] { "en", "de", "es", "fr", "it", "zh" },
                new CancelRouteResponses(),
                new FindPointOfInterestResponses(),
                new POIMainResponses(),
                new RouteResponses(),
                new POISharedResponses()));

            // register dialogs
            services.AddTransient<MainDialog>();
            services.AddTransient<CancelRouteDialog>();
            services.AddTransient<FindParkingDialog>();
            services.AddTransient<FindPointOfInterestDialog>();
            services.AddTransient<RouteDialog>();
            services.AddTransient<GetDirectionsDialog>();

            // Configure adapters
            //services.AddTransient<IBotFrameworkHttpAdapter, DefaultAdapter>();
            services.AddTransient<SkillWebSocketBotAdapter, POISkillWebSocketBotAdapter>();
            services.AddTransient<SkillWebSocketAdapter>();

            // Register WhiteListAuthProvider
            services.AddSingleton<IWhitelistAuthenticationProvider, WhitelistAuthenticationProvider>();

            // Configure bot
            services.AddTransient<MainDialog>();
            services.AddTransient<IBot, DialogBot<MainDialog>>();

            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2#route-template-reference
            services.AddRouting();
        }

        private static void Configure(IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);
            routeBuilder.MapGet("api/skill/messages", context =>
            {
                var bot = context.RequestServices.GetRequiredService<IBot>();
                return context.RequestServices.GetRequiredService<SkillWebSocketAdapter>().ProcessAsync(context.Request, context.Response, bot);
            });

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouter(routeBuilder.Build())
                .UseMvc();
        }
    }
}
