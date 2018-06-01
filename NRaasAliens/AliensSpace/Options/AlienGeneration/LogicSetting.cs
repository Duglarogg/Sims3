using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class LogicSetting : IntegerRangeSettingOption<GameObject>, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override Pair<int, int> Value
        {
            get => Aliens.Settings.mLogicSkill;
            set => Aliens.Settings.mLogicSkill = Validate(value.First, value.Second);
        }

        public override string GetTitlePrefix()
        {
            return "LogicSetting";
        }

        protected override Pair<int, int> Validate(int value1, int value2)
        {
            Pair<int,int> result = base.Validate(value1, value2);

            if (result.First < 0)
                result.First = 0;

            if (result.Second > 10)
                result.Second = 10;

            return result;
        }
    }
}
