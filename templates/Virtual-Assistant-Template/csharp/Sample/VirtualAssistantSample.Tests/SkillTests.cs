// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using EmailSkill.Responses.Shared;
using EmailSkill.Responses.ShowEmail;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Builder.Solutions.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PointOfInterestSkill.Dialogs;
using PointOfInterestSkill.Models;
using PointOfInterestSkill.Responses.Shared;
using PointOfInterestSkillTests.Flow.Strings;
using SkillTest;
using VirtualAssistantSample.Responses.Main;
using VirtualAssistantSample.Tests.Utterances;

namespace VirtualAssistantSample.Tests
{
    [TestClass]
    public class SkillTests : SkillTestBase
    {
        [ClassInitialize]
        public static new void Initialize(TestContext testContext)
        {
            SkillTestBase.Initialize(testContext);
        }

        [TestMethod]
        public async Task Test_PoiSkill_RouteToPointOfInterestByIndexTest()
        {
            ResponseManager = new ResponseManager(
                new string[] { "en", "de", "es", "fr", "it", "zh" },
                new POISharedResponses());

            await GetTestFlow()
                .Send(PointOfInterestSkillTests.Flow.Utterances.BaseTestUtterances.LocationEventInVA)
                .Send(PointOfInterestSkillTests.Flow.Utterances.FindPointOfInterestUtterances.WhatsNearby)
                .AssertReply(AssertContains(POISharedResponses.MultipleLocationsFound, new string[] { CardStrings.Overview }))
                .Send(PointOfInterestSkillTests.Flow.Utterances.BaseTestUtterances.OptionOne)
                .AssertReply(AssertContains(null, new string[] { CardStrings.DetailsNoCall }))
                .Send(PointOfInterestSkillTests.Flow.Utterances.BaseTestUtterances.ShowDirections)
                .AssertReply(AssertContains(null, new string[] { CardStrings.Route }))
                .Send(PointOfInterestSkillTests.Flow.Utterances.BaseTestUtterances.StartNavigation)
                .AssertReply(CheckForEvent())
                .AssertReply(CompleteDialog())
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_EmailSkill_ShowEmail()
        {
            var test = new EmailSkillTest.Flow.ShowEmailFlowTests();
            test.ResponseManager = new ResponseManager(
                new string[] { "en", "de", "es", "fr", "it", "zh" },
                new EmailSharedResponses(),
                new ShowEmailResponses());
            test.ServiceManager = new EmailSkillTest.Flow.Fakes.MockServiceManager();

            await this.GetTestFlow()
                .Send(EmailSkillTest.Flow.Utterances.ShowEmailUtterances.ShowEmails)
                .AssertReply(test.ShowAuth())
                .Send(MockSkillStartupBase.MagicCode)
                .AssertReply(test.ShowEmailList())
                .AssertReplyOneOf(test.ReadOutPrompt())
                .Send(EmailSkillTest.Flow.Utterances.GeneralTestUtterances.No)
                .AssertReplyOneOf(test.NotShowingMessage())
                .AssertReply(test.ActionEndMessage())
                .StartTestAsync();
        }


        private Action<IActivity> AssertContains(string response, IList<string> cardIds)
        {
            return activity =>
            {
                var messageActivity = activity.AsMessageActivity();

                if (response == null)
                {
                    Assert.IsTrue(string.IsNullOrEmpty(messageActivity.Text));
                }
                else
                {
                    CollectionAssert.Contains(ParseReplies(response, new StringDictionary()), messageActivity.Text);
                }

                AssertSameId(messageActivity, cardIds);
            };
        }

        private void AssertSameId(IMessageActivity activity, IList<string> cardIds = null)
        {
            if (cardIds == null)
            {
                return;
            }

            for (int i = 0; i < cardIds.Count; ++i)
            {
                var card = activity.Attachments[i].Content as JObject;
                Assert.AreEqual(card["id"], cardIds[i]);
            }
        }

        /// <summary>
        /// Asserts bot response of CompleteDialog.
        /// </summary>
        /// <returns>Returns an Action with IActivity object.</returns>
        private Action<IActivity> CompleteDialog()
        {
            return activity =>
            {
                Assert.AreEqual(activity.Type, ActivityTypes.Handoff);
            };
        }

        /// <summary>
        /// Asserts bot response of Event Activity.
        /// </summary>
        /// <returns>Returns an Action with IActivity object.</returns>
        private Action<IActivity> CheckForEvent(PointOfInterestDialogBase.OpenDefaultAppType openDefaultAppType = PointOfInterestDialogBase.OpenDefaultAppType.Map)
        {
            return activity =>
            {
                var eventReceived = activity.AsEventActivity();
                Assert.IsNotNull(eventReceived, "Activity received is not an Event as expected");
                var openApp = JsonConvert.DeserializeObject<OpenDefaultApp>(JsonConvert.SerializeObject(eventReceived.Value));
                if (openDefaultAppType == PointOfInterestDialogBase.OpenDefaultAppType.Map)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(openApp.MapsUri));
                }
                else if (openDefaultAppType == PointOfInterestDialogBase.OpenDefaultAppType.Telephone)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(openApp.TelephoneUri));
                }
            };
        }
    }
}