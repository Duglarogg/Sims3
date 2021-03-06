﻿using NRaas.AbductorSpace.Helpers;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.CelebritySystem;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Pools;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Interactions
{
    public class TakeToHospitalEx : ImmediateInteraction<Sim, Sim>, Common.IPreLoad
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public void OnPreLoad()
        {
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
                return RabbitHole.GetRabbitHolesOfType(RabbitHoleType.Hospital).Count > 0 && target.BuffManager.HasElement(AbductionBuffs.sAlienBabyIsComing);
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                if (actor == target)
                {
                    return Common.Localize("GoToHospitalEx:MenuName");
                }

                return Common.Localize("TakeToHospitalEx:MenuName");
            }
        }
    }
}
