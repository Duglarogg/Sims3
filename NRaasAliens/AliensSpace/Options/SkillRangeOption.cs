using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options
{
    public abstract class SkillRangeOption : IntegerRangeSettingOption<GameObject>
    {
        protected abstract int[] Setting { get; }

        protected override Pair<int, int> Value
        {
            get => new Pair<int, int>(Setting[0], Setting[1]);
            set => Validate(value.First, value.Second);
        }

        protected override Pair<int, int> Validate(int value1, int value2)
        {
            Pair<int, int> result = base.Validate(value1, value2);

            if (result.First < 0)
                result.First = 0;

            if (result.Second > 10)
                result.Second = 10;

            Setting[0] = result.First;
            Setting[1] = result.Second;

            return result;
        }
    }
}
