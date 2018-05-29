using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class DebugTriggerAlienAbduction : ImmediateInteraction<Sim, Lot>
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
                Logger.Append("Debug - Trigger Alien Abduction: No Aliens");
                return false;
            }

            List<Sim> abductees = AlienUtilsEx.GetAbductees(Target);

            if (abductees == null)
            {
                Logger.Append("Debug - Trigger Alien Abduction: No Abductees");
                return false;
            }

            Sim abductee = RandomUtil.GetRandomObjectFromList<Sim>(abductees);
            SimDescription alien = RandomUtil.GetRandomObjectFromList<SimDescription>(aliens);

            if (!AlienUtilsEx.CanSimBeAbducted(abductee))
            {
                Logger.Append("Debug - Trigger Alien Abduction: Can't Abduct Sim");
                return false;
            }

            AlienAbductionSituationEx.Create(alien, abductee, Target);

            return true;
        }

        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Lot, DebugTriggerAlienAbduction>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return "Debug - Trigger Alien Abduction";
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

                if (AlienUtils.IsHouseboatAndNotDocked(target))
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("Houseboat Not Docked");
                    return false;
                }

                if (target.GetSimsCount() == 0)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback("No Abductees on Lot");
                    return false;
                }

                return true;
            }
        }
    }
}
