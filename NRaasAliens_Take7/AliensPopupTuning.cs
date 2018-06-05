using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas
{
    public class AliensPopupTuning
    {
        [Tunable, TunableComment("Whether or not to use a popup menu approach when displaying the interactions")]
        public static bool kPopupMenuStyle = false;
    }
}
