using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Solutions.Middleware
{
    public class SpeakVoiceMiddleware : IMiddleware
    {
        private readonly string _voiceName;
        private readonly string _locale;

        public SpeakVoiceMiddleware(string locale, string voiceName)
        {
            _voiceName = voiceName;
            _locale = locale;
        }

        public Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                foreach (var activity in activities)
                {
                    switch (activity.Type)
                    {
                        case ActivityTypes.Message:
                            activity.Speak = activity.Speak ?? activity.Text;
                            if (activity.ChannelId.Equals("directlinespeech"))
                            {
                                activity.Speak = SsmlDecorator.Decorate(activity.Speak, _locale, _voiceName);
                            }

                            break;
                    }
                }

                return await nextSend().ConfigureAwait(false);
            });

            return next(cancellationToken);
        }
    }
}
