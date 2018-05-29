using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Situations;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class DebugTriggerAlienVisit : ImmediateInteraction<Sim, Lot>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public static void AddInteraction(Lot lot)
        {
            foreach (InteractionObjectPair pair in lot.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == Singleton.GetType())
                {
                    return;
                }
            }

            lot.AddInteraction(Singleton);
        }

        public override bool Run()
        {
            List<SimDescription> aliens = AlienUtilsEx.GetAliens();

            if (aliens == null)
            {
                Logger.Append("Debug - Trigger Alien Visit: No Aliens");
                return false;
            }

            SimDescription alien = RandomUtil.GetRandomObjectFromList<SimDescription>(aliens);

            Lot farthestLot = LotManager.GetFarthestLot(Target);
            Sim visitor = alien.InstantiateOffScreen(farthestLot);

            AlienSituation.Create(visitor, Target);

            return true;
        }

        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Lot, DebugTriggerAlienVisit>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return "Debug - Trigger Alien Visit";
            }

            public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
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

                return true;
            }
        }
    }
}
