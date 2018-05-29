using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Booters
{
    internal class BuffBooter
    {
        public void LoadBuffData()
        {
            AddBuffs(null);
            UIManager.NewHotInstallStoreBuffData += new UIManager.NewHotInstallStoreBuffCallback(AddBuffs);
        }

        public void AddBuffs(ResourceKey[] keys)
        {
            try
            {
                ResourceKey key = new ResourceKey(ResourceUtils.HashString64("Duglarogg.Abductor.Buffs"), 0x0333406C, 0u);
                XmlDbData data = XmlDbData.ReadData(key, false);

                if (data != null)
                {
                    BuffManager.ParseBuffData(data, true);
                }
            }
            catch (Exception e)
            {
                Logger.WriteExceptionLog(e, this, "Duglarogg.AbductorSpace.Booters.BuffBooter.AddBuffs Error");
            }
        }
    }
}
