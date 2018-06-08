using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class OccultAlienChanceSetting : IntegerSettingOption<GameObject>, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mOccultAlienChance;
            set => Aliens.Settings.mOccultAlienChance = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mAllowOccultAliens;
        }

        public override string GetTitlePrefix()
        {
            return "OccultAlienChance";
        }

        protected override int Validate(int value)
        {
            if (value < 1)
                return 1;

            if (value > 100)
                return 100;

            return value;
        }
    }
}
