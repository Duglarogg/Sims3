using NRaas.AliensSpace.Helpers;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.CAS;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Interactions
{
    public class ReturnAlienBabyEx : AlienUtils.ReturnAlienBaby, Common.IAddInteraction, Common.IPreLoad
    {
        static InteractionDefinition sOldSingleton;

        public new class Definition : AlienUtils.ReturnAlienBaby.Definition
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return base.GetInteractionName(actor, target, new InteractionObjectPair(sOldSingleton, target));
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!target.TraitManager.HasElement(BuffsAndTraits.sAlienChild))
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Target is not the product of an alien pregnancy.");
                    return false;
                }

                if (target.SimDescription.ToddlerOrAbove)
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Target is too old to return.");
                    return false;
                }

                if (target.Genealogy.IsParentOrStepParent(actor.Genealogy))
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Actor is not a parent of the target.");
                    return false;
                }

                if (actor.Household != target.Household)
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Actor and target are not in the same household.");
                    return false;
                }

                if (target.SimDescription.AgingState != null && target.SimDescription.AgingState.IsAgingInProgress())
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Target is aging up.");
                    return false;
                }

                return true;
            }
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Replace<Sim, AlienUtils.ReturnAlienBaby.Definition>(Singleton);
        }

        public void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.GetTuning<Sim, AlienUtils.ReturnAlienBaby.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }

            Tunings.Inject<Sim, AlienUtils.ReturnAlienBaby.Definition, Definition>(false);

            sOldSingleton = Singleton;
            Singleton = new Definition();
        }

        public override bool Run()
        {
            return Run(this);
        }

        public static bool Run(AlienUtils.ReturnAlienBaby returnBaby)
        {
            bool flag = TwoButtonDialog.Show(
                Common.Localize("ReturnAlienBabyEx:Dialogue"),
                Common.Localize("ReturnAlienBabyEx:DialogueAccept"),
                Common.Localize("ReturnAlienBabyEx:DialogueReject"));

            if (flag)
            {
                AgingManager.Singleton.CancelAgingAlarmsForSim(returnBaby.Target.SimDescription.AgingState);
                returnBaby.Target.DisableInteractions();
                returnBaby.Target.InteractionQueue.CancelAllInteractions();
                returnBaby.mEffect = VisualEffect.Create("ep8BabyTeleportFx");
                returnBaby.mEffect.SetPosAndOrient(returnBaby.Target.Position, returnBaby.Target.ForwardVector, returnBaby.Target.UpVector);
                returnBaby.mEffect.Start();
                returnBaby.Target.FadeOut(true);
                List<IGenealogy> list = new List<IGenealogy>(returnBaby.Target.Genealogy.IParents);
                list.AddRange(returnBaby.Target.Genealogy.ISiblings);

                foreach (IGenealogy current in list)
                    returnBaby.Target.Genealogy.RemoveDirectRelation(current);

                returnBaby.Actor.Household.Remove(returnBaby.Target.SimDescription);
                returnBaby.Target.SimDescription.Dispose();
                Simulator.Sleep(30u);
            }

            return true;
        }
    }
}
