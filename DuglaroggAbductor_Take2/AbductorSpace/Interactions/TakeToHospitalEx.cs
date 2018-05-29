using Duglarogg.AbductorSpace.Helpers;
using NRaas.CommonSpace.Helpers;
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
        public static readonly InteractionDefinition Singleton = new Definition();

        public static void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.GetTuning<RabbitHole, Pregnancy.GoToHospital.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }

            Tunings.Inject<Sim, Pregnancy.TakeToHospital.Definition, Definition>(true);
        }

        public override bool Run()
        {
            RabbitHole closestHospital = RabbitHole.GetClosestRabbitHoleOfType(RabbitHoleType.Hospital, Target.Position);
            HaveAlienBabyHospital haveBabyHospital = HaveAlienBabyHospital.Singleton.CreateInstance(closestHospital, Target,
                new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false) as HaveAlienBabyHospital;

            if (Actor != Target)
            {
                haveBabyHospital.AddFollower(Actor);
            }

            Target.InteractionQueue.Add(haveBabyHospital);
            return true;
        }

        public class Definition : ImmediateInteractionDefinition<Sim, Sim, TakeToHospitalEx>, IUsableDuringBirthSequence, IUsableDuringFire
        {
            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (RabbitHole.GetRabbitHolesOfType(RabbitHoleType.Hospital).Count == 0)
                {
                    return false;
                }

                if (!target.BuffManager.HasElement(BuffsAndTraits.sBabyIsComing))
                {
                    return false;
                }

                return true;
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                if (actor == target)
                {
                    return Localization.LocalizeString("Duglarogg/Abductor/Interactions/GoToHospitalEx:MenuName");
                }

                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/TakeToHospitalEx:MenuName");
            }
        }
    }
}
