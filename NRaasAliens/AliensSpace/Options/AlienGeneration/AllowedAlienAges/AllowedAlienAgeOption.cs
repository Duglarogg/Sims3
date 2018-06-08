using System;
using System.Collections.Generic;
using System.Text;
using NRaas.CommonSpace.Options;
using Sims3.SimIFace.CAS;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienAges
{
    public abstract class AllowedAlienAgeOption : AgeToggleOption
    {
        protected override List<CASAgeGenderFlags> Setting => Aliens.Settings.mValidAlienAges;

        public override ITitlePrefixOption ParentListingOption => new ListingOption();
    }
}
