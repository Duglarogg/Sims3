﻿using NRaas.AliensSpace.Helpers;
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

namespace NRaas.AliensSpace.Interactions
{
    public class DebugTriggerAbduction : ImmediateInteraction<Sim, Lot>, Common.IAddInteraction
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Lot, DebugTriggerAbduction>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return "DEBUG - Abduct from Lot";
            }

            public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!Aliens.Settings.Debugging)
                    return false;

                if (Household.AlienHousehold == null)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Alien household does not exist.");
                    return false;
                }

                if (AlienUtils.IsHouseboatAndNotDocked(target))
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Houseboat is not docked.");
                    return false;
                }

                if (AlienUtilsEx.GetValidAliens() == null)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("No valid aliens.");
                    return false;
                }

                if (AlienUtilsEx.GetValidAbductees(target) == null)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("No valid abductees.");
                    return false;
                }

                return true;
            }
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<Lot>(Singleton);
        }

        public override bool Run()
        {
            if (Target == null)
                return false;

            List<SimDescription> aliens = AlienUtilsEx.GetValidAliens();
            List<Sim> abductees = AlienUtilsEx.GetValidAbductees(Target);

            if (aliens == null)
            {
                Common.DebugNotify("DEBUG - Trigger Abduction: No valid aliens.");
                return false;
            }

            if (abductees == null)
            {
                Common.DebugNotify("DEBUG - Trigger Abduction: No valid abductees.");
                return false;
            }

            Sim abductee = RandomUtil.GetRandomObjectFromList(abductees);
            SimDescription alien = RandomUtil.GetRandomObjectFromList(aliens);
            AlienAbductionSituationEx.Create(alien, abductee, Target);

            return true;
        }

    }
}
