using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Solutions.Testing.Mocks;
using HRSkillTests.Flow.Utterances;
using System.Collections.Generic;

namespace HRSkillTests.Flow.LuisTestUtils
{
    public class HRSkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, HRSkillLU.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, HRSkillLU.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static HRSkillLU CreateIntent(string userInput, HRSkillLU.Intent intent)
        {
            var result = new HRSkillLU
            {
                Text = userInput,
                Intents = new Dictionary<HRSkillLU.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new HRSkillLU._Entities
            {
                _instance = new HRSkillLU._Entities._Instance()
            };

            return result;
        }
    }
}
