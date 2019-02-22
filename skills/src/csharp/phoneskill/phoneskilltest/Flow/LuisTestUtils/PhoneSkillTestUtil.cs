using System;
using System.Collections.Generic;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Solutions.Testing.Mocks;
using PhoneSkillTest.Flow.Utterances;

namespace PhoneSkillTest.Flow.LuisTestUtils
{
    public class PhoneSkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>();

        public static MockLuisRecognizer CreateRecognizer()
        {
            if (_utterances.Count == 0)
            {
                AddUtterance(OutgoingCallUtterances.OutgoingCallNoEntities, PhoneLuis.Intent.OutgoingCall);

                AddUtterance(OutgoingCallUtterances.OutgoingCallContactName, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
                {
                    CreateEntity("contactName", "bob", 5),
                });

                AddUtterance(OutgoingCallUtterances.OutgoingCallPhoneNumber, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
                {
                    CreateEntity("phoneNumber", "0118 999 88199 9119 725 3", 5),
                });

                AddUtterance(OutgoingCallUtterances.RecipientContactName, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
                {
                    CreateEntity("contactName", "bob", 0),
                });

                AddUtterance(OutgoingCallUtterances.RecipientPhoneNumber, PhoneLuis.Intent.OutgoingCall, new List<InstanceData>()
                {
                    CreateEntity("phoneNumber", "0118 999 88199 9119 725 3", 0),
                });
            }

            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, PhoneLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        private static void AddUtterance(string userInput, PhoneLuis.Intent intent, IList<InstanceData> entities = null)
        {
            _utterances[userInput] = CreateIntent(userInput, intent, entities);
        }

        private static PhoneLuis CreateIntent(string userInput, PhoneLuis.Intent intent, IList<InstanceData> entities = null)
        {
            var result = new PhoneLuis
            {
                Text = userInput,
                Intents = new Dictionary<PhoneLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new PhoneLuis._Entities
            {
                _instance = new PhoneLuis._Entities._Instance()
            };

            if (entities != null)
            {
                IDictionary<string, List<InstanceData>> typeToEntities = new Dictionary<string, List<InstanceData>>();
                foreach (var entity in entities)
                {
                    if (!typeToEntities.TryGetValue(entity.Type, out var entityList))
                    {
                        entityList = new List<InstanceData>();
                    }

                    entityList.Add(entity);
                    typeToEntities[entity.Type] = entityList;
                }

                foreach (var (type, entityList) in typeToEntities)
                {
                    switch (type)
                    {
                        case "contactName":
                            result.Entities.contactName = GetEntityValues(entityList);
                            result.Entities._instance.contactName = entityList.ToArray();
                            break;
                        case "contactRelation":
                            result.Entities.contactRelation = GetListEntityValues(entityList);
                            result.Entities._instance.contactRelation = entityList.ToArray();
                            break;
                        case "phoneNumberType":
                            result.Entities.phoneNumberType = GetListEntityValues(entityList);
                            result.Entities._instance.phoneNumberType = entityList.ToArray();
                            break;
                        case "phoneNumber":
                            result.Entities.phoneNumber = GetEntityValues(entityList);
                            result.Entities._instance.phoneNumber = entityList.ToArray();
                            break;
                        case "phoneNumberSpelledOut":
                            result.Entities.phoneNumberSpelledOut = GetEntityValues(entityList);
                            result.Entities._instance.phoneNumberSpelledOut = entityList.ToArray();
                            break;
                        default:
                            throw new Exception($"No mapping for entity type \"{type}\"");
                    }
                }
            }

            return result;
        }

        private static InstanceData CreateEntity(string type, string text, int startIndex, string resolvedValue = null)
        {
            var entity = new InstanceData();
            entity.Type = type;
            entity.Text = text;
            entity.StartIndex = startIndex;

            // The end index is inclusive.
            entity.EndIndex = startIndex + text.Length - 1;

            if (!string.IsNullOrEmpty(resolvedValue))
            {
                // TODO Figure out the proper types to use here.
                entity.Properties.Add("resolution", new Dictionary<string, IList<string>>()
                {
                    { "values", new List<string>() { resolvedValue } },
                });
            }

            return entity;
        }

        private static string[] GetEntityValues(IList<InstanceData> entities)
        {
            var values = new string[entities.Count];
            for (int i = 0; i < entities.Count; ++i)
            {
                values[i] = entities[i].Text;
            }

            return values;
        }

        private static string[][] GetListEntityValues(IList<InstanceData> entities)
        {
            var values = new string[entities.Count][];
            for (int i = 0; i < entities.Count; ++i)
            {
                // TODO Figure out the proper way to fill the 2d array.
                values[i] = new string[1] { entities[i].Text };
            }

            return values;
        }
    }
}
