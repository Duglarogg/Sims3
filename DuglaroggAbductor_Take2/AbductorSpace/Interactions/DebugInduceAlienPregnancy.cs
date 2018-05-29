using Duglarogg.AbductorSpace.Buffs;
using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class DebugInduceAlienPregnancy : ImmediateInteraction<Sim, Sim>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public static void AddInteraction(Sim sim)
        {
            foreach (InteractionObjectPair pair in sim.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == Singleton.GetType())
                {
                    return;
                }
            }

            sim.AddInteraction(Singleton);
        }

        public override bool Run()
        {
            Target.BuffManager.AddElement(BuffsAndTraits.sAbductedEx, Origin.FromAbduction);

            if (!Target.SimDescription.IsPregnant)
            {
                AbductedEx.BuffInstanceAbductedEx instance = Target.BuffManager.GetElement(BuffsAndTraits.sAbductedEx) as AbductedEx.BuffInstanceAbductedEx;

                Pregnancy pregnancy = CommonPregnancy.CreatePregnancy(instance.Abductee, instance.Alien, !CommonPregnancy.AllowPlantSimPregnancy());

                if (pregnancy != null)
                {
                    instance.IsAlienPregnancy = true;
                    Target.SimDescription.Pregnancy = pregnancy;
                    EventTracker.SendEvent(EventTypeId.kGotPregnant, Target);
                }
            }


            return true;
        }

        public class Definition : ImmediateInteractionDefinition<Sim, Sim, DebugInduceAlienPregnancy>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return "Debug - Induce Alien Pregnancy";
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!Abductor.Settings.mDebugging)
                {
                    return false;
                }

                if (Household.AlienHousehold == null || Household.AlienHousehold.NumMembers == 0)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Alien Household Null or Empty");
                    return false;
                }

                if (!target.IsHuman)
                {
                    return false;
                }

                if (target.SimDescription.ChildOrBelow)
                {
                    return false;
                }

                if (target.SimDescription.IsPregnant)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Already Pregnant");
                    return false;
                }

                return true;
            }
        }
    }
}
