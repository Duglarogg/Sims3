using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class AdvancedTechSetting : IntegerRangeSettingOption<GameObject>, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override Pair<int, int> Value
        {
            get => Aliens.Settings.mFutureSkill;
            set => Aliens.Settings.mFutureSkill = Validate(value.First, value.Second);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP11) && Aliens.Settings.mFutureSim;
        }

        public override string GetTitlePrefix()
        {
            return "AdvancedTech";
        }

        protected override Pair<int, int> Validate(int value1, int value2)
        {
            Pair<int, int> result = base.Validate(value1, value2);

            if (result.First < 0)
                result.First = 0;

            if (result.Second > 10)
                result.Second = 10;

            return result;
        }
    }
}
