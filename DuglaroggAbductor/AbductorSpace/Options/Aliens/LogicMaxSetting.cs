using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.Aliens
{
    public class LogicMaxSetting : IntegerSettingOption<GameObject>, IAliensOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mLogicSkill[1];
            }

            set
            {
                value = Math.Max(Abductor.Settings.mLogicSkill[0], value);
                if (value > 10) value = 10;
                Abductor.Settings.mLogicSkill[1] = value;
            }
        }

        public override string GetTitlePrefix()
        {
            return "LogicMax";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
