using NRaas.AliensSpace.Helpers;
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
    public class DebugAbductSim : ImmediateInteraction<Sim, Sim>, Common.IAddInteraction
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Sim, DebugAbductSim>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return "DEBUG - Abduct Sim";
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!Aliens.Settings.Debugging)
                    return false;

                if (Household.AlienHousehold == null)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Alien household is null.");
                    return false;
                }

                if (AlienUtils.IsHouseboatAndNotDocked(target.LotCurrent))
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Target is on an undocked houseboat.");
                    return false;
                }

                if (AlienUtilsEx.GetValidAliens() == null)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("No valid aliens.");
                    return false;
                }

                if (!target.SimDescription.IsHuman)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Target is not human.");
                    return false;
                }

                if (target.SimDescription.ChildOrBelow)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Target is not teen or older.");
                    return false;
                }

                return true;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<Sim>(Singleton);
        }

        public override bool Run()
        {
            if (Target == null)
                return false;

            List<SimDescription> aliens = AlienUtilsEx.GetValidAliens();
            Lot lot = Target.LotCurrent;

            if (aliens == null)
            {
                Common.DebugNotify("DEBUG - Abduct Sim: No valid aliens");
                return false;
            }

            if (lot == null)
            {
                Common.DebugNotify("DEBUG - Abduct Sim: Target's current lot is null");
                return false;
            }

            SimDescription alien = RandomUtil.GetRandomObjectFromList(aliens);
            AlienAbductionSituationEx.Create(alien, Target, lot);

            return true;
        }
    }
}
