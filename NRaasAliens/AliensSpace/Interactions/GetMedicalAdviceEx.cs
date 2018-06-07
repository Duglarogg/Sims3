using NRaas.AliensSpace.Helpers;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Interactions
{
    public class GetMedicalAdviceEx : Hospital.GetMedicalAdvice, Common.IAddInteraction, Common.IPreLoad
    {
        public static InteractionDefinition sOldSingleton;

        public new class Definition : Hospital.GetMedicalAdvice.Definition
        {
            public override string GetInteractionName(Sim actor, Hospital target, InteractionObjectPair iop)
            {
                return base.GetInteractionName(actor, target, new InteractionObjectPair(sOldSingleton, target));
            }

            public override bool Test(Sim a, Hospital target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return (a.BuffManager != null && a.BuffManager.HasAnyElement(new BuffNames[] { BuffNames.Pregnant, BuffsAndTraits.sXenogenesis }));
            }
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Replace<RabbitHole, Hospital.GetMedicalAdvice.Definition>(Singleton);
        }

        public void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.GetTuning<RabbitHole, Hospital.GetMedicalAdvice.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }

            Tunings.Inject<RabbitHole, Hospital.GetMedicalAdvice.Definition, Definition>(false);

            sOldSingleton = Singleton;
            Singleton = new Definition();
        }
    }
}
