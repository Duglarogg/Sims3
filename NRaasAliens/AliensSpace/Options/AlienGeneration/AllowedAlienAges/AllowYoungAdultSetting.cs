using NRaas.CommonSpace.Options;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienAges
{
    public class AllowYoungAdultSetting : AllowedAlienAgeOption, IAllowedAlienAgesOption
    {
        protected override CASAgeGenderFlags Type => CASAgeGenderFlags.YoungAdult;
    }
}
