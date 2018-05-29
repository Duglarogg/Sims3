using Duglarogg.CommonSpace.Helpers;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.CommonSpace.Booters
{
    public class BuffBooter
    {
        private static string sFileName;

        public BuffBooter() : this(VersionStamp.sNamespace + ".Buffs")
        { }

        private BuffBooter(string reference)
        {
            sFileName = reference;
        }

        public void LoadBuffData()
        {
            AddBuffs(null);
            UIManager.NewHotInstallStoreBuffData += 
                new UIManager.NewHotInstallStoreBuffCallback(AddBuffs);
        }

        public void AddBuffs(ResourceKey[] keys)
        {
            try
            {
                ResourceKey key = new ResourceKey(ResourceUtils.HashString64(sFileName), 0x0333406C, 0u);
                XmlDbData data = XmlDbData.ReadData(key, false);

                if (data != null)
                {
                    BuffManager.ParseBuffData(data, true);
                }
            }
            catch (Exception e)
            {
                Logger.WriteExceptionLog(e, this, "BuffBooter.AddBuffs Error");
            }
        }
    }
}
