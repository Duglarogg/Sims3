using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
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
        public static readonly InteractionDefinition Singleton;

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
            List<SimDescription> aliens = AlienUtilsEx.GetAliens(false);

            if (aliens == null)
            {
                Logger.Append("Debug - Induce Alien Pregnancy: No Aliens");
                return false;
            }

            SimDescription alien = RandomUtil.GetRandomObjectFromList<SimDescription>(aliens);
            Target.SimDescription.Pregnancy = new Pregnancy(Target, alien);
            Target.BuffManager.AddElement(BuffNames.Abducted, Origin.FromPregnancy);

            return true;
        }

        [DoesntRequireTuning]
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
                    greyedOutTooltipCallback = CreateTooltipCallback("Target isn't Human");
                    return false;
                }

                if (!target.SimDescription.TeenOrAbove)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Target isn't Old Enough");
                    return false;
                }

                if (target.SimDescription.IsPregnant)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Target is Already Pregnant");
                    return false;
                }

                return true;
            }
        }
    }
}
