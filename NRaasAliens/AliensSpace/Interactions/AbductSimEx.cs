using NRaas.AliensSpace.Helpers;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CelebritySystem;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
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
    public class AbductSimEx : Interaction<Sim, CarUFO>, ICountsAsIndoorInteraction, Common.IPreLoad, Common.IAddInteraction
    {
        public class Definition : InteractionDefinition<Sim, CarUFO, AbductSimEx>
        {
            public override string GetInteractionName(Sim actor, CarUFO target, InteractionObjectPair iop)
            {
                return "Never seen - NPC only interaction!";
            }

            public override bool Test(Sim actor, CarUFO target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (actor.IsSelectable)
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("For NPCs only!");
                    return false;
                }

                if (!target.CanBeUsedBy(actor, true))
                    return false;

                if (target.InUse)
                    return false;

                return true;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();
        public Sim SimToAbduct = null;
        public SocialJigTwoPerson mJig;
        public VisualEffect mPreAbductFX;
        public VisualEffect mCarFX;
        public bool mFromInventory = true;

        private bool AbductSim()
        {
            if (!StartSync(true))
                return false;

            Target.SetPosition(mJig.Position);
            Target.SetForward(mJig.ForwardVector);
            mPreAbductFX = VisualEffect.Create("ep8UfoCloaked");
            mPreAbductFX.SetPosAndOrient(Target.Position, Target.ForwardVector, Target.UpVector);
            mPreAbductFX.Start();
            Actor.SynchronizationLevel = Sim.SyncLevel.Routed;

            if (!Actor.WaitForSynchronizationLevelWithSim(SimToAbduct, Sim.SyncLevel.Routed, 60f))
            {
                FinishLinkedInteraction(true);
                return false;
            }

            StateMachineClient mCurrentStateMachine = LinkedInteractionInstance.mCurrentStateMachine;
            mCurrentStateMachine.SetActor("x", Actor);
            mCurrentStateMachine.SetActor("UFO", Target);
            mCurrentStateMachine.EnterState("x", "Enter Abducter");
            mCurrentStateMachine.EnterState("UFO", "Enter Abducter");
            mCurrentStateMachine.AddOneShotScriptEventHandler(100u, new SacsEventHandler(OnAnimationEvent));
            mCurrentStateMachine.AddOneShotScriptEventHandler(101u, new SacsEventHandler(OnAnimationEvent));
            mCurrentStateMachine.AddOneShotScriptEventHandler(102u, new SacsEventHandler(OnAnimationEvent));
            mCurrentStateMachine.RequestState(false, "UFO", "UFO idle");
            mCurrentStateMachine.RequestState(true, "x", "UFO idle");
            DoTimedLoop(CarUFO.kAbductUFOHoverTime, ExitReason.None);
            mCurrentStateMachine.RequestState(false, "UFO", "Exit Abduct");
            mCurrentStateMachine.RequestState(false, "y", "Exit Abduct");
            mCurrentStateMachine.RequestState(true, "x", "Exit Abduct");

            return true;
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<CarUFO>(Singleton);
        }

        public override void Cleanup()
        {
            if (mJig != null)
            {
                mJig.Dispose();
                mJig = null;
            }

            if (mPreAbductFX != null)
            {
                mPreAbductFX.Dispose();
                mPreAbductFX = null;
            }

            if (mCarFX != null)
            {
                mCarFX.Dispose();
                mCarFX = null;
            }

            if (StandardEntryCalled)
            {
                Animation.ForceAnimation(Actor.ObjectId, false);
                Animation.ForceAnimation(Target.ObjectId, false);
                CarUFO.HideMapTag(Actor, false);
                Target.CleanupInteractionStateMachine();
                Target.SetHiddenFlags(HiddenFlags.Nothing);
                Target.FadeIn(false, 0f);

                if (!Target.InInventory)
                    Actor.Inventory.TryToAdd(Target);

                Target.LotHome = null;
                Target.mOwnerLot = null;
            }

            base.Cleanup();
        }

        private bool CreateAndPlaceJig()
        {
            mJig = (GlobalFunctions.CreateObjectOutOfWorld("vehicleUFO_jig", ProductVersion.EP8) as SocialJigTwoPerson);

            if (mJig == null)
                return false;

            mJig.RegisterParticipants(Actor, SimToAbduct);

            if (TryPlaceJigOnLot())
                return true;

            if (TryPlaceJigOnRoad())
                return true;

            mJig.Destroy();
            mJig = null;

            return false;
        }

        private void OnAnimationEvent(StateMachineClient sender, IEvent evt)
        {
            switch (evt.EventId)
            {
                case 100u:
                    SimToAbduct.FadeOut();
                    Target.mPassengers.Add(SimToAbduct);
                    return;

                case 101u:
                    if (mPreAbductFX != null)
                        mPreAbductFX.Stop();

                    Target.FadeIn();
                    Actor.FadeIn();
                    VisualEffect.FireOneShotEffect("ep8UfoWarpFx", Target, Slot.FXJoint_2, VisualEffect.TransitionType.SoftTransition);
                    mCarFX = VisualEffect.Create("ep8ufocar");
                    mCarFX.ParentTo(Target, Slot.FXJoint_2);
                    mCarFX.Start();
                    return;

                case 102u:
                    Actor.FadeOut(false, false, 0u);
                    Target.FadeOut(false, false, 0u);
                    mCarFX.Stop();
                    return;

                default:
                    return;
            }
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

            Tunings.Inject<CarUFO, CarUFO.AbductSimA.Definition, Definition>(true);
        }

        public override bool Run()
        {
            if (Target.InUse)
                return false;

            if (SimToAbduct == null)
                return false;

            StandardEntry();

            if (!SetupAbductee())
            {
                StandardExit();
                return false;
            }

            Animation.ForceAnimation(Actor.ObjectId, true);
            Animation.ForceAnimation(Target.ObjectId, true);
            Target.mTakeOffPos = Actor.Position;

            if (!Target.RouteToUFOAndTakeOff(Actor))
            {
                StandardExit();
                return false;
            }

            if (SimToAbduct.IsSelectable)
                Camera.FocusOnGivenPosition(mJig.Position, CarUFO.kAbductLerpParams, true);

            BeginCommodityUpdates();
            bool flag = AbductSim();
            EndCommodityUpdates(true);

            Sim[] sims;

            if (flag)
            {
                EventTracker.SendEvent(EventTypeId.kAbductSimUFO, Actor, SimToAbduct);
                sims = new Sim[] { Actor, SimToAbduct };
                DoTimedLoop((float)Aliens.Settings.mAbductionLength, ExitReason.None);
            }
            else
                sims = new Sim[] { Actor };

            DateAndTime previousDateAndTime = SimClock.CurrentTime();
            Vector3 landingRefPos = mJig.Position;

            while (!Target.TryLandUFOAndExitSims(sims, landingRefPos, SimToAbduct.IsSelectable))
            {
                Simulator.Sleep(30u);

                if (SimClock.ElapsedTime(TimeUnit.Minutes, previousDateAndTime) > 30f)
                {
                    Target.ForceExitUFODueToLandingFailure(sims);
                    break;
                }
            }

            mFromInventory = Actor.Inventory.TryToAdd(Target);

            if (flag)
            {
                SimToAbduct.BuffManager.AddElement(BuffsAndTraits.sAbductedEx, Origin.FromAbduction);
                ThoughtBalloonManager.BalloonData balloonData = new ThoughtBalloonManager.BalloonData(Actor.GetThumbnailKey());
                balloonData.BalloonType = ThoughtBalloonTypes.kThoughtBalloon;
                balloonData.LowAxis = ThoughtBalloonAxis.kDislike;
                balloonData.Duration = ThoughtBalloonDuration.Medium;
                balloonData.mPriority = ThoughtBalloonPriority.High;
                SimToAbduct.ThoughtBalloonManager.ShowBalloon(balloonData);
                SimToAbduct.PlayReaction(RandomUtil.GetRandomObjectFromList(new List<ReactionTypes>(AlienUtils.kAbductionReactions)), 
                    ReactionSpeed.NowOrLater);
                SimToAbduct.ShowTNSIfSelectable(CarUFO.LocalizeString(SimToAbduct.IsFemale, "NPCAbductionTNS", new object[] { SimToAbduct }),
                    StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, SimToAbduct.ObjectId);
                FinishLinkedInteraction(true);
            }

            StandardExit();

            if (flag)
                WaitForSyncComplete();

            return flag;
        }

        public override bool RunFromInventory()
        {
            mFromInventory = true;
            return Run();
        }

        private bool SetupAbductee()
        {
            if (!CreateAndPlaceJig())
                return false;

            SimToAbduct.InteractionQueue.CancelAllInteractions();
            InteractionPriority priority = new InteractionPriority(InteractionPriorityLevel.High);
            CarUFO.AbductSimB abductSimB = CarUFO.AbductSimB.Singleton.CreateInstance(Actor, SimToAbduct, priority, false, false) as CarUFO.AbductSimB;
            abductSimB.LinkedInteractionInstance = this;
            abductSimB.mJig = mJig;
            abductSimB.mCurrentStateMachine = mCurrentStateMachine;

            if (!SimToAbduct.InteractionQueue.AddNext(abductSimB))
                return false;

            return true;
        }

        private bool TryPlaceJigOnLot()
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

        private bool TryPlaceJigOnRoad()
        {
            Vector3 zero = Vector3.Zero;
            Quaternion identity = Quaternion.Identity;

            if (!World.FindPlaceOnRoad(mJig.Proxy, SimToAbduct.LotCurrent.FindMailbox().Position, 0u, ref zero, ref identity))
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
