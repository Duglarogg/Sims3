using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienActivity
{
    public class BaseActivityChanceSetting : IntegerSettingOption<GameObject>, IAlienActivityOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mBaseActivityChance;
            }

            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                Abductor.Settings.mBaseActivityChance = value;
            }
        }

        public override string GetTitlePrefix()
        {
            return "BaseActivityChance";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
