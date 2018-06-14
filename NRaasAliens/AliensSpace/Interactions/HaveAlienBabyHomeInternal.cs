using NRaas.AliensSpace.Helpers;
using NRaas.AliensSpace.Proxies;
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

namespace NRaas.AliensSpace.Interactions
{
    public class HaveAlienBabyHomeInternal : Interaction<Sim, Sim>
    {
        public static readonly InteractionDefinition Singleton = new Definition();
        public int TotalCount;
        public int BabyIndex;
        public static ulong kIconNameHash = ResourceUtils.HashString64("hud_interactions_baby");

        [DoesntRequireTuning]
        public class Definition : InteractionDefinition<Sim, Sim, HaveAlienBabyHomeInternal>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Gameplay/Core/Lot/HaveBabyHome:InteractionName", new object[0]);
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
        }

        public override void Cleanup()
        {
            if (Actor.Posture != null && Actor.Posture.Container != null)
            {
                Target.UnParent();
                Target.AttemptToPutInSafeLocation(true);
            }

            if (BabyIndex == TotalCount && !Actor.HasBeenDestroyed)
                Actor.SimDescription.SetPregnancy(0f);

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
            Actor.SimDescription.SetPregnancy(1f - (float)BabyIndex / (float)TotalCount);
            AnimateSim("Exit");
            Pregnancy.MakeBabyVisible(Target);
            ChildUtils.CarryChild(Actor, Target, true);

            return true;
        }
    }
}
