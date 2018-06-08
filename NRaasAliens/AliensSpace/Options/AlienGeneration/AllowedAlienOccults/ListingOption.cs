using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienOccults
{
    public class ListingOption : OptionList<IAllowedAlienOccultsOption>, IPrimaryOption<GameObject>, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => null;

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mAllowOccultAliens;
        }

        public override string GetTitlePrefix()
        {
            return "AllowedAlienOccults";
        }
    }
}
