using NRaas.CommonSpace.Helpers;
using NRaas.AbductorSpace.Helpers;
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

namespace NRaas.AbductorSpace.Interactions
{
    public class AbductSimAEx : CarUFO.AbductSimA, Common.IPreLoad, Common.IAddInteraction
    {
        static InteractionDefinition sOldSingleton;

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Replace<CarUFO, Definition>(Singleton);
        }

        public void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.GetTuning<CarUFO, Definition>();

            if (tuning != null)
            {
                tuning.Availability.Teens = true;
            }

            Tunings.Inject<CarUFO, Definition, NewDefinition>(false);

            sOldSingleton = Singleton;
            Singleton = new NewDefinition();
        }

        public override bool Run()
        {
            if (Target.InUse) return false;

            mNPCAbductor = (SimToAbduct != null);

            if (!mNPCAbductor) SimToAbduct = GetSelectedObject() as Sim;

            if (SimToAbduct == null) return false;

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

            Camera.FocusOnGivenPosition(mJig.Position, CarUFO.kAbductLerpParams, true);
            BeginCommodityUpdates();
            bool flag = AbductSim();
            EndCommodityUpdates(true);
            Sim[] sims;

            if (flag)
            {
                EventTracker.SendEvent(EventTypeId.kAbductSimUFO, Actor, SimToAbduct);
                sims = new Sim[] { Actor, SimToAbduct };

                if (mNPCAbductor) DoTimedLoop(AlienUtils.kAbductionLength, ExitReason.None);
            }
            else
            {
                sims = new Sim[] { Actor };
            }

            DateAndTime previous = SimClock.CurrentTime();
            Vector3 landRefPos = GetLandingRefPos(mNPCAbductor);

            while (!Target.TryLandUFOAndExitSims(sims, landRefPos, true))
            {
                Simulator.Sleep(30u);

                if (SimClock.ElapsedTime(TimeUnit.Minutes, previous) > 30f)
                {
                    Target.ForceExitUFODueToLandingFailure(sims);
                    break;
                }
            }

            mFromInventory = (mFromInventory || mNPCAbductor);

            if (mFromInventory) mFromInventory = Actor.Inventory.TryToAdd(Target);

            if (!mFromInventory) Target.ParkUFO(Actor.LotHome, Actor);

            if (flag)
            {
                if (mNPCAbductor)
                {
                    AbductionBuffs.ApplyAbductedEx(SimToAbduct, Actor.SimDescription, false);
                    ThoughtBalloonManager.BalloonData data = new ThoughtBalloonManager.BalloonData(Actor.GetThumbnailKey());
                    data.BalloonType = ThoughtBalloonTypes.kThoughtBalloon;
                    data.LowAxis = ThoughtBalloonAxis.kDislike;
                    data.Duration = ThoughtBalloonDuration.Medium;
                    data.mPriority = ThoughtBalloonPriority.High;
                    SimToAbduct.ThoughtBalloonManager.ShowBalloon(data);
                    SimToAbduct.PlayReaction(AlienUtils.kAbductionReactions[RandomUtil.GetInt(0, AlienUtils.kAbductionReactions.Length - 1)], ReactionSpeed.NowOrLater);
                    SimToAbduct.ShowTNSIfSelectable(CarUFO.LocalizeString(SimToAbduct.IsFemale, "NPCAbductionTNS", new object[] { SimToAbduct.ObjectId }),
                        StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, SimToAbduct.ObjectId);
                }
                else
                {
                    Sim.ForceSocial(Actor, SimToAbduct, "Reveal Prank", InteractionPriorityLevel.High, true);
                }

                FinishLinkedInteraction(true);
            }

            StandardExit();

            if (flag) WaitForSyncComplete();

            return flag;
        }

        public class NewDefinition : Definition
        {
            public static new bool CanBeAbducted(Sim abductee, Sim abductor)
            {
                if (SocialComponent.IsInServicePreventingSocialization(abductee)) return false;

                if (abductee.SimDescription.ChildOrBelow) return false;

                if (!abductee.IsHuman) return false;

                //if (abductee.LotCurrent.IsWorldLot) return false;

                if (AlienUtils.IsHouseboatAndNotDocked(abductee.LotCurrent)) return false;

                GreyedOutTooltipCallback greyedOutTooltipCallback = null;

                return InteractionDefinitionUtilities.IsPass(SocialInteractionA.Definition.CanSocializeWith(null, abductor, abductee, false, ref greyedOutTooltipCallback));
            }

            public static new List<Sim> GetValidCandidates(Sim a, CarUFO target)
            {
                List<Sim> list = new List<Sim>();
                Relationship[] relationships = Relationship.GetRelationships(a);

                foreach(Relationship current in relationships)
                {
                    Sim otherSim = current.GetOtherSim(a);

                    if (otherSim != null && a.Household != otherSim.Household && CanBeAbducted(a, otherSim))
                    {
                        list.Add(otherSim);
                    }
                }

                return list;
            }

            public override void PopulatePieMenuPicker(ref InteractionInstanceParameters parameters, out List<ObjectPicker.TabInfo> listObjs, out List<ObjectPicker.HeaderInfo> headers, out int NumSelectableRows)
            {
                if (parameters.Actor.Household.IsAlienHousehold)
                {
                    base.PopulatePieMenuPicker(ref parameters, out listObjs, out headers, out NumSelectableRows);
                    return;
                }

                NumSelectableRows = 1;
                List<Sim> validCandidates = GetValidCandidates(parameters.Actor as Sim, parameters.Target as CarUFO);
                base.PopulatePieMenuPicker(ref parameters, out listObjs, out headers, out NumSelectableRows);
            }

            public override bool Test(Sim a, CarUFO target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!target.CanBeUsedBy(a, false)) return false;

                if (target.InUse)
                {
                    greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(a.IsFemale, "Ui/Tooltip:ObjectInUse", new object[0]));
                    return false;
                }

                if (!a.Household.IsAlienHousehold)
                {
                    List<Sim> validCandidates = GetValidCandidates(a, target);

                    if (validCandidates.Count == 0)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(CarUFO.LocalizeString(a.IsFemale, "AbductSimNoSimsTooltip", new object[0]));
                        return false;
                    }
                }

                return true;
            }

            public override string GetInteractionName(Sim actor, CarUFO target, InteractionObjectPair iop)
            {
                return Common.Localize("AbductSimAEx:MenuName");
            }
        }
    }
}
