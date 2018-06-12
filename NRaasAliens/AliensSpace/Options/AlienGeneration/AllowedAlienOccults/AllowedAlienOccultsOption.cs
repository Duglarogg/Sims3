using System;
using System.Collections.Generic;
using System.Text;
using NRaas.CommonSpace.Options;
using Sims3.UI.Hud;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienOccults
{
    public abstract class AllowedAlienOccultsOption : OccultToggleOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override List<OccultTypes> Setting => Aliens.Settings.mValidAlienOccults;
    }
}
