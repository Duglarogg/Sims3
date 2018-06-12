using System;
using System.Collections.Generic;
using System.Text;
using NRaas.CommonSpace.Options;
using Sims3.SimIFace.CAS;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienAges
{
    public class AllowTeenSetting : AllowedAlienAgeOption, IAllowedAlienAgesOption
    {
        protected override CASAgeGenderFlags Type => CASAgeGenderFlags.Teen;
    }
}
