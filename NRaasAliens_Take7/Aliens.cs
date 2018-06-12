using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas
{
    public class Aliens : Common
    {
        [Tunable, TunableComment("Scripting Mod Instantiator")]
        protected static bool kInstantiator = true;

        static Aliens()
        {

        }

        public Aliens()
        { }

        public static void ResetSettings()
        {
            // sSettings = null;
        }
    }
}
