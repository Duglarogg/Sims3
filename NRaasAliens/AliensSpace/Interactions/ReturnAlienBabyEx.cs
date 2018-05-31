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
    public class ReturnAlienBabyEx : SocialInteraction, Common.IAddInteraction, Common.IPreLoad
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        VisualEffect mEffect;

        public class Definition : InteractionDefinition<Sim, Sim, ReturnAlienBabyEx>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Common.Localize("ReturnAlienBabyEx:MenuName");
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!target.TraitManager.HasElement(BuffsAndTraits.sAlienChild))
                {
                    greyedOutTooltipCallback = Aliens.Settings.Debugging ? CreateTooltipCallback("Target is not alien child.") : null;
                    return false;
                }

                if (target.SimDescription.ToddlerOrAbove)
                {
                    greyedOutTooltipCallback = Aliens.Settings.Debugging ? CreateTooltipCallback("Target is too old.") : null;
                    return false;
                }

                if (target.Genealogy.IsParentOrStepParent(actor.Genealogy))
                {
                    greyedOutTooltipCallback = Aliens.Settings.Debugging ? CreateTooltipCallback("Your are not a parent of this child.") : null;
                    return false;
                }

                return true;
            }
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<Sim>(Singleton);
        }

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
            Tunings.Inject<Sim, AlienUtils.ReturnAlienBaby.Definition, Definition>(true);
        }

        public override bool Run()
        {
            bool flag = TwoButtonDialog.Show(Common.Localize("ReturnAlienBabyEx:Dialogue", Actor.IsFemale, new object[] { Actor }),
                Common.Localize("ReturnAlienBabyEx:DialogueAccept"), Common.Localize("ReturnAlienBabyEx:DialogueReject"));

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

            return base.Run();
        }
    }
}
