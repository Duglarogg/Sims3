using NRaas.AbductorSpace.Buffs;
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

namespace NRaas.AbductorSpace.Helpers
{
    public class AbductionBuffs
    {
        public static BuffNames sAbductedEx = unchecked((BuffNames)ResourceUtils.HashString64("DuglaroggAbductionEx"));
        public static BuffNames sXenogenesis = unchecked((BuffNames)ResourceUtils.HashString64("DuglaroggXenogenesis"));
        public static BuffNames sAlienBabyIsComing = unchecked((BuffNames)ResourceUtils.HashString64("DuglaroggAlienBabyIsComing"));

        public static TraitNames sAlienChild = unchecked((TraitNames)ResourceUtils.HashString64("DuglaroggAlienChild"));

        public static void ApplyAbductedEx(Sim abductee, SimDescription abductor, bool autonomous)
        {
            abductee.BuffManager.AddElement(sAbductedEx, Origin.FromAbduction);
            BuffAbductedEx.BuffInstanceAbductedEx instance = abductee.BuffManager.GetElement(sAbductedEx) as BuffAbductedEx.BuffInstanceAbductedEx;
            instance.Abductee = abductee;
            instance.Abductor = abductor;
            instance.IsAutonomous = autonomous;
            instance.Impregnate();
        }

        public static void ApplyXenogenesis(Sim abductee, SimDescription abductor)
        {
            abductee.BuffManager.AddElement(sXenogenesis, Origin.FromPregnancy);
            BuffXenogenesis.BuffInstanceXenogenesis instance = abductee.BuffManager.GetElement(sXenogenesis) as BuffXenogenesis.BuffInstanceXenogenesis;
            instance.Abductee = abductee;
            instance.Abductor = abductor;
            instance.Pregnancy = abductee.SimDescription.Pregnancy;
            instance.StartPregnancy();
        }
    }
}
