using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.Aliens
{
    public class AdvancedTechMinSetting : IntegerSettingOption<GameObject>, IAliensOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mAdvancedTechSkill[1];
            }

            set
            {
                value = Math.Max(Abductor.Settings.mAdvancedTechSkill[0], value);
                if (value > 10) value = 10;
                Abductor.Settings.mAdvancedTechSkill[1] = value;
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Abductor.Settings.mFutureSim;
        }

        public override string GetTitlePrefix()
        {
            return "AdvancedTechMin";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
