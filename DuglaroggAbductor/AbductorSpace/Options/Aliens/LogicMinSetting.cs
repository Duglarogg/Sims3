using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.Aliens
{
    public class LogicMinSetting : IntegerSettingOption<GameObject>, IAliensOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mLogicSkill[0];
            }

            set
            {
                if (value < 0) value = 0;
                value = Math.Min(value, Abductor.Settings.mLogicSkill[1]);
                Abductor.Settings.mLogicSkill[0] = value;
            }
        }

        public override string GetTitlePrefix()
        {
            return "LogicMin";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
