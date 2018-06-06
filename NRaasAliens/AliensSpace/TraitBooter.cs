using NRaas.CommonSpace.Booters;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Booters
{
    internal class TraitBooter
    {
        private readonly string sReference = "NRaas." + VersionStamp.sNamespace + ".Traits";

        public void LoadTraitData()
        {
            AddTraits(null);
            UIManager.NewHotInstallStoreTraitsData += new UIManager.NewHotInstallStoreTraitsCallback(AddTraits);
        }

        public void AddTraits(ResourceKey[] resourceKeys)
        {
            try
            {
                ResourceKey key = new ResourceKey(ResourceUtils.HashString64(sReference), 0x0333406C, 0u);
                XmlDbData xmlDbData = XmlDbData.ReadData(key, false);

                if (xmlDbData != null)
                    TraitManager.ParseTraitData(xmlDbData, true);
            }
            catch(Exception e)
            {
                Common.DebugException("TraitBooter", e);
            }
        }
    }
}
