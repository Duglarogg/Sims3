using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Tutorial;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class ShowAlienPregnancy : Interaction<Sim, Sim>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            if (Actor.IsSelectable)
            {
                PlumbBob.SelectActor(Actor);
                Camera.FocusOnSim(Actor, Pregnancy.kShowPregnancyCameraLerp.Zoom, Pregnancy.kShowPregnancyCameraLerp.Pitch,
                    Pregnancy.kShowPregnancyCameraLerp.Time, true, false);
                Audio.StartSound("sting_alien_visit");
            }

            Actor.SimDescription.ShowPregnancy();
            Actor.BuffManager.AddElement(AlienUtilsEx.sXenogenesis, Origin.FromPregnancy);
            ActiveTopic.AddToSim(Actor, "Announce Pregnancy");

            /* TODO------------------------------------------------ */
            /* Set alarm to give leave at start of third trimester! */
            /* ---------------------------------------------------- */

            Tutorialette.TriggerLesson(Lessons.Pregnancy, Actor);
            Actor.PlaySoloAnimation("a_alien_pregnancy_inspectStomach");

            /* TODO------------------------------ */
            /* Will not remove jobs at this time! */
            /* ---------------------------------- */

            return true;
        }

        [DoesntRequireTuning]
        public class Definition : InteractionDefinition<Sim, Sim, ShowAlienPregnancy>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return "Never Seen!";
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
        }
    }
}
