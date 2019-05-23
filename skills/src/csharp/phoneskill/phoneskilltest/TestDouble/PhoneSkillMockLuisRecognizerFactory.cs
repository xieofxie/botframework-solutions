using System.Collections.Generic;
using Luis;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Solutions.Testing.Mocks;
using PhoneSkillTest.Flow.Utterances;

namespace PhoneSkillTest.TestDouble
{
    public class PhoneSkillMockLuisRecognizerFactory
    {
        public static MockLuisRecognizer CreateMockGeneralLuisRecognizer()
        {
            var builder = new MockLuisRecognizerBuilder<General, General.Intent>();

            builder.AddUtterance(GeneralUtterances.Cancel, General.Intent.Cancel);
            builder.AddUtterance(GeneralUtterances.Escalate, General.Intent.Escalate);
            builder.AddUtterance(GeneralUtterances.Help, General.Intent.Help);
            builder.AddUtterance(GeneralUtterances.Incomprehensible, General.Intent.None);
            builder.AddUtterance(GeneralUtterances.Logout, General.Intent.Logout);

            return builder.Build();
        }

        public static MockLuisRecognizer CreateMockPhoneLuisRecognizer()
        {
            var builder = new MockLuisRecognizerBuilder<PhoneLuis, PhoneLuis.Intent>();

            builder.AddUtterance(GeneralUtterances.Incomprehensible, PhoneLuis.Intent.None);

            builder.AddUtterance(OutgoingCallUtterances.OutgoingCallContactName, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
            {
                MockLuisUtil.CreateEntity("contactName", "bob", 5),
            });

            builder.AddUtterance(OutgoingCallUtterances.OutgoingCallContactNameMultipleMatches, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
            {
                MockLuisUtil.CreateEntity("contactName", "narthwani", 5),
            });

            builder.AddUtterance(OutgoingCallUtterances.OutgoingCallNoEntities, PhoneLuis.Intent.OutgoingCall);

            builder.AddUtterance(OutgoingCallUtterances.OutgoingCallPhoneNumber, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
            {
                MockLuisUtil.CreateEntity("phoneNumber", "0118 999 88199 9119 725 3", 5),
            });

            builder.AddUtterance(OutgoingCallUtterances.RecipientContactName, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
            {
                MockLuisUtil.CreateEntity("contactName", "bob", 0),
            });

            builder.AddUtterance(OutgoingCallUtterances.RecipientPhoneNumber, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
            {
                MockLuisUtil.CreateEntity("phoneNumber", "0118 999 88199 9119 725 3", 0),
            });

            return builder.Build();
        }

        public static MockLuisRecognizer CreateMockContactSelectionLuisRecognizer()
        {
            var builder = new MockLuisRecognizerBuilder<ContactSelectionLuis, ContactSelectionLuis.Intent>();

            builder.AddUtterance(OutgoingCallUtterances.ContactSelection1st, ContactSelectionLuis.Intent.ContactSelection, new List<InstanceData>()
            {
                MockLuisUtil.CreateEntity("index", "1st", 4),
            });

            builder.AddUtterance(OutgoingCallUtterances.ContactSelectionFirst, ContactSelectionLuis.Intent.ContactSelection, new List<InstanceData>()
            {
                MockLuisUtil.CreateEntity("index", "first", 4),
            });

            builder.AddUtterance(OutgoingCallUtterances.ContactSelectionLast, ContactSelectionLuis.Intent.ContactSelection, new List<InstanceData>()
            {
                MockLuisUtil.CreateEntity("index", "last", 4),
            });

            return builder.Build();
        }
    }
}
