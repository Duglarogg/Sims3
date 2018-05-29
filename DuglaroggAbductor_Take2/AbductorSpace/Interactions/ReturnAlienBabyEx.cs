using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class ReturnAlienBabyEx : SocialInteraction
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        VisualEffect mEffect;

        public static void AddInteraction(Sim sim)
        {
            foreach (InteractionObjectPair pair in sim.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == Singleton.GetType())
                {
                    return;
                }
            }

            sim.AddInteraction(Singleton);
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

        public override bool Run()
        {
            bool flag = TwoButtonDialog.Show(
                Localization.LocalizeString("Duglarogg/Abdcutor/Interactions/ReturnAlienBabyEx:Dialogue"),
                Localization.LocalizeString("Duglarogg/Abdcutor/Interactions/ReturnAlienBabyEx:DialogueAccept"),
                Localization.LocalizeString("Duglarogg/Abdcutor/Interactions/ReturnAlienBabyEx:DialogueReject"));

            if (flag)
            {
                SimDescription description = Target.SimDescription;
                Target.DisableInteractions();
                Target.InteractionQueue.CancelAllInteractions();
                mEffect = VisualEffect.Create("ep8BabyTeleportFx");
                mEffect.SetPosAndOrient(Target.Position, Target.ForwardVector, Target.UpVector);
                mEffect.Start();
                Target.FadeOut(true);
                List<IGenealogy> list = new List<IGenealogy>(description.Genealogy.IParents);
                list.AddRange(description.Genealogy.ISiblings);

                foreach (IGenealogy current in list)
                {
                    description.Genealogy.RemoveDirectRelation(current);
                }

                Actor.Household.Remove(description);
                description.Dispose();
                Simulator.Sleep(30u);
            }

            return true;
        }

        public class Definition : InteractionDefinition<Sim, Sim, ReturnAlienBabyEx>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/ReturnAlienBabyEx:MenuName");
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                bool debug = Abductor.Settings.mDebugging;

                if (!target.TraitManager.HasElement(BuffsAndTraits.sAlienChild))
                {
                    greyedOutTooltipCallback = debug ? CreateTooltipCallback("Target is not product of alien pregnancy.") : null;
                    return false;
                }

                if (target.SimDescription.ToddlerOrAbove)
                {
                    greyedOutTooltipCallback = debug ? CreateTooltipCallback("Target is too old.") : null;
                    return false;
                }

                if (target.Genealogy.IsParentOrStepParent(actor.Genealogy))
                {
                    greyedOutTooltipCallback = debug ? CreateTooltipCallback("Actor is not parent of target.") : null;
                    return false;
                }

                return true;
            }
        }
    }
}
