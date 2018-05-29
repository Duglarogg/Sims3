using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.Aliens
{
    public class HandinessMinSetting : IntegerSettingOption<GameObject>, IAliensOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mHandinessSkill[0];
            }

            set
            {
                if (value < 0) value = 0;
                value = Math.Min(value, Abductor.Settings.mHandinessSkill[1]);
                Abductor.Settings.mHandinessSkill[0] = value;
            }
        }

        public override string GetTitlePrefix()
        {
            return "HandinessMin";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
