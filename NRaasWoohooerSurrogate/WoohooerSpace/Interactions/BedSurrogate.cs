using NRaas.CommonSpace.Helpers;
using NRaas.WoohooerSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.CelebritySystem;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.StoryProgression;
using Sims3.Gameplay.ThoughtBalloons;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NRaas.WoohooerSpace.Interactions
{
    public class BedSurrogate : WooHoo, Common.IPreLoad, Common.IAddInteraction
    {
        static readonly InteractionDefinition DonateSimSingleton = new DonateDefinition();
        static readonly InteractionDefinition ImpregnateSimSingleton = new ImpregnateDefinition();

        static readonly InteractionDefinition DonateBedSingleton = new DonateBedDefinition();
        static readonly InteractionDefinition ImpregnateBedSingleton = new ImpregnateBedDefinition();

        public static ReactionBroadcasterParams sBroadcastParams = null;
        ScientificSample dnaMale = null;
        ScientificSample dnaFemale = null;

        public BedSurrogate()
        {
            if (sBroadcastParams == null)
            {
                sBroadcastParams = Conversation.ReactToSocialParams.Clone();
                sBroadcastParams.AffectBroadcasterRoomOnly = Woohooer.sWasAffectBroadcasterRoomOnly;
            }
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<Sim>(DonateSimSingleton);
            interactions.Add<Sim>(ImpregnateSimSingleton);

            interactions.Add<IBedDouble>(DonateBedSingleton);
            interactions.Add<IBedDouble>(ImpregnateBedSingleton);
        }

        public void OnPreLoad()
        {
            Woohooer.InjectAndReset<Sim, Definition, ProxyDefinition>(false);

            Woohooer.InjectAndReset<Sim, Definition, DonateDefinition>(false);
            Woohooer.InjectAndReset<Sim, TryForBaby.Definition, ImpregnateDefinition>(false);

            Woohooer.InjectAndReset<Bed, DonateDefinition, DonateBedDefinition>(true);
            Woohooer.InjectAndReset<Bed, DonateDefinition, ImpregnateBedDefinition>(true);
        }

        protected void OnPregnancyEvent(StateMachineClient sender, IEvent evt)
        {
            IWooHooDefinition definition = InteractionDefinition as IWooHooDefinition;

            switch (definition.GetStyle(this))
            {
                case CommonWoohoo.WoohooStyle.Safe:
                    CommonSurrogate.DonateDNA(Actor, Target);
                    return;

                case CommonWoohoo.WoohooStyle.Risky:
                case CommonWoohoo.WoohooStyle.TryForBaby:
                    CommonSurrogatePregnancy.GetDNASamples(Actor, Target, ref dnaMale, ref dnaFemale);
                    break;
            }

            if (CommonSurrogatePregnancy.IsSuccess(Actor, Target, Autonomous, definition.GetStyle(this)))
            {
                Pregnancy pregnancy = CommonSurrogatePregnancy.Impregnate(Actor, Target, dnaFemale, dnaMale, Autonomous, definition.GetStyle(this));

                if (pregnancy != null)
                {
                    if (Actor.Posture.Container is HeartShapedBed)
                        pregnancy.SetForcedBabyTrait(TraitNames.Excitable);
                    else if (Actor.Posture.Container is Igloo)
                        pregnancy.SetForcedBabyTrait(TraitNames.LovesTheCold);
                }
            }
        }

        private new void StartJealousyBroadcaster()
        {
            try
            {
                if (mReactToSocialBroadcaster == null)
                {
                    mReactToSocialBroadcaster = new ReactionBroadcaster(Actor, sBroadcastParams, SocialComponentEx.ReactToJealousEventHigh);
                    CommonWoohoo.CheckForWitnessedCheating(Actor, Target, !Rejected);
                }

                if (IsMaster)
                {
                    BedSurrogate linked = LinkedInteractionInstance as BedSurrogate;

                    if (linked != null)
                        linked.StartJealousyBroadcaster();
                }
            }
            catch (Exception e)
            {
                Common.Exception(Actor, Target, e);
            }
        }

        public static bool IsOwner(Sim sim, IBedDouble target)
        {
            if (target.BedOwners().Count > 0)
                if (!target.BedOwners().Contains(sim))
                    return false;

            return true;
        }

        protected bool CanSleep(Sim sim, IBedDouble target)
        {
            InteractionInstance nextInteraction = sim.InteractionQueue.GetNextInteraction();
            bool flag = nextInteraction != null;
            bool flag2 = (flag && (nextInteraction.PosturePreconditions != null)) && nextInteraction.PosturePreconditions.ContainsPosture(CommodityKind.Sleeping);

            if ((((mSituation != null) && mSituation.SomeoneDidIntrude) || (flag && !flag2)) || (target is BedDreamPod))
                return false;

            if (sim.LotHome == sim.LotCurrent)
                if (!IsOwner(sim, target))
                    return false;

            return BedSleep.CanSleep(sim, true);
        }

        public override bool Run()
        {
            try
            {
                Actor.GreetSimOnMyLotIfPossible(Target);

                if (StartBedCuddleA.GetCuddleType(Actor, Target) == StartBedCuddleA.CuddleType.CuddleTargetOnDifferentBed)
                {
                    ChildUtils.SetPosturePrecondition(this, CommodityKind.Relaxing, new CommodityKind[] { CommodityKind.NextToTarget });
                    Actor.InteractionQueue.PushAsContinuation(this, true);

                    return true;
                }

                BedMultiPart container = null;

                try
                {
                    if (Actor.Posture == null)
                        return false;

                    if (!Actor.Posture.Satisfies(CommodityKind.Relaxing, null))
                        return false;

                    if (!SafeToSync())
                        return false;

                    container = Actor.Posture.Container as BedMultiPart;

                    if (container == null)
                        return false;

                    if (IsMaster && ReturnInstance == null)
                    {
                        EnterStateMachine("BedSocials", "FromRelax", "x", "y");
                        AddPersistentScriptEventHandler(0x0, EventCallbackChangeVisibility);
                        SetActor("bed", container);
                        container.PartComp.GetPartSimIsIn(Actor).SetPartParameters(mCurrentStateMachine);
                        WooHoo interaction = InteractionDefinition.CreateInstance(Actor, Target, GetPriority(), false, CancellableByPlayer) as WooHoo;
                        interaction.IsMaster = false;
                        interaction.LinkedInteractionInstance = this;
                        ChildUtils.SetPosturePrecondition(interaction, CommodityKind.Relaxing, new CommodityKind[] { CommodityKind.NextToTarget });
                        Target.InteractionQueue.AddNext(interaction);

                        if (Target.Posture.Container != Actor.Posture.Container)
                        {
                            Actor.LookAtManager.SetInteractionLookAt(Target, 0xc8, LookAtJointFilter.TorsoBones);
                            Actor.Posture.CurrentStateMachine.RequestState("x", "callOver");
                        }

                        Actor.SynchronizationLevel = Sim.SyncLevel.Started;
                        Actor.SynchronizationTarget = Target;
                        Actor.SynchronizationRole = Sim.SyncRole.Initiator;

                        if (!StartSync(IsMaster))
                            return false;

                        if (!Actor.WaitForSynchronizationLevelWithSim(Target, Sim.SyncLevel.Routed, 30f))
                            return false;

                        Actor.SocialComponent.StartSocializingWith(Target);
                    }
                    else if (!StartSync(IsMaster))
                        return false;
                }
                catch (ResetException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Common.DebugException(Actor, Target, e);
                    return false;
                }

                StandardEntry(false);
                BeginCommodityUpdates();
                bool succeeded = false;

                try
                {
                    if (IsMaster)
                    {
                        if (CommonWoohoo.NeedPrivacy(container.InherentlyProvidesPrivacy, Actor, Target))
                        {
                            mSituation = new WooHooPrivacySituation(this);

                            if (!mSituation.Start())
                            {
                                FinishLinkedInteraction();
                                PostLoop();

                                if (ReturnInstance == null)
                                {
                                    InteractionInstance instance = BedRelax.Singleton.CreateInstance(Actor.Posture.Container, Actor, GetPriority(), true, true);
                                    Actor.InteractionQueue.PushAsContinuation(instance, true);
                                }
                                else
                                    DoResume();

                                WooHoo linkedInteractionInstance = LinkedInteractionInstance as WooHoo;

                                if (linkedInteractionInstance != null)
                                {
                                    if (ReturnInstance == null)
                                    {
                                        InteractionInstance instance2 = BedRelax.Singleton.CreateInstance(Target.Posture.Container, Target, GetPriority(), true, true);
                                        Target.InteractionQueue.PushAsContinuation(instance2, true);
                                    }
                                    else
                                        linkedInteractionInstance.DoResume();

                                    linkedInteractionInstance.Failed = true;
                                }

                                WaitForSyncComplete();

                                return false;
                            }
                        }

                        IWooHooDefinition definition = InteractionDefinition as IWooHooDefinition;
                        Actor.LookAtManager.ClearInteractionLookAt();
                        Target.LookAtManager.ClearInteractionLookAt();

                        if (ReturnInstance != null)
                        {
                            ReturnInstance.EnsureMaster();
                            mCurrentStateMachine = ReturnInstance.mCurrentStateMachine;
                        }

                        StartSocial(CommonSurrogate.GetSocialName(definition.GetStyle(this)));
                        InitiateSocialUI(Actor, Target);
                        WooHoo linked = LinkedInteractionInstance as WooHoo;

                        if (linked != null)
                            linked.Rejected = Rejected;

                        if (Rejected)
                        {
                            if (Actor.Posture.Container == Target.Posture.Container)
                            {
                                ThoughtBalloonManager.BalloonData bd = new ThoughtBalloonManager.DoubleBalloonData("balloon_woohoo", "balloon_question");
                                bd.BalloonType = ThoughtBalloonTypes.kSpeechBalloon;
                                Actor.ThoughtBalloonManager.ShowBalloon(bd);
                                AddOneShotScriptEventHandler(0x194, ShowRejectBalloonAndEnqueueRouteAway);
                                mCurrentStateMachine.RequestState(false, "x", "WooHooReject");
                                mCurrentStateMachine.RequestState(true, "y", "WooHooReject");
                                mCurrentStateMachine.RequestState(true, null, "ToRelax");
                            }
                        }
                        else
                        {
                            mCurrentStateMachine.AddOneShotScriptEventHandler(0x6e, OnPregnancyEvent);
                            mCurrentStateMachine.AddOneShotScriptEventHandler(0x6e, EventCallbackChangeClothes);
                            string wooHooEffectName = container.TuningBed.WooHooEffectName;

                            if (!string.IsNullOrEmpty(wooHooEffectName))
                            {
                                mWooHooEffect = VisualEffect.Create(wooHooEffectName);
                                mWooHooEffect.ParentTo(container, Slots.Hash("_FX_0"));
                                AddOneShotScriptEventHandler(0xc8, EventCallbackWooHoo);
                                AddOneShotScriptEventHandler(0xc9, EventCallbackWooHoo);
                            }

                            if (container is BedDreamPod)
                            {
                                AddOneShotScriptEventHandler(0xc8, EventCallbackDreamPodWooHoo);
                                AddOneShotScriptEventHandler(0xc9, EventCallbackDreamPodWooHoo);
                            }

                            Sim.ClothesChangeReason reason = Sim.ClothesChangeReason.GoingToBed;

                            if ((Woohooer.Settings.mNakedOutfitBed) && (!container.IsOutside))
                            {
                                reason = Sim.ClothesChangeReason.GoingToBathe;
                                Woohooer.Settings.AddChange(Actor);
                                Woohooer.Settings.AddChange(Target);
                            }

                            mHelperX = new Sim.SwitchOutfitHelper(Actor, reason);
                            mHelperY = new Sim.SwitchOutfitHelper(Target, reason);
                            mHelperX.Start();
                            mHelperY.Start();
                            mJealousyAlarm = AlarmManager.Global.AddAlarm(kJealousyBroadcasterDelay, TimeUnit.Minutes, StartJealousyBroadcaster, "StartJealousyBroadcaster", AlarmType.DeleteOnReset, container);
                            container.PreWooHooBehavior(Actor, Target, this);
                            mCurrentStateMachine.RequestState(false, "x", "WooHoo");
                            mCurrentStateMachine.RequestState(true, "y", "WooHoo");
                            container.PostWooHooBehavior(Actor, Target, this);
                            Relationship.Get(Actor, Target, true).STC.Update(Actor, Target, CommodityTypes.Amorous, kSTCIncreaseAfterWoohoo);

                            if (CanSleep(Actor, container))
                                SleepAfter = true;
                            else
                            {
                                SleepAfter = false;
                                container.PartComp.GetPartSimIsIn(Actor).BedMade = true;
                            }

                            if (CanSleep(Target, container))
                                (LinkedInteractionInstance as WooHoo).SleepAfter = true;
                            else
                            {
                                (LinkedInteractionInstance as WooHoo).SleepAfter = false;
                                container.PartComp.GetPartSimIsIn(Target).BedMade = true;
                            }

                            /*
                            if (SleepAfter)
                            {
                                mCurrentStateMachine.RequestState(null, "ToSleep");
                            }
                            else*/
                            {
                                mCurrentStateMachine.RequestState(null, "ToRelax");
                            }

                            CommonWoohoo.RunPostWoohoo(Actor, Target, container, definition.GetStyle(this), definition.GetLocation(container), true);

                            if (container is BedDoubleHover)
                            {
                                Actor.BuffManager.AddElement(BuffNames.MeterHighClub, Origin.FromWooHooOnHoverBed);
                                Target.BuffManager.AddElement(BuffNames.MeterHighClub, Origin.FromWooHooOnHoverBed);
                            }

                            if (container is BedDreamPod)
                            {
                                Actor.BuffManager.AddElement(BuffNames.DoubleDreaming, Origin.FromWooHooInDreamPod);
                                Target.BuffManager.AddElement(BuffNames.DoubleDreaming, Origin.FromWooHooInDreamPod);
                            }
                        }

                        FinishSocial(CommonSurrogate.GetSocialName(definition.GetStyle(this)), true);
                        CleanupSituation();
                        Actor.AddExitReason(ExitReason.Finished);
                    }
                    else
                    {
                        container = Target.Posture.Container as BedMultiPart;

                        if (container == null)
                            return false;

                        PartComponent<BedData> partComp = container.PartComp;

                        if (partComp.GetSimInOtherPart(Target) == null)
                        {
                            int num;
                            BedData otherPart = partComp.GetOtherPart(partComp.GetPartSimIsIn(Target));

                            if (!Actor.RouteToSlotListAndCheckInUse(container, otherPart.RoutingSlot, out num))
                            {
                                Actor.AddExitReason(ExitReason.RouteFailed);
                                return false;
                            }

                            Actor.SynchronizationLevel = Sim.SyncLevel.Routed;

                            if (Rejected)
                            {
                                Actor.PlaySoloAnimation("a2a_bed_relax_cuddle_reject_standing_y", true);
                                Actor.RouteAway(kMinDistanceToMoveAwayWhenRejected, kMaxDistanceToMoveAwayWhenRejected, true,
                                    new InteractionPriority(InteractionPriorityLevel.Zero), false, true, true, RouteDistancePreference.NoPreference);

                                return true;
                            }

                            if (!otherPart.RelaxOnBed(Actor, "Enter_BedRelax_" + otherPart.StateNameSuffix))
                                return false;
                        }
                        else
                            Actor.SynchronizationLevel = Sim.SyncLevel.Routed;

                        DoLoop(~(ExitReason.Replan | ExitReason.MidRoutePushRequested | ExitReason.ObjectStateChanged | ExitReason.PlayIdle | ExitReason.MaxSkillPointsReached));

                        if (!Actor.HasExitReason(ExitReason.Finished))
                        {
                            PostLoop();
                            WaitForMasterInteractionToFinish();
                        }
                    }

                    PostLoop();
                    WaitForSyncComplete();

                    succeeded = !Failed && !Rejected;
                }
                finally
                {
                    EndCommodityUpdates(succeeded);
                    StandardExit(false, false);
                }

                if (succeeded)
                {
                    VisitSituation situation = VisitSituation.FindVisitSituationInvolvingGuest(Actor);
                    VisitSituation situation2 = VisitSituation.FindVisitSituationInvolvingGuest(Target);

                    if (situation != null && situation2 != null)
                    {
                        situation.GuestStartingInappropriateAction(Actor, 3.5f);
                        situation2.GuestStartingInappropriateAction(Target, 3.5f);
                    }
                }

                if (succeeded && SleepAfter)
                {
                    if (!Actor.InteractionQueue.HasInteractionOfType(BedSleep.Singleton))
                    {
                        InteractionInstance instance = BedSleep.Singleton.CreateInstance(container, Actor, GetPriority(), Autonomous, CancellableByPlayer);
                        Actor.InteractionQueue.PushAsContinuation(instance, true);
                    }

                    if (VisitSituation.FindVisitSituationInvolvingGuest(Target) != null && Actor.IsAtHome)
                        SocialCallback.OnStayOver(Actor, Target, false);
                    else if (VisitSituation.FindVisitSituationInvolvingGuest(Actor) != null && Target.IsAtHome)
                        SocialCallback.OnStayOver(Target, Actor, false);
                }
                else if (!IsOwner(Actor, container))
                {
                    InteractionInstance instance = Actor.Posture.GetStandingTransition();

                    if (instance != null)
                        Actor.InteractionQueue.Add(instance);
                }

                return succeeded;
            }
            catch (ResetException)
            {
                throw;
            }
            catch (Exception e)
            {
                Woohooer.Settings.AddChange(Actor);
                Woohooer.Settings.AddChange(Target);
                Common.Exception(Actor, Target, e);

                return false;
            }
        }

        public static bool CanCuddleOnBedOfSimA(Sim a, Sim b)
        {
            try
            {
                if (a.Posture == null)
                    return false;

                BedMultiPart container = a.Posture.Container as BedMultiPart;

                if (container == null)
                    return false;

                Sim simInOtherPart = container.PartComp.GetSimInOtherPart(a);

                if (simInOtherPart != null && simInOtherPart != b)
                    return false;

                if (!CanShareBed(a, b))
                    return false;

                if (simInOtherPart == null)
                    return (container.UseCount == 1);

                return (container.UseCount == 2);
            }
            catch (Exception e)
            {
                Common.Exception(a, b, e);
            }

            return false;
        }

        protected static bool CanShareBed(Sim newSim, Sim simUsingBed)
        {
            if (simUsingBed != null)
            {
                WooHoo runningInteraction = simUsingBed.InteractionQueue.RunningInteraction as WooHoo;

                if (runningInteraction != null && newSim != runningInteraction.Target)
                    return false;
            }

            return true;
        }

        public class ProxyDefinition : CommonWoohoo.PrimaryProxyDefinition<Sim, BedSurrogate, BedWoohoo.BaseBedDefinition>
        {
            public ProxyDefinition(BedWoohoo.BaseBedDefinition definition) : base(definition) { }

            public override Sim ITarget(InteractionInstance interaction)
            {
                return interaction.Target as Sim;
            }
        }

        public class DonateBedDefinition : BedWoohoo.BaseBedDefinition
        {
            public DonateBedDefinition() { }

            public DonateBedDefinition(Sim target) : base(target) { }

            public override CommonWoohoo.WoohooStyle GetStyle(InteractionInstance interaction)
            {
                return CommonWoohoo.WoohooStyle.Safe;
            }

            public override string GetInteractionName(Sim actor, IBedDouble target, InteractionObjectPair iop)
            {
                if (actor.IsRobot)
                    return Common.Localize("DonateRobot:MenuName");
                else
                    return Common.Localize("DonateHuman:MenuName");
            }

            protected override bool Satisfies(Sim actor, Sim target, IBedDouble obj, bool isAutonomous, ref GreyedOutTooltipCallback callback)
            {
                try
                {
                    if (!base.Satisfies(actor, target, obj, isAutonomous, ref callback))
                        return false;

                    return CommonSurrogate.SatisfiesDonate(actor, target, "BedDonate", isAutonomous, true, true, ref callback);
                }
                catch (ResetException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Common.Exception(actor, target, e);
                }

                return false;
            }

            public override InteractionDefinition ProxyClone(Sim target)
            {
                return new ProxyDefinition(new DonateBedDefinition(target));
            }
        }

        public class ImpregnateBedDefinition : BedWoohoo.BaseBedDefinition
        {
            public ImpregnateBedDefinition() { }

            public ImpregnateBedDefinition(Sim target) : base(target) { }

            public override CommonWoohoo.WoohooStyle GetStyle(InteractionInstance interaction)
            {
                return CommonWoohoo.WoohooStyle.TryForBaby;
            }

            public override string GetInteractionName(Sim actor, IBedDouble target, InteractionObjectPair iop)
            {
                if (actor.IsFemale)
                    return Common.Localize("ImpregnateFemale:MenuName");
                else
                    return Common.Localize("ImpregnateMale:MenuName");
            }

            protected override bool Satisfies(Sim actor, Sim target, IBedDouble obj, bool isAutonomous, ref GreyedOutTooltipCallback callback)
            {
                try
                {
                    if (!base.Satisfies(actor, target, obj, isAutonomous, ref callback))
                        return false;

                    return CommonSurrogatePregnancy.SatisfiesImpregnate(actor, target, "BedImpregnate", isAutonomous, true, ref callback);
                }
                catch (ResetException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Common.Exception(actor, target, e);
                    return false;
                }
            }

            public override InteractionDefinition ProxyClone(Sim target)
            {
                return new ProxyDefinition(new ImpregnateBedDefinition(target));
            }
        }

        public abstract class BaseDefinition : CommonWoohoo.BaseDefinition<Sim, BedSurrogate>, IOverrideStartInteractionBehavior
        {
            public override Sim GetTarget(Sim actor, Sim target, InteractionInstance interaction)
            {
                return target;
            }

            public override CommonWoohoo.WoohooLocation GetLocation(IGameObject obj)
            {
                return CommonWoohoo.WoohooLocation.Bed;
            }

            public override int Attempts { set { } }

            public void StartInteraction(Sim actor, IGameObject paramTarget)
            {
                try
                {
                    Sim target = paramTarget as Sim;

                    if (actor.Conversation != null && actor.Conversation != target.Conversation)
                        actor.SocialComponent.LeaveConversation();
                }
                catch (Exception e)
                {
                    Common.Exception(actor, paramTarget, e);
                }
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                try
                {
                    if (actor == target)
                    {
                        greyedOutTooltipCallback = Common.DebugTooltip("You cannot Woohoo with yourself!");
                        return false;
                    }

                    if (actor.CurrentInteraction is ISleeping)
                    {
                        greyedOutTooltipCallback = Common.DebugTooltip("Actor Sleeping");
                        return false;
                    }

                    if (target.CurrentInteraction is ISleeping)
                    {
                        greyedOutTooltipCallback = Common.DebugTooltip("Target Sleeping");
                        return false;
                    }

                    if (!actor.Posture.Satisfies(CommodityKind.Relaxing, target))
                    {
                        greyedOutTooltipCallback = Common.DebugTooltip("Actor Not Relaxing On Bed");
                        return false;
                    }

                    if (!isAutonomous && SocialComponent.IsTargetUnavailableForSocialInteraction(target, ref greyedOutTooltipCallback))
                    {
                        if (greyedOutTooltipCallback == null)
                            greyedOutTooltipCallback = Common.DebugTooltip("Target Unavailable.");

                        return false;
                    }

                    if (CanCuddleOnBedOfSimA(actor, target))
                        return true;
                    else if (CanCuddleOnBedOfSimA(target, actor))
                        return true;
                    else
                    {
                        greyedOutTooltipCallback = Common.DebugTooltip("Cuddle Fail");
                        return false;
                    }
                }
                catch (ResetException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Common.Exception(actor, target, e);
                }

                return false;
            }

            public override InteractionDefinition ProxyClone(Sim target)
            {
                throw new NotImplementedException();
            }
        }

        public class DonateDefinition : BaseDefinition
        {
            public string mSocialName;

            public DonateDefinition()
            {
                mSocialName = "Donate";
            }

            public DonateDefinition(string socialName)
            {
                mSocialName = socialName;
            }

            public override CommonWoohoo.WoohooStyle GetStyle(InteractionInstance interaction)
            {
                return CommonWoohoo.WoohooStyle.Safe;
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                if (actor.IsRobot)
                    return Common.Localize("DonateRobot:MenuName");
                else
                    return Common.Localize("DonateHuman:MenuName");
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback callback)
            {
                try
                {
                    if (!base.Test(actor, target, isAutonomous, ref callback))
                        return false;

                    return CommonSurrogate.SatisfiesDonate(actor, target, "BedDonate", isAutonomous, true, true, ref callback);
                }
                catch (ResetException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Common.Exception(actor, target, e);
                }

                return false;
            }
        }

        public class ImpregnateDefinition : BaseDefinition
        {
            public ImpregnateDefinition() { }

            public override CommonWoohoo.WoohooStyle GetStyle(InteractionInstance interaction)
            {
                return CommonWoohoo.WoohooStyle.TryForBaby;
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                if (actor.IsFemale)
                    return Common.Localize("ImpregnateFemale:MenuName");
                else
                    return Common.Localize("ImpregnateMale:MenuName");
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback callback)
            {
                try
                {
                    if (!base.Test(actor, target, isAutonomous, ref callback))
                        return false;

                    return CommonSurrogatePregnancy.SatisfiesImpregnate(actor, target, "BedImpregnate", isAutonomous, true, ref callback);
                }
                catch (ResetException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Common.Exception(actor, target, e);
                }

                return false;
            }
        }

        public class LocationControl : WoohooLocationControl
        {
            public override CommonWoohoo.WoohooLocation Location => CommonWoohoo.WoohooLocation.Bed;

            public override bool Matches(IGameObject obj)
            {
                return obj is BedDouble;
            }

            public override bool HasWoohooableObject(Lot lot)
            {
                return (lot.GetObjects(new Predicate<BedDouble>(TestUse)).Count > 0);
            }

            public override bool HasLocation(Lot lot)
            {
                return (lot.CountObjects<BedDouble>() > 0);
            }

            public override bool AllowLocation(SimDescription sim, bool testVersion)
            {
                if (!sim.IsHuman)
                    return false;

                return Woohooer.Settings.mAutonomousBed;
            }

            protected bool TestUse(BedDouble obj)
            {
                if (!TestRepaired(obj))
                    return false;

                if (obj.UseCount > 0)
                    return false;

                if (obj.NumberOfSpots() < 2)
                    return false;

                return true;
            }

            public override List<GameObject> GetAvailableObjects(Sim actor, Sim target, ItemTestFunction testFunc)
            {
                List<BedDouble> objects = new List<BedDouble>();
                BedDouble bed = actor.Bed as BedDouble;

                if (bed != null)
                {
                    if (bed.LotCurrent == actor.LotCurrent && TestUse(bed))
                        objects.Add(bed);
                }

                bed = target.Bed as BedDouble;

                if (bed != null)
                {
                    if (bed.LotCurrent == target.LotCurrent && TestUse(bed))
                        objects.Add(bed);
                }

                if (objects.Count == 0)
                    objects = actor.LotCurrent.GetObjects(new Predicate<BedDouble>(TestUse));

                List<GameObject> results = new List<GameObject>();

                foreach (GameObject obj in objects)
                {
                    if (testFunc != null && !testFunc(obj, null))
                        continue;

                    results.Add(obj);
                }

                return results;
            }

            public override InteractionDefinition GetInteraction(Sim actor, Sim target, CommonWoohoo.WoohooStyle style)
            {
                switch(style)
                {
                    case CommonWoohoo.WoohooStyle.Safe:
                        return new DonateBedDefinition(target);

                    case CommonWoohoo.WoohooStyle.Risky:
                    case CommonWoohoo.WoohooStyle.TryForBaby:
                        return new ImpregnateBedDefinition(target);
                }

                return null;
            }
        }

        public class TentLocationControl : WoohooLocationControl
        {
            public override CommonWoohoo.WoohooLocation Location => CommonWoohoo.WoohooLocation.Tent;

            public override bool Matches(IGameObject obj)
            {
                return obj is Tent;
            }

            public override bool HasWoohooableObject(Lot lot)
            {
                return lot.GetObjects(new Predicate<Tent>(TestUse)).Count > 0;
            }

            public override bool HasLocation(Lot lot)
            {
                return lot.CountObjects<Tent>() > 0;
            }

            public override bool AllowLocation(SimDescription sim, bool testVersion)
            {
                if (!sim.IsHuman)
                    return false;

                if (testVersion)
                    if (!GameUtils.IsInstalled(ProductVersion.EP1))
                        return false;

                return Woohooer.Settings.mAutonomousTent;
            }

            public bool TestUse(Tent obj)
            {
                if (!TestRepaired(obj))
                    return false;

                if (obj.UseCount > 0)
                    return false;

                if (obj.NumberOfSpots() < 2)
                    return false;

                if (!obj.Placed)
                    return false;

                if (!obj.InWorld)
                    return false;

                return true;
            }

            public override List<GameObject> GetAvailableObjects(Sim actor, Sim target, ItemTestFunction testFunc)
            {
                List<GameObject> results = new List<GameObject>();

                foreach (Tent obj in actor.LotCurrent.GetObjects(new Predicate<Tent>(TestUse)))
                {
                    if (testFunc != null && !testFunc(obj, null))
                        continue;

                    results.Add(obj);
                }

                return results;
            }

            public override InteractionDefinition GetInteraction(Sim actor, Sim target, CommonWoohoo.WoohooStyle style)
            {
                switch(style)
                {
                    case CommonWoohoo.WoohooStyle.Safe:
                        return new DonateBedDefinition(target);

                    case CommonWoohoo.WoohooStyle.Risky:
                    case CommonWoohoo.WoohooStyle.TryForBaby:
                        return new ImpregnateBedDefinition(target);
                }

                return null;
            }
        }

        public class IglooLocationControl : WoohooLocationControl
        {
            public override CommonWoohoo.WoohooLocation Location => CommonWoohoo.WoohooLocation.Igloo;

            public override bool Matches(IGameObject obj)
            {
                return obj is Igloo;
            }

            public override bool HasWoohooableObject(Lot lot)
            {
                return lot.GetObjects(new Predicate<Igloo>(TestUse)).Count > 0;
            }

            public override bool HasLocation(Lot lot)
            {
                return lot.CountObjects<Igloo>() > 0;
            }

            public override bool AllowLocation(SimDescription sim, bool testVersion)
            {
                if (!sim.IsHuman)
                    return false;

                if (testVersion)
                    if (!GameUtils.IsInstalled(ProductVersion.EP8))
                        return false;

                return Woohooer.Settings.mAutonomousIgloo;
            }

            public bool TestUse(Igloo obj)
            {
                if (!TestRepaired(obj))
                    return false;

                if (obj.UseCount > 0)
                    return false;

                if (obj.NumberOfSpots() < 2)
                    return false;

                return true;
            }

            public override List<GameObject> GetAvailableObjects(Sim actor, Sim target, ItemTestFunction testFunc)
            {
                List<GameObject> results = new List<GameObject>();

                foreach (Igloo obj in actor.LotCurrent.GetObjects(new Predicate<Igloo>(TestUse)))
                {
                    if (testFunc != null && !testFunc(obj, null))
                        continue;

                    results.Add(obj);
                }

                return results;
            }

            public override InteractionDefinition GetInteraction(Sim actor, Sim target, CommonWoohoo.WoohooStyle style)
            {
                switch(style)
                {
                    case CommonWoohoo.WoohooStyle.Safe:
                        return new DonateBedDefinition(target);

                    case CommonWoohoo.WoohooStyle.Risky:
                    case CommonWoohoo.WoohooStyle.TryForBaby:
                        return new ImpregnateBedDefinition(target);
                }

                return null;
            }
        }
    }
}
