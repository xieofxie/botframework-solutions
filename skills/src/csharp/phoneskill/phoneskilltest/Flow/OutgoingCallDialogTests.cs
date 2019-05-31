using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhoneSkill.Models;
using PhoneSkill.Responses.OutgoingCall;
using PhoneSkillTest.Flow.Utterances;
using PhoneSkillTest.TestDouble;

namespace PhoneSkillTest.Flow
{
    [TestClass]
    public class OutgoingCallDialogTests : PhoneSkillTestBase
    {
        [TestMethod]
        public async Task Test_OutgoingCall_PhoneNumber()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallPhoneNumber)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "0118 999 88199 9119 725 3" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "0118 999 88199 9119 725 3",
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_RecipientPromptPhoneNumber()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallNoEntities)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.RecipientPrompt))
               .Send(OutgoingCallUtterances.RecipientPhoneNumber)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "0118 999 88199 9119 725 3" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "0118 999 88199 9119 725 3",
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_ContactName()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallContactName)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "Bob Botter" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "555 666 6666",
                   Contact = StubContactProvider.BobBotter,
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_RecipientPromptContactName()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallNoEntities)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.RecipientPrompt))
               .Send(OutgoingCallUtterances.RecipientContactName)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "Bob Botter" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "555 666 6666",
                   Contact = StubContactProvider.BobBotter,
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_ContactName_ContactSelectionByIndex()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallContactNameMultipleMatches)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.ContactSelection, new StringDictionary()
               {
                   { "contactName", "narthwani" },
               },
               new List<string>()
               {
                   "Ditha Narthwani",
                   "Sanjay Narthwani",
               }))
               .Send(OutgoingCallUtterances.SelectionFirst)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "Ditha Narthwani" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "555 777 7777",
                   Contact = StubContactProvider.DithaNarthwani,
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_ContactName_ContactSelectionByPartialName()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallContactNameMultipleMatches)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.ContactSelection, new StringDictionary()
               {
                   { "contactName", "narthwani" },
               },
               new List<string>()
               {
                   "Ditha Narthwani",
                   "Sanjay Narthwani",
               }))
               .Send(OutgoingCallUtterances.ContactSelectionPartialName)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "Sanjay Narthwani" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "555 888 8888",
                   Contact = StubContactProvider.SanjayNarthwani,
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_ContactName_ContactSelectionByFullName()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallContactNameMultipleMatches)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.ContactSelection, new StringDictionary()
               {
                   { "contactName", "narthwani" },
               },
               new List<string>()
               {
                   "Ditha Narthwani",
                   "Sanjay Narthwani",
               }))
               .Send(OutgoingCallUtterances.ContactSelectionFullName)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "Sanjay Narthwani" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "555 888 8888",
                   Contact = StubContactProvider.SanjayNarthwani,
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_ContactName_PhoneNumberSelectionByIndex()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallContactNameMultipleNumbers)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.PhoneNumberSelection, new StringDictionary()
               {
                   { "contact", "Andrew Smith" },
               },
               new List<string>()
               {
                   "Home",
                   "Business",
                   "Mobile",
               }))
               .Send(OutgoingCallUtterances.SelectionFirst)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "Andrew Smith" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "555 111 1111",
                   Contact = new ContactCandidate
                   {
                       Name = StubContactProvider.AndrewSmith.Name,
                       PhoneNumbers = new List<PhoneNumber>
                       {
                           StubContactProvider.AndrewSmith.PhoneNumbers[0],
                       },
                   },
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_ContactName_PhoneNumberSelectionByStandardizedType()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallContactNameMultipleNumbers)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.PhoneNumberSelection, new StringDictionary()
               {
                   { "contact", "Andrew Smith" },
               },
               new List<string>()
               {
                   "Home",
                   "Business",
                   "Mobile",
               }))
               .Send(OutgoingCallUtterances.PhoneNumberSelectionStandardizedType)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCallWithPhoneNumberType, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "Andrew Smith" },
                   { "phoneNumberType", "Mobile" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "555 333 3333",
                   Contact = new ContactCandidate
                   {
                       Name = StubContactProvider.AndrewSmith.Name,
                       PhoneNumbers = new List<PhoneNumber>
                       {
                           StubContactProvider.AndrewSmith.PhoneNumbers[2],
                       },
                   },
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_ContactName_PhoneNumberSelectionByStandardizedTypeThenIndex()
        {
            await GetTestFlow()
               .Send(OutgoingCallUtterances.OutgoingCallContactNameMultipleNumbersWithSameType)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.PhoneNumberSelection, new StringDictionary()
               {
                   { "contact", "Eve Smith" },
               },
               new List<string>()
               {
                   "Home",
                   "Mobile",
                   "Mobile",
               }))
               .Send(OutgoingCallUtterances.PhoneNumberSelectionStandardizedType)
               .AssertReply(Message(OutgoingCallResponses.PhoneNumberSelection, new StringDictionary()
               {
                   { "contact", "Eve Smith" },
               },
               new List<string>()
               {
                   // TODO this isn't useful for the user
                   "Mobile",
                   "Mobile",
               }))
               .Send(OutgoingCallUtterances.SelectionFirst)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCallWithPhoneNumberType, new StringDictionary()
               {
                   { "contactOrPhoneNumber", "Eve Smith" },
                   { "phoneNumberType", "Mobile" },
               }))
               .AssertReply(OutgoingCallEvent(new OutgoingCall
               {
                   Number = "555 101 0101",
                   Contact = new ContactCandidate
                   {
                       Name = StubContactProvider.EveSmith.Name,
                       PhoneNumbers = new List<PhoneNumber>
                       {
                           StubContactProvider.EveSmith.PhoneNumbers[1],
                       },
                   },
               }))
               .StartTestAsync();
        }
    }
}
