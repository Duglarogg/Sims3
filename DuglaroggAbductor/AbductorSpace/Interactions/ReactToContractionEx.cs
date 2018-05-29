using NRaas.AbductorSpace.Helpers;
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
    public class ReactToContractionEx : Interaction<Sim, Sim>, Common.IPreLoad
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public void OnPreLoad()
        {
            Tunings.Inject<Sim, Pregnancy.ReactToContraction.Definition, Definition>(true);
        }

        public override bool Run()
        {
            SetPriority(new InteractionPriority(InteractionPriorityLevel.Zero));

            if (Actor.RouteToDynamicObjectRadius(Target, 1.5f, 3.5f, null, null))
            {
                if (!Target.SimDescription.IsPregnant)
                {
                    return false;
                }

                BeginCommodityUpdates();
                EnterStateMachine("ReactToFire", "Enter", "x");
                SetParameter("ReactToContraction", "yes");
                AnimateSim("Panic");
                bool flag = DoTimedLoop(Pregnancy.kSimMinutesReactToContraction);
                AnimateSim("Exit");
                EndCommodityUpdates(true);

                /* WISHLIST------------------------------------------------------------------------- */
                // Check if actor is spouse, parent, child, or friend of target
                // If actor meets one of the above requirements, then actor takes target to hospital
                /* --------------------------------------------------------------------------------- */
            }

            return true;
        }

        public class Definition : InteractionDefinition<Sim, Sim, ReactToContractionEx>
        {
            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return target.SimDescription.IsPregnant;
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Common.Localize("ReactToContractionEx:MenuName");
            }
        }
    }
}
