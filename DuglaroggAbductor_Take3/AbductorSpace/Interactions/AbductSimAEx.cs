using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.ThoughtBalloons;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class AbductSimAEx : CarUFO.AbductSimA
    {
        public static InteractionDefinition sOldSingleton;

        public static void OnPreLoad()
        {
            sOldSingleton = Singleton;
            Singleton = new Definition();
        }

        public static void ReplaceInteraction(CarUFO ufo)
        {
            ufo.RemoveInteractionByType(sOldSingleton);

            foreach(InteractionObjectPair pair in ufo.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == Singleton.GetType())
                {
                    return;
                }
            }

            ufo.AddInteraction(Singleton);
        }

        public override bool Run()
        {
            return Run(this);
        }

        public static bool Run(CarUFO.AbductSimA ths)
        {
            if (ths.Target.InUse)
            {
                return false;
            }

            ths.mNPCAbductor = (ths.SimToAbduct != null);

            if (!ths.mNPCAbductor)
            {
                ths.SimToAbduct = ths.GetSelectedObject() as Sim;
            }

            if (ths.SimToAbduct == null)
            {
                return false;
            }

            ths.StandardEntry();

            if (!ths.SetupAbductee())
            {
                ths.StandardExit();
                return false;
            }

            Animation.ForceAnimation(ths.Actor.ObjectId, true);
            Animation.ForceAnimation(ths.Target.ObjectId, true);
            ths.Target.mTakeOffPos = ths.Actor.Position;

            if (!ths.Target.RouteToUFOAndTakeOff(ths.Actor))
            {
                ths.StandardExit();
                return false;
            }

            Camera.FocusOnGivenPosition(ths.mJig.Position, CarUFO.kAbductLerpParams, true);
            ths.BeginCommodityUpdates();
            bool flag = ths.AbductSim();
            ths.EndCommodityUpdates(true);
            Sim[] sims;

            if (flag)
            {
                EventTracker.SendEvent(EventTypeId.kAbductSimUFO, ths.Actor, ths.SimToAbduct);
                sims = new Sim[] { ths.Actor, ths.SimToAbduct };

                if (ths.mNPCAbductor)
                {
                    ths.DoTimedLoop(AlienUtils.kAbductionLength, ExitReason.None);
                }
            }
            else
            {
                sims = new Sim[] { ths.Actor };
            }

            DateAndTime previous = SimClock.CurrentTime();
            Vector3 landRefPos = ths.GetLandingRefPos(ths.mNPCAbductor);

            while (!ths.Target.TryLandUFOAndExitSims(sims, landRefPos, true))
            {
                Simulator.Sleep(30u);

                if (SimClock.ElapsedTime(TimeUnit.Minutes, previous) > 30f)
                {
                    ths.Target.ForceExitUFODueToLandingFailure(sims);
                    break;
                }
            }

            ths.mFromInventory = (ths.mFromInventory || ths.mNPCAbductor);

            if (ths.mFromInventory)
            {
                ths.mFromInventory = ths.Actor.Inventory.TryToAdd(ths.Target);
            }

            if (!ths.mFromInventory)
            {
                ths.Target.ParkUFO(ths.Actor.LotHome, ths.Actor);
            }

            if (flag)
            {
                if (ths.mNPCAbductor)
                {
                    if (AlienUtilsEx.IsImpregnationSuccessful(ths.SimToAbduct, ths.Actor))
                    {
                        ths.SimToAbduct.SimDescription.Pregnancy = new Pregnancy(ths.SimToAbduct, ths.Actor.SimDescription);
                        ths.SimToAbduct.TraitManager.AddHiddenElement(AlienUtilsEx.sAlienPregnancy);
                    }

                    ths.SimToAbduct.BuffManager.AddElement(BuffNames.Abducted, Origin.FromAbduction);
                    ThoughtBalloonManager.BalloonData data = new ThoughtBalloonManager.BalloonData(ths.Actor.GetThumbnailKey());
                    data.BalloonType = ThoughtBalloonTypes.kThoughtBalloon;
                    data.LowAxis = ThoughtBalloonAxis.kDislike;
                    data.Duration = ThoughtBalloonDuration.Medium;
                    data.mPriority = ThoughtBalloonPriority.High;
                    ths.SimToAbduct.ThoughtBalloonManager.ShowBalloon(data);
                    ths.SimToAbduct.PlayReaction(AlienUtils.kAbductionReactions[RandomUtil.GetInt(0, AlienUtils.kAbductionReactions.Length - 1)], ReactionSpeed.NowOrLater);
                    ths.SimToAbduct.ShowTNSIfSelectable(CarUFO.LocalizeString(ths.SimToAbduct.IsFemale, "NPCAbductionTNS", new object[] { ths.SimToAbduct.ObjectId }),
                        StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ths.SimToAbduct.ObjectId);
                }
                else
                {
                    Sim.ForceSocial(ths.Actor, ths.SimToAbduct, "Reveal Prank", InteractionPriorityLevel.High, true);
                }

                ths.FinishLinkedInteraction(true);
            }

            ths.StandardExit();

            if (flag)
            {
                ths.WaitForSyncComplete();
            }

            return flag;
        }

        public new class Definition : CarUFO.AbductSimA.Definition
        {
            public new static bool CanBeAbducted(Sim alien, Sim abductee)
            {
                if (SocialComponent.IsInServicePreventingSocialization(abductee))
                {
                    return false;
                }

                if (!abductee.IsHuman)
                {
                    return false;
                }

                if (abductee.SimDescription.ChildOrBelow)
                {
                    return false;
                }

                if (AlienUtils.IsHouseboatAndNotDocked(abductee.LotHome))
                {
                    return false;
                }

                GreyedOutTooltipCallback greyedOutTooltipCallback = null;
                return InteractionDefinitionUtilities.IsPass(SocialInteractionA.Definition.CanSocializeWith(null, 
                    alien, abductee, false, ref greyedOutTooltipCallback, false, false, false));
            }

            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                InteractionInstance instance = new AbductSimAEx();
                instance.Init(ref parameters);
                return instance;
            }

            public override string GetInteractionName(Sim actor, CarUFO target, InteractionObjectPair iop)
            {
                return base.GetInteractionName(actor, target, new InteractionObjectPair(sOldSingleton, target));
            }
        }
    }
}
