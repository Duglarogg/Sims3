using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class GetMedicalAdviceEx : Hospital.GetMedicalAdvice
    {
        public static InteractionDefinition sOldSingleton;

        public static void OnPreLoad()
        {
            sOldSingleton = Singleton;
            Singleton = new Definition();
        }

        public static void ReplaceInteraction(RabbitHole hospital)
        {
            if (hospital.Guid != RabbitHoleType.Hospital)
            {
                return;
            }

            hospital.RemoveInteractionByType(sOldSingleton);

            foreach (InteractionObjectPair pair in hospital.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == Singleton.GetType())
                {
                    return;
                }
            }

            hospital.AddInteraction(Singleton);
        }

        public new class Definition : Hospital.GetMedicalAdvice.Definition
        {
            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                InteractionInstance instance = new GetMedicalAdviceEx();
                instance.Init(ref parameters);
                return instance;
            }

            public override string GetInteractionName(Sim actor, Hospital target, InteractionObjectPair iop)
            {
                return base.GetInteractionName(actor, target, new InteractionObjectPair(sOldSingleton, target));
            }

            public override bool Test(Sim a, Hospital target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return (a.BuffManager.HasElement(BuffNames.Pregnant) || a.BuffManager.HasElement(AlienUtilsEx.sXenogenesis)) && a.FamilyFunds > kCostOfMedicalAdvice;
            }
        }
    }
}
