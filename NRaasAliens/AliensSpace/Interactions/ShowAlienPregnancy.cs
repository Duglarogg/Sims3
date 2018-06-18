using NRaas.AliensSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Pools;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.CAS;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Interactions
{
    public class ShowAlienPregnancy : Interaction<Sim, Sim>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        [DoesntRequireTuning]
        public class Definition : InteractionDefinition<Sim, Sim, ShowAlienPregnancy>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return "Never seen!";
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
        }

        public override bool Run()
        {
            Actor.SimDescription.Pregnancy.mHasShownPregnancy = true;

            if (Actor.IsSelectable)
            {
                PlumbBob.SelectActor(Actor);
                Camera.FocusOnSim(Actor, Pregnancy.kShowPregnancyCameraLerp.Zoom, Pregnancy.kShowPregnancyCameraLerp.Pitch,
                    Pregnancy.kShowPregnancyCameraLerp.Time, true, false);
                Audio.StartObjectSound(Actor.ObjectId, "sting_alien_visit", false);
            }

            Actor.SimDescription.ShowPregnancy();
            Actor.BuffManager.AddElement(BuffsAndTraits.sXenogenesis, Origin.FromPregnancy);

            if (Actor.BuffManager.HasElement(BuffNames.Nauseous))
                Actor.BuffManager.GetElement(BuffNames.Nauseous).mBuffOrigin = Origin.FromPregnancy;

            ActiveTopic.AddToSim(Actor, "Announce Pregnancy");
            Actor.SimDescription.Pregnancy.TryToGiveLeave();
            Tutorialette.TriggerLesson(Lessons.Pregnancy, Actor);
            Actor.PlaySoloAnimation("a_alien_pregnancy_inspectStomach", false);

            if (Actor.Occupation != null)
                Actor.Occupation.RemoveAllJobs();

            return true;
        }
    }
}
