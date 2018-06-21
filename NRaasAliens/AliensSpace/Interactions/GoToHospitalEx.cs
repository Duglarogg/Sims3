using NRaas.AliensSpace.Helpers;
using NRaas.AliensSpace.Proxies;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Interactions
{
    public class GoToHospitalEx : RabbitHole.RabbitHoleInteraction<Sim, RabbitHole>, Common.IPreLoad
    {
        public static readonly InteractionDefinition Singleton = new Definition();
        public HaveAlienBabyHospital haveBabyInstance;

        public class Definition : InteractionDefinition<Sim, RabbitHole, GoToHospitalEx>
        {
            public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop)
            {
                return Common.Localize("GoToHospitalEx:MenuName");
            }

            public override bool Test(Sim actor, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
        }

        public override bool InRabbitHole()
        {
            if (haveBabyInstance != null)
            {
                haveBabyInstance.AddFollower(Actor);

                while (!Actor.WaitForExitReason(Sim.kWaitForExitReasonDefaultTime, ExitReason.Canceled) && !haveBabyInstance.BabyBorn) { }

                return true;
            }

            return false;
        }

        public void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.GetTuning<RabbitHole, Pregnancy.GoToHospital.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Children = true;
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }

            Tunings.Inject<RabbitHole, Pregnancy.GoToHospital.Definition, Definition>(true);
        }

        public override bool Run()
        {
            RequestWalkStyle(Sim.WalkStyle.Run);
            return base.Run();
        }

        public override bool Test()
        {
            return !haveBabyInstance.BabyShouldBeBorn && base.Test();
        }
    }
}
