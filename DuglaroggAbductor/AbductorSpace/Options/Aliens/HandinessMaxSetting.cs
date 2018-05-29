using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.Aliens
{
    public class HandinessMaxSetting : IntegerSettingOption<GameObject>, IAliensOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mHandinessSkill[1];
            }

            set
            {
                value = Math.Max(Abductor.Settings.mHandinessSkill[0], value);
                if (value > 10) value = 10;
                Abductor.Settings.mHandinessSkill[1] = value;
            }
        }

        public override string GetTitlePrefix()
        {
            return "HandinessMax";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
