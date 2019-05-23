using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;

namespace PhoneSkillTest.TestDouble
{
    public class MockLuisUtil
    {
        /// <summary>
        /// Create an entity to be returned by a MockLuisRecognizer.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <param name="text">The extracted query substring.</param>
        /// <param name="startIndex">The start index of the extracted substring in the query. (The end index is calculated automatically.)</param>
        /// <param name="resolvedValue">The resolved value of the entity.</param>
        /// <returns>The entity.</returns>
        public static InstanceData CreateEntity(string type, string text, int startIndex, string resolvedValue = null)
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
    }
}
