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
    internal class TraitBooter
    {
        public void LoadTraitData()
        {
            AddTraits(null);
            UIManager.NewHotInstallStoreTraitsData += new UIManager.NewHotInstallStoreTraitsCallback(AddTraits);
        }

        public void AddTraits(ResourceKey[] keys)
        {
            try
            {
                ResourceKey key = new ResourceKey(ResourceUtils.HashString64("Duglarogg.Abductor.Traits"), 0x0333406C, 0u);
                XmlDbData data = XmlDbData.ReadData(key, false);

                if (data != null)
                {
                    TraitManager.ParseTraitData(data, true);
                }
            }
            catch (Exception e)
            {
                Logger.WriteExceptionLog(e, this, "Duglarogg.AbductorSpace.Booters.TraitBooter.AddTraits() Error");
            }
        }
    }
}
