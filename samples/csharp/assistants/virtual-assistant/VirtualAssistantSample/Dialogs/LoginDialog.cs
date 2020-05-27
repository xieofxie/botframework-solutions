// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Authentication;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Util;
using Microsoft.Extensions.DependencyInjection;
using VirtualAssistantSample.Models;
using VirtualAssistantSample.Services;

namespace VirtualAssistantSample.Dialogs
{
    public class LoginDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly UserReferenceState _userReference;

        public LoginDialog(
            IServiceProvider serviceProvider)
            : base(nameof(LoginDialog))
        {
            var userState = serviceProvider.GetService<UserState>();
            _accessor = userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _userReference = serviceProvider.GetService<UserReferenceState>();

            Settings = serviceProvider.GetService<BotSettings>();

            if (!Settings.OAuthConnections.Any())
            {
                throw new Exception("You must configure an authentication connection before using this component.");
            }

            AddDialog(new MultiProviderAuthDialog(Settings.OAuthConnections));

            var sample = new WaterfallStep[]
           {
                GetAuthTokenAsync,
                AfterGetAuthTokenAsync,
           };

            AddDialog(new WaterfallDialog(nameof(LoginDialog), sample));
            InitialDialogId = nameof(LoginDialog);
        }

        protected BotSettings Settings { get; }

        protected async Task<DialogTurnResult> GetAuthTokenAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            try
            {
                return await sc.PromptAsync(nameof(MultiProviderAuthDialog), new PromptOptions(), cancellationToken);
            }
            catch (SkillException ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
            catch (Exception ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        protected async Task<DialogTurnResult> AfterGetAuthTokenAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default)
        {
            try
            {
                // When the user authenticates interactively we pass on the tokens/Response event which surfaces as a JObject
                // When the token is cached we get a TokenResponse object.
                if (sc.Result is ProviderTokenResponse providerTokenResponse)
                {
                    if (_userReference.StartPollNotification(sc.Context, providerTokenResponse.TokenResponse.Token))
                    {
                        await sc.Context.SendActivityAsync("Notification on");
                    }
                }

                return await sc.NextAsync(cancellationToken: cancellationToken);
            }
            catch (SkillException ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
            catch (Exception ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        protected async Task HandleDialogExceptionsAsync(WaterfallStepContext sc, Exception ex, CancellationToken cancellationToken)
        {
            // send trace back to emulator
            var trace = new Activity(type: ActivityTypes.Trace, text: $"DialogException: {ex.Message}, StackTrace: {ex.StackTrace}");
            await sc.Context.SendActivityAsync(trace, cancellationToken);
        }
    }
}
