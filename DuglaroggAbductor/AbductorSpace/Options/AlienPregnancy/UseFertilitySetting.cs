using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienPregnancy
{
    public class UseFertilitySetting : BooleanSettingOption<GameObject>, IAlienPregnancyOption
    {
        protected override bool Value
        {
            get
            {
                return Abductor.Settings.mUseFertility;
            }

            set
            {
                Abductor.Settings.mUseFertility = value;
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return (Abductor.Settings.mImpregnationChance > 0);
        }

        public override string GetTitlePrefix()
        {
            return "UseFertility";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
