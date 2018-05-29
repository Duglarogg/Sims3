using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas
{
    public class AliensPopupTuning
    {
        [Tunable, TunableComment("Whether to use a popup menu approach when displaying the interactions")]
        public static bool kPopupMenuStyle = false;
    }

    class VersionStamp : Common.ProtoVersionStamp, Common.IPreLoad
    {
        public static readonly string sNameSpace = "NRaas.Aliens";
        public static readonly int sVersion = 1;

        public class Version : ProtoVersion<GameObject>
        { }

        public class TotalReset : ProtoResetSettings<GameObject>
        { }

        public void OnPreLoad()
        {
            sPopupMenuStyle = AliensPopupTuning.kPopupMenuStyle;
        }

        public static void ResetSettings()
        {
            Aliens.ResetSettings();
        }
    }
}
