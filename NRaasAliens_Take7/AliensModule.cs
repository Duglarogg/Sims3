using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas
{
    public class AliensModule
    {
        [Tunable, TunableComment("Scripting Mod Instantiator")]
        protected static bool kInstantiator = true;

        static AliensModule() { }

        public AliensModule() { }
    }
}
