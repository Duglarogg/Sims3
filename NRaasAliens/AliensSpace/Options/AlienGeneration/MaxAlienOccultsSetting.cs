using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class MaxAlienOccultsSetting : IntegerSettingOption<GameObject>, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mMaxAlienOccults;
            set => Aliens.Settings.mMaxAlienOccults = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mAllowOccultAliens;
        }

        public override string GetTitlePrefix()
        {
            return "MaxAlienOccults";
        }

        protected override int Validate(int value)
        {
            if (value < 1)
                return 1;

            return value;
        }
    }
}
