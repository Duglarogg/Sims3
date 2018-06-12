using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class PregnancyLengthSetting : IntegerSettingOption<GameObject>, IAlienPregnancyOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mPregnancyLength;

            set
            {
                Aliens.Settings.mPregnancyLength = Validate(value);
                Aliens.Settings.UpdatePregnancyTuning();
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mPregnancyChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "PregnancyLength";
        }

        protected override int Validate(int value)
        {
            if (value < 1)
                return 1;

            return value;
        }
    }
}
