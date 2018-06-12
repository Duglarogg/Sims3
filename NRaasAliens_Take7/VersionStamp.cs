using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas
{
    class VersionStamp : Common.ProtoVersionStamp, Common.IPreLoad
    {
        public static readonly string sNameSpace = "NRaas.Aliens";
        public static readonly int sVersion = 7;

        public class Version : ProtoVersion<GameObject> { }

        public class TotalReset : ProtoResetSettings<GameObject> { }

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
