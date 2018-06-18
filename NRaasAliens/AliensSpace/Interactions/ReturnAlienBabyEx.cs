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
    public class ReturnAlienBabyEx : SocialInteraction, Common.IPreLoad
    {
        public class Definition : InteractionDefinition<Sim, Sim, ReturnAlienBabyEx>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Common.Localize("ReturnAlienBabyEx:MenuName");
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.SimDescription.ToddlerOrAbove)
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Target is too old.");
                    return false;
                }

                if (!target.Genealogy.IsParentOrStepParent(actor.Genealogy))
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Actor is not parent of target.");
                    return false;
                }

                if (actor.Household != target.Household)
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Actor and target not in same household.");
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

        public static readonly InteractionDefinition Singleton = new Definition();
        public VisualEffect mEffect;

        public override void Cleanup()
        {
            if (mEffect != null)
            {
                mEffect.Stop();
                mEffect.Dispose();
                mEffect = null;
            }

            base.Cleanup();
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

            Tunings.Inject<Sim, AlienUtils.ReturnAlienBaby.Definition, Definition>(true);
        }

        public override bool Run()
        {
            bool flag = TwoButtonDialog.Show(
                Common.Localize("ReturnAlienBabyEx:Dialogue", Actor.IsFemale, new object[] { Actor } ),
                Common.Localize("ReturnAlienBabyEx:DialogueAccept"), 
                Common.Localize("ReturnAlienBabyEx:DialogueReject"));

            if (flag)
            {
                Target.DisableInteractions();
                Target.InteractionQueue.CancelAllInteractions();
                mEffect = VisualEffect.Create("ep8BabyTeleportFx");
                mEffect.SetPosAndOrient(Target.Position, Target.ForwardVector, Target.UpVector);
                mEffect.Start();
                Target.FadeOut(true);
                List<IGenealogy> list = new List<IGenealogy>(Target.SimDescription.Genealogy.IParents);
                list.AddRange(Target.SimDescription.Genealogy.ISiblings);

                foreach (IGenealogy current in list)
                    Target.SimDescription.Genealogy.RemoveDirectRelation(current);

                Actor.Household.Remove(Target.SimDescription);
                Target.SimDescription.Dispose();
                Simulator.Sleep(30u);
            }

            return flag;
        }

        public static bool Run(AlienUtils.ReturnAlienBaby returnBaby)
        {
            Common.DebugNotify("ReturnAlienBabyEx" + Common.NewLine 
                + " - Actor:" + returnBaby.Actor.FullName + Common.NewLine 
                + " - Target: " + returnBaby.Target.FullName);

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
