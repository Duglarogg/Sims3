using NRaas.AliensSpace.Helpers;
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

/* <WISHLIST>
 *  HaveRelationship()
 *      May allow strangers and aquaitances (with appropriate traits) to take a laboring Sim to the hospital.
 */

namespace NRaas.AliensSpace.Interactions
{
    public class ReactToContractionEx : Interaction<Sim, Sim>, Common.IPreLoad
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public class Definition : InteractionDefinition<Sim, Sim, ReactToContractionEx>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:ReactToContraction", new object[0]);
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return target.SimDescription.IsPregnant;
            }
        }

        public bool HaveRelationship()
        {
            Relationship relationship = Actor.GetRelationship(Target, false);

            if (relationship != null && relationship.AreEnemies())
                return false;

            // <WISHLIST> May checks for good Samaritan Sims </WISHLIST>
            if (relationship == null || !relationship.AreFriends())
                return false;
 
            if (relationship.MarriedInGame)
                return true;
            else if (Target.Genealogy.IsParentOrStepParent(Actor.Genealogy))
                return true;
            else if (Actor.IsBloodRelated(Target))
                return true;
            else if (relationship.AreRoommates())
                return true;

            return true;
        }

        public void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.GetTuning<Sim, Pregnancy.ReactToContraction.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Children = true;
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }

            Tunings.Inject<Sim, Pregnancy.ReactToContraction.Definition, Definition>(true);
        }

        public override bool Run()
        {
            SetPriority(new InteractionPriority(InteractionPriorityLevel.Zero));

            if (Actor.RouteToDynamicObjectRadius(Target, 1.5f, 3.5f, null, null))
            {
                if (!Target.SimDescription.IsPregnant)
                    return false;

                BeginCommodityUpdates();
                EnterStateMachine("ReactToFire", "Enter", "x");
                SetParameter("ReactToContraction", "yes");
                AnimateSim("Panic");
                bool flag = DoTimedLoop(Pregnancy.kSimMinutesReactToContraction);
                AnimateSim("Exit");
                EndCommodityUpdates(true);

                // See if actor takes target to hospital
                if (flag && Target.SimDescription.IsPregnant && HaveRelationship() && !Target.InteractionQueue.HasInteractionOfType(Pregnancy.GoToHospital.Singleton))
                {
                    InteractionInstance entry = TakeToHospitalEx.Singleton.CreateInstance(Target, Actor, GetPriority(), Autonomous, CancellableByPlayer);
                    Actor.InteractionQueue.AddNext(entry);
                }
            }

            return true;
        }
    }
}
