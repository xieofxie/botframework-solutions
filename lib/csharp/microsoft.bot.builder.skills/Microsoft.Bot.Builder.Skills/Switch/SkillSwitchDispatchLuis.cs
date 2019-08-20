using Microsoft.Bot.Builder.Skills.Auth;
using Microsoft.Bot.Builder.Skills.Models.Manifest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Skills.Switch
{
    public class SkillSwitchDispatchLuis
    {
        private readonly IStatePropertyAccessor<SkillContext> _skillContextAccessor;
        private readonly ISkillSwitchManager _skillSwitchManager;

        public SkillSwitchDispatchLuis(
            UserState userState,
            ISkillSwitchManager skillSwitchManager)
        {
            _skillContextAccessor = userState.CreateProperty<SkillContext>(nameof(SkillContext));
            _skillSwitchManager = skillSwitchManager;
        }

        public async Task<(T intent, double score)> TopIntentWithSkillSwitch<T>(ITurnContext turnContext, List<SkillManifest> skills, Dictionary<T, IntentScore> intents, T tNone)
        {
            var skillSwitchData = await _skillSwitchManager.GetSkillSwitch(turnContext);

            var skillContext = await _skillContextAccessor.GetAsync(turnContext, () => new SkillContext());
            skillContext.SetSkillSwitchData(skillSwitchData);

            T maxIntent = tNone;
            var max = 0.0;
            foreach (var entry in intents)
            {
                if (SkillRouter.IsSkill(skills, entry.Key.ToString()) != null)
                {
                    if (skillSwitchData == null || !skillSwitchData.ContainsKey(entry.Key.ToString()))
                    {
                        continue;
                    }
                }

                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}
