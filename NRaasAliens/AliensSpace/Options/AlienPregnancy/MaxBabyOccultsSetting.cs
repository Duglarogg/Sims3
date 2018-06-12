using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class MaxBabyOccultsSetting : IntegerSettingOption<GameObject>, IAlienPregnancyOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mMaxBabyOccults;
            set => Aliens.Settings.mMaxBabyOccults = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mAllowOccultBabies;
        }

        public override string GetTitlePrefix()
        {
            return "MaxBabyOccults";
        }

        protected override int Validate(int value)
        {
            if (value < 1)
                return 1;

            return value;
        }
    }
}
