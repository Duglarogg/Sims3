using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class TakeToHospitalEx : ImmediateInteraction<Sim, Sim>
    {
        public static InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            RabbitHole closestHospital = RabbitHole.GetClosestRabbitHoleOfType(RabbitHoleType.Hospital, Target.Position);
            HaveAlienBabyHospital haveBabyHospital = HaveAlienBabyHospital.Singleton.CreateInstance(
                closestHospital, Target, new InteractionPriority(InteractionPriorityLevel.Pregnancy), 
                false, false) as HaveAlienBabyHospital;

            if (Actor != Target)
            {
                haveBabyHospital.AddFollower(Actor);
            }

            Target.InteractionQueue.Add(haveBabyHospital);

            return true;
        }

        public class Definition : ImmediateInteractionDefinition<Sim, Sim, TakeToHospitalEx>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                if (actor == target)
                {
                    return Localization.LocalizeString("Duglarogg/Abductor/Interactions/GoToHospitalEx:MenuName");
                }

                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/TakeToHospitalEx:MenuName");
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return (RabbitHole.GetRabbitHolesOfType(RabbitHoleType.Hospital).Count > 0) && target.BuffManager.HasElement(AlienUtilsEx.sBabyIsComing);
            }
        }
    }
}
