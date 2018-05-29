using Sims3.Gameplay.ActorSystems;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Helpers
{
    public class BuffsAndTraits
    {
        public static BuffNames sAbductedEx = unchecked((BuffNames)ResourceUtils.HashString64("DuglaroggAbductionEx"));
        public static BuffNames sXenogenesis = unchecked((BuffNames)ResourceUtils.HashString64("DuglaroggXenogenesis"));
        public static BuffNames sBabyIsComing = unchecked((BuffNames)ResourceUtils.HashString64("DuglaroggBabyIsComing"));

        public static TraitNames sAlienChild = unchecked((TraitNames)ResourceUtils.HashString64("DuglaroggAlienChild"));
    }
}
