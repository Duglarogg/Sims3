using NRaas.CommonSpace.Options;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy.AllowedBabyOccults
{
    public abstract class AllowedBabyOccultsOption : OccultToggleOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override List<OccultTypes> Setting => Aliens.Settings.mValidBabyOccults;
    }
}
