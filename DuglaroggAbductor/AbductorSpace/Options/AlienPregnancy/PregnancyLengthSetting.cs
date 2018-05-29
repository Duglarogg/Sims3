using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienPregnancy
{
    public class PregnancyLengthSetting : IntegerSettingOption<GameObject>, IAlienPregnancyOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mPregnancyLength;
            }

            set
            {
                if (value < 1) value = 1;
                Abductor.Settings.mPregnancyLength = value;
                Abductor.Settings.UpdateAlienPregnancyTuning();
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return (Abductor.Settings.mImpregnationChance > 0);
        }

        public override string GetTitlePrefix()
        {
            return "PregnancyLength";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
