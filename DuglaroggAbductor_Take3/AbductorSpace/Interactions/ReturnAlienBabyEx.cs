using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
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
    public class ReturnAlienBabyEx : AlienUtils.ReturnAlienBaby
    {
        static InteractionDefinition sOldSingleton;

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

        public static void OnPreLoad()
        {
            sOldSingleton = Singleton;
            Singleton = new Definition();
        }

        public override bool Run()
        {
            return Run(this);
        }

        public static bool Run(AlienUtils.ReturnAlienBaby ths)
        {
            bool flag = TwoButtonDialog.Show(
                Localization.LocalizeString("Duglarogg/Abductor/Interactions/ReturnAlienBabyEx:Dialogue"),
                Localization.LocalizeString("Duglarogg/Abductor/Interactions/ReturnAlienBabyEx:DialogueAccept"),
                Localization.LocalizeString("Duglarogg/Abductor/Interactions/ReturnAlineBabyEx:DialogueReject"));

            if (flag)
            {
                SimDescription description = ths.Target.SimDescription;
                ths.Target.DisableInteractions();
                ths.Target.InteractionQueue.CancelAllInteractions();
                ths.mEffect = VisualEffect.Create("ep8BabyTeleportFx");
                ths.mEffect.SetPosAndOrient(ths.Target.Position, ths.Target.ForwardVector, ths.Target.UpVector);
                ths.mEffect.Start();
                ths.Target.FadeOut(true);
                List<IGenealogy> list = new List<IGenealogy>(description.Genealogy.IParents);
                list.AddRange(description.Genealogy.ISiblings);

                foreach(IGenealogy current in list)
                {
                    description.Genealogy.RemoveDirectRelation(current);
                }

                ths.Actor.Household.Remove(description);
                description.Dispose();
                Simulator.Sleep(30u);
            }

            return true;
        }

        public new class Definition : AlienUtils.ReturnAlienBaby.Definition
        {
            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                InteractionInstance instance = new ReturnAlienBabyEx();
                instance.Init(ref parameters);
                return instance;
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/RetunAlienBabyEx:MenName");
            }

            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!target.TraitManager.HasElement(AlienUtilsEx.sAlienChild))
                {
                    greyedOutTooltipCallback = Abductor.Settings.mDebugging ? CreateTooltipCallback("Target is not an Alien Child") : null;
                    return false;
                }

                if (!target.SimDescription.Baby)
                {
                    greyedOutTooltipCallback = Abductor.Settings.mDebugging ? CreateTooltipCallback("Target is not a Baby") : null;
                    return false;
                }

                if (target.SimDescription.IsDueToAgeUp() || (target.SimDescription.AgingState != null && target.SimDescription.AgingState.IsAgingInProgress()))
                {
                    greyedOutTooltipCallback = Abductor.Settings.mDebugging ? CreateTooltipCallback("Target is Aging Up") : null;
                    return false;
                }

                if (!target.Genealogy.IsParentOrStepParent(a.Genealogy))
                {
                    greyedOutTooltipCallback = Abductor.Settings.mDebugging ? CreateTooltipCallback("Actor is not Parent") : null;
                    return false;
                }

                if (target.Household != a.Household)
                {
                    greyedOutTooltipCallback = Abductor.Settings.mDebugging ? CreateTooltipCallback("Target is not in Actor's Household") : null;
                    return false;
                }

                return true;
            }
        }
    }
}
