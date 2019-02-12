using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Solutions.Testing.Fakes;
using HospitalitySkillTests.Flow.Utterances;
using System.Collections.Generic;

namespace HospitalitySkillTests.Flow.LuisTestUtils
{
    public class HospitalitySkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, HospitalitySkillLU.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static HospitalitySkillLU CreateIntent(string userInput, HospitalitySkillLU.Intent intent)
        {
            var result = new HospitalitySkillLU
            {
                Text = userInput,
                Intents = new Dictionary<HospitalitySkillLU.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new HospitalitySkillLU._Entities
            {
                _instance = new HospitalitySkillLU._Entities._Instance()
            };

            return result;
        }
    }
}
