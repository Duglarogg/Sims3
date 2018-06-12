using System;
using System.Collections.Generic;
using System.Text;
using Sims3.SimIFace.CAS;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienAges
{
    public class AllowAdultSetting : AllowedAlienAgeOption, IAllowedAlienAgesOption
    {
        protected override CASAgeGenderFlags Type => CASAgeGenderFlags.Adult;
    }
}
