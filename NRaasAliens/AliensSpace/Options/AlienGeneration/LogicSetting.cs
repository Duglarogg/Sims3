using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
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
            get => new Pair<int,int>(Aliens.Settings.mLogicSkill[0], Aliens.Settings.mLogicSkill[1]);
            set => Validate(value.First, value.Second);
        }

        public override string GetTitlePrefix()
        {
            return "Logic";
        }

        protected override Pair<int, int> Validate(int value1, int value2)
        {
            Pair<int,int> result = base.Validate(value1, value2);

            if (result.First < 0)
                result.First = 0;

            if (result.Second > 10)
                result.Second = 10;

            Aliens.Settings.mLogicSkill[0] = result.First;
            Aliens.Settings.mLogicSkill[1] = result.Second;

            return result;
        }
    }
}
