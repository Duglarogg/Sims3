using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienPregnancy
{
    public class ImpregnationChanceSetting : IntegerSettingOption<GameObject>, IAlienPregnancyOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mImpregnationChance;
            }

            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                Abductor.Settings.mImpregnationChance = value;
            }
        }

        public override string GetTitlePrefix()
        {
            return "ImpregnationChance";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
