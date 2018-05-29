using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienPregnancy
{
    public class BackacheChanceSetting : IntegerSettingOption<GameObject>, IAlienPregnancyOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mBackacheChance;
            }

            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                Abductor.Settings.mBackacheChance = value;
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return (Abductor.Settings.mImpregnationChance > 0);
        }

        public override string GetTitlePrefix()
        {
            return "BackacheChance";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
