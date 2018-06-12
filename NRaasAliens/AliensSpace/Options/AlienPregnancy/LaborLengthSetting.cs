using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class LaborLengthSetting : IntegerSettingOption<GameObject>, IAlienPregnancyOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mLaborLength;
            set
            {
                Aliens.Settings.mLaborLength = Validate(value);
                Aliens.Settings.UpdatePregnancyTuning();
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mPregnancyChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "LaborLength";
        }

        protected override int Validate(int value)
        {
            int max = (int)Math.Round((1f / 9f) * Aliens.Settings.mPregnancyLength * 24f);

            if (value < 1)
                return 1;

            if (value > max)
                return max;

            return value;
        }
    }
}
