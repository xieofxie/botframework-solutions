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

namespace SkillTest
{
    public abstract class MockSkillStartupBase
    {
        public static readonly string AuthenticationProvider = "Azure Active Directory";

        public static readonly string MagicCode = "000000";

        public Tuple<CancellationTokenSource, Task, int> StartSkill(string id, string password, int port)
        {
            var tokenSource = new CancellationTokenSource();
            var webHost = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    ConfigureServices(services, id, password);

                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2#route-template-reference
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    Configure(app);
                })
                .UseUrls($"http://localhost:{port}")
                .Build();
            webHost.StartAsync();
            var task = webHost.WaitForShutdownAsync(tokenSource.Token);
            return new Tuple<CancellationTokenSource, Task, int>(tokenSource, task, port);
        }

        // Follow Startup
        protected abstract void ConfigureServices(IServiceCollection services, string id, string password);

        private void Configure(IApplicationBuilder app)
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
