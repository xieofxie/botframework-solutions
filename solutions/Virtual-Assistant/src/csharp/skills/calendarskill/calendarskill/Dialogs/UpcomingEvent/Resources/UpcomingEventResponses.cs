  
// https://docs.microsoft.com/en-us/visualstudio/modeling/t4-include-directive?view=vs-2017
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Solutions.Dialogs;

namespace CalendarSkill.Dialogs.UpcomingEvent.Resources
{
    /// <summary>
    /// Contains bot responses.
    /// </summary>
    public static class UpcomingEventResponses
    {
        private static readonly ResponseManager _responseManager;

        static UpcomingEventResponses()
        {
            var dir = Path.GetDirectoryName(typeof(UpcomingEventResponses).Assembly.Location);
            var resDir = Path.Combine(dir, @"Dialogs\UpcomingEvent\Resources");
            _responseManager = new ResponseManager(resDir, "UpcomingEventResponses");
        }

        // Generated accessors
        public static BotResponse UpcomingMeetingMessage => GetBotResponse();

        public static BotResponse ShowUpcomingMeetingCallInMessage => GetBotResponse();

        public static BotResponse UpcomingMeetingNavigateToLocationMessage => GetBotResponse();

        public static BotResponse ShowNextMeetingMessage => GetBotResponse();

        public static BotResponse ShowMultipleNextMeetingMessage => GetBotResponse();

        private static BotResponse GetBotResponse([CallerMemberName] string propertyName = null)
        {
            return _responseManager.GetBotResponse(propertyName);
        }
    }
}