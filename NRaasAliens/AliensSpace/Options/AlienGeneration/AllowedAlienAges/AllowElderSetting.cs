using System;
using System.Collections.Generic;
using System.Text;
using Sims3.SimIFace.CAS;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienAges
{
    public class AllowElderSetting : AllowedAlienAgeOption, IAllowedAlienAgesOption
    {
        protected override CASAgeGenderFlags Type => CASAgeGenderFlags.Elder;
    }
}
