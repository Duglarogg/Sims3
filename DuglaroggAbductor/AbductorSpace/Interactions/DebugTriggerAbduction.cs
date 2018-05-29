using NRaas.AbductorSpace.Helpers;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Interactions
{
    public class DebugTriggerAbduction : ImmediateInteraction<Sim, Lot>
    {
        public static readonly InteractionDefinition Singleton;

        public override bool Run()
        {
            if (Target == null) return false;

            List<SimDescription> aliens = AlienUtilsEx.GetValidAliens();
            SimDescription alien = RandomUtil.GetRandomObjectFromList<SimDescription>(aliens);

            List<Sim> abductees = AlienUtilsEx.GetValidAbductees(Target);
            Sim abductee = RandomUtil.GetRandomObjectFromList<Sim>(abductees);

            AlienAbductionSituationEx.Create(alien, abductee, Target);

            return true;
        }

        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Lot, DebugTriggerAbduction>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return "DEBUG - Trigger Alien Abduction";
            }

            public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!Abductor.Settings.Debugging) return false;

                if (Household.AlienHousehold == null)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Alien household does not exist.");
                    return false;
                }

                if (AlienUtilsEx.GetValidAliens() == null)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("No valid abductors.");
                    return false;
                }

                if (AlienUtils.IsHouseboatAndNotDocked(target))
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Houseboat is not docked.");
                    return false;
                }

                if (!AlienUtilsEx.CanASimBeAbducted(target))
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("No valid abductees.");
                    return false;
                }

                return true;
            }
        }
    }
}
