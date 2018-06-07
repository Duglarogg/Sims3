using NRaas.CommonSpace.Helpers;
using NRaas.AliensSpace.Helpers;
using Sims3.Gameplay;
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

namespace NRaas.AliensSpace.Interactions
{
    public class AbductSimAEx : CarUFO.AbductSimA, Common.IPreLoad, Common.IAddInteraction
    {
        static InteractionDefinition sOldSingleton;

        public new class Definition : CarUFO.AbductSimA.Definition
        {
            public static new bool CanBeAbducted(Sim abductee, Sim abductor)
            {
                if (SocialComponent.IsInServicePreventingSocialization(abductee))
                    return false;

                if (abductee.SimDescription.ChildOrBelow)
                    return false;

                if (!abductee.IsHuman)
                    return false;

                if (AlienUtils.IsHouseboatAndNotDocked(abductee.LotCurrent))
                    return false;

                GreyedOutTooltipCallback greyedOutTooltipCallback = null;

                return InteractionDefinitionUtilities.IsPass(SocialInteractionA.Definition.CanSocializeWith(null, abductor, abductee, false,
                    ref greyedOutTooltipCallback));
            }

            public override string GetInteractionName(Sim actor, CarUFO target, InteractionObjectPair iop)
            {
                return base.GetInteractionName(actor, target, new InteractionObjectPair(sOldSingleton, target));
            }
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Replace<CarUFO, CarUFO.AbductSimA.Definition>(Singleton);
        }

        public new Vector3 GetLandingRefPos(bool isNPCAbductor)
        {
            Mailbox mailbox = null;

            if (isNPCAbductor)
                mailbox = SimToAbduct.LotHome.FindMailbox();
            else
                mailbox = Actor.LotHome.FindMailbox();

            if (mailbox != null)
                Target.mTakeOffPos = mailbox.Position;

            return Target.mTakeOffPos;
        }

        public void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.GetTuning<CarUFO, CarUFO.AbductSimA.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }

            Tunings.Inject<CarUFO, CarUFO.AbductSimA.Definition, Definition>(false);

            sOldSingleton = Singleton;
            Singleton = new Definition();
        }

        public override bool Run()
        {
            return Run(this);
        }

        public static bool Run(CarUFO.AbductSimA abduct)
        {
            if (abduct.Target.InUse)
                return false;

            abduct.mNPCAbductor = abduct.SimToAbduct != null;

            if (!abduct.mNPCAbductor)
                abduct.SimToAbduct = abduct.GetSelectedObject() as Sim;

            if (abduct.SimToAbduct == null)
                return false;

            abduct.StandardEntry();

            if (!abduct.SetupAbductee())
            {
                abduct.StandardExit();
                return false;
            }

            Animation.ForceAnimation(abduct.Actor.ObjectId, true);
            Animation.ForceAnimation(abduct.Target.ObjectId, true);
            abduct.Target.mTakeOffPos = abduct.Actor.Position;

            if (!abduct.Target.RouteToUFOAndTakeOff(abduct.Actor))
            {
                abduct.StandardExit();
                return false;
            }

            Camera.FocusOnGivenPosition(abduct.mJig.Position, CarUFO.kAbductLerpParams, true);
            abduct.BeginCommodityUpdates();
            bool flag = abduct.AbductSim();
            abduct.EndCommodityUpdates(true);
            Sim[] sims = null;

            if (flag)
            {
                EventTracker.SendEvent(EventTypeId.kAbductSimUFO, abduct.Actor, abduct.SimToAbduct);
                sims = new Sim[] { abduct.Actor, abduct.SimToAbduct };

                if (abduct.mNPCAbductor)
                    abduct.DoTimedLoop(Aliens.Settings.mAbductionLength, ExitReason.None);
            }
            else
                sims = new Sim[] { abduct.Actor };

            abduct.mFromInventory = abduct.mFromInventory || abduct.mNPCAbductor;
            DateAndTime previous = SimClock.CurrentTime();
            Vector3 landRefPos = abduct.GetLandingRefPos(abduct.mNPCAbductor);

            while (!abduct.Target.TryLandUFOAndExitSims(sims, landRefPos, true))
            {
                SpeedTrap.Sleep(30u);

                if (SimClock.ElapsedTime(TimeUnit.Minutes, previous) > 30)
                {
                    abduct.Target.ForceExitUFODueToLandingFailure(sims);
                    break;
                }
            }

            if (abduct.mFromInventory)
                abduct.mFromInventory = abduct.Actor.Inventory.TryToAdd(abduct.Target);

            if (!abduct.mFromInventory)
                abduct.Target.ParkUFO(abduct.Actor.LotHome, abduct.Actor);

            if (flag)
            {
                if (abduct.mNPCAbductor)
                {
                    abduct.SimToAbduct.BuffManager.AddElement(BuffsAndTraits.sAbductedEx, Origin.FromAbduction);
                    ThoughtBalloonManager.BalloonData data = new ThoughtBalloonManager.BalloonData(abduct.Actor.GetThumbnailKey());
                    data.BalloonType = ThoughtBalloonTypes.kThoughtBalloon;
                    data.LowAxis = ThoughtBalloonAxis.kDislike;
                    data.Duration = ThoughtBalloonDuration.Medium;
                    data.mPriority = ThoughtBalloonPriority.High;
                    abduct.SimToAbduct.ThoughtBalloonManager.ShowBalloon(data);
                    abduct.SimToAbduct.PlayReaction(RandomUtil.GetRandomObjectFromList(AlienUtils.kAbductionReactions), ReactionSpeed.NowOrLater);
                    StyledNotification.Show(new StyledNotification.Format(CarUFO.LocalizeString(abduct.SimToAbduct.IsFemale, "NPCAbductionTNS",
                        new object[] { abduct.SimToAbduct.ObjectId }), StyledNotification.NotificationStyle.kGameMessagePositive));
                }
                else
                    Sim.ForceSocial(abduct.Actor, abduct.SimToAbduct, "Reveal Prank", InteractionPriorityLevel.High, true);

                abduct.FinishLinkedInteraction(true);
            }

            abduct.StandardExit();

            if (flag)
                abduct.WaitForSyncComplete();

            return flag;
        }

        public new bool TryPlaceJigOnLot()
        {
            Vector3 position = SimToAbduct.Position;
            Vector3 randomDirXZ = RandomUtil.GetRandomDirXZ();
            FindGoodLocationBooleans fglBools = FindGoodLocationBooleans.Routable | FindGoodLocationBooleans.AllowOnStreets 
                | FindGoodLocationBooleans.AllowOnSideWalks | FindGoodLocationBooleans.AllowOffLot;

            if (!GlobalFunctions.FindGoodLocationNearby(mJig, ref position, ref randomDirXZ, 0f, GlobalFunctions.FindGoodLocationStrategies.All, fglBools))
                return false;

            mJig.SetPosition(position);
            mJig.SetForward(randomDirXZ);
            mJig.SetOpacity(0f, 0f);
            mJig.AddToWorld();

            return true;
        }

        public new bool TryPlaceJigOnRoad()
        {
            Vector3 zero = Vector3.Zero;
            Quaternion identity = Quaternion.Identity;

            if (!World.FindPlaceOnRoad(mJig.Proxy, SimToAbduct.Position, 0u, ref zero, ref identity))
                return false;

            mJig.SetPosition(zero);
            Vector3 v = identity.ToMatrix().at.V3;
            mJig.SetForward(v);
            mJig.SetOpacity(0f, 0f);
            mJig.AddToWorld();

            return true;
        }
    }
}
