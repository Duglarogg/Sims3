using NRaas.AliensSpace.Buffs;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.StoryProgression;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.Enums;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Helpers
{
    public class BuffsAndTraits
    {
        public static readonly BuffNames sAbductedEx = unchecked((BuffNames)ResourceUtils.HashString64("NRaasAbductedEx"));
        public static readonly BuffNames sXenogenesis = unchecked((BuffNames)ResourceUtils.HashString64("NRaasXenogenesis"));
        public static readonly BuffNames sAlienBabyIsComing = unchecked((BuffNames)ResourceUtils.HashString64("NRaasAlienBabyIsComing"));
        public static readonly TraitNames sAlienChild = unchecked((TraitNames)ResourceUtils.HashString64("NRaasAlienChild"));
    }
}
