using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.Aliens
{
    public class AdvancedTechMaxSetting : IntegerSettingOption<GameObject>, IAliensOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mAdvancedTechSkill[0];
            }

            set
            {
                if (value < 0) value = 0;
                value = Math.Min(value, Abductor.Settings.mAdvancedTechSkill[1]);
                Abductor.Settings.mAdvancedTechSkill[0] = value;
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Abductor.Settings.mFutureSim;
        }

        public override string GetTitlePrefix()
        {
            return "AdvancedTechMax";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
