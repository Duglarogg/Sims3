using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class PuddlesSetting : IntegerSettingOption<GameObject>, IAlienPregnancyOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mNumPuddles;
            set => Aliens.Settings.mNumPuddles = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mPregnancyChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "Puddles";
        }

        protected override int Validate(int value)
        {
            if (value < 0)
                return 0;

            return value;
        }
    }
}
