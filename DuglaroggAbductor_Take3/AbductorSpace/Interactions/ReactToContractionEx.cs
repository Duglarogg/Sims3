using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class ReactToContractionEx : Interaction<Sim, Sim>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            SetPriority(new InteractionPriority(InteractionPriorityLevel.Zero));

            if (Actor.RouteToDynamicObjectRadius(Target, 1.5f, 3.5f, null, null))
            {
                if (!Target.SimDescription.IsPregnant)
                {
                    return false;
                }

                BeginCommodityUpdates();
                EnterStateMachine("ReactToFire", "Enter", "x");
                SetParameter("ReactToContraction", "yes");
                AnimateSim("Panic");
                bool flag = DoTimedLoop(Pregnancy.kSimMinutesReactToContraction);
                AnimateSim("Exit");
                EndCommodityUpdates(true);

                /* WISHLIST------------------------------------------------------------------------- */
                // Check if actor is spouse, parent, child, or friend of target
                // If actor meets one of the above requirements, then actor takes target to hospital
                /* --------------------------------------------------------------------------------- */
            }

            return true;
        }

        public class Definition : InteractionDefinition<Sim, Sim, ReactToContractionEx>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/ReactToContractionEx:MenuName");
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return target.SimDescription.IsPregnant;
            }
        }
    }
}
