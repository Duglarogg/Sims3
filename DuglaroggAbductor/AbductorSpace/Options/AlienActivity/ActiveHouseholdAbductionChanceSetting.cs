using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienActivity
{
    public class ActiveHouseholdAbductionChanceSetting : IntegerSettingOption<GameObject>, IAlienActivityOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mActiveAbductionChance;
            }

            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                Abductor.Settings.mActiveAbductionChance = value;
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return (Abductor.Settings.mBaseActivityChance > 0) && (Abductor.Settings.mBaseAbductionChance > 0);
        }

        public override string GetTitlePrefix()
        {
            return "ActiveAbductionChance";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
