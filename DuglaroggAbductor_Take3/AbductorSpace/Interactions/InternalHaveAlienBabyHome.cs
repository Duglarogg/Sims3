using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class InternalHaveAlienBabyHome : Interaction<Sim, Sim>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public int TotalCount;
        public int BabyIndex;
        public static ulong kIconNameHash = ResourceUtils.HashString64("hud_interactions_baby");

        public override void Cleanup()
        {
            if (Actor.Posture != null && Actor.Posture.Container != null)
            {
                Target.UnParent();
                Target.AttemptToPutInSafeLocation(true);
            }

            if (BabyIndex == TotalCount && !Actor.HasBeenDestroyed)
            {
                Actor.SimDescription.SetPregnancy(0f);
            }

            base.Cleanup();
        }

        public override ThumbnailKey GetIconKey()
        {
            return new ThumbnailKey(new ResourceKey(kIconNameHash, 796721156u, 0u), ThumbnailSize.Medium);
        }

        public override bool Run()
        {
            mCurrentStateMachine.SetActor("y", Target);
            mCurrentStateMachine.EnterState("x", "Enter");
            AnimateSim("HaveBaby");
            Actor.SimDescription.SetPregnancy(1f - BabyIndex / TotalCount);
            AnimateSim("Exit");
            Pregnancy.MakeBabyVisible(Target);
            ChildUtils.CarryChild(Actor, Target, true);

            return true;
        }

        [DoesntRequireTuning]
        public class Definition : InteractionDefinition<Sim, Sim, InternalHaveAlienBabyHome>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/HaveAlienBabyHome:MenuName");
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
        }
    }
}
