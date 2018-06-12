using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy.AllowedBabyOccults
{
    public class ListingOption : OptionList<IAllowedBabyOccultsOption>, IAlienPregnancyOption
    {
        public override ITitlePrefixOption ParentListingOption => null;

        public override string GetTitlePrefix()
        {
            return "AllowedBabyOccults";
        }
    }
}
