using NRaas.CommonSpace.Helpers;
using NRaas.WoohooerSpace.Helpers;
using NRaas.WoohooerSpace.Interactions;
using NRaas.WoohooerSpace.Scoring;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.ActiveCareer.ActiveCareers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.CelebritySystem;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Pools;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.Controller;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.WoohooerSpace.Proxies
{
    public class HumanPregnancyProxy : Pregnancy
    {
        Sim mRobot;
        ulong mRobotDescriptionID;

        public HumanPregnancyProxy(Sim mother, Sim robot, SimDescription dna)
        {
            mMom = mother;
            mMomDeathType = mMom.SimDescription.DeathStyle;
            mMomWasGhostFromPotion = mMom.BuffManager.HasElement(BuffNames.TheUndead);
            mMomOccultType = mMom.OccultManager.CurrentOccultTypes;

            mRobot = robot;
            mRobotDescriptionID = robot.SimDescription.SimDescriptionId;

            mDad = null;
            DadDescriptionId = dna.SimDescriptionId;
            mDadDeathType = dna.DeathStyle;
            mDadWasGhostFromPotion = mRobot != null && mRobot.BuffManager.HasElement(BuffNames.TheUndead);
            mDadOccultType = dna.OccultManager.CurrentOccultTypes;

            mChanceOfRandomOccultMutation = kBaseChanceOfBabyHavingRandomOccultMutation;
            mHourOfPregnancy = 0;
            mRandomGenSeed = RandomUtil.GetInt(2147483647);
            Initialize();
        }

        public HumanPregnancyProxy(Sim mother, SimDescription robot, SimDescription dna)
        {
            mMom = mother;
            mMomDeathType = mMom.SimDescription.DeathStyle;
            mMomWasGhostFromPotion = mMom.BuffManager.HasElement(BuffNames.TheUndead);
            mMomOccultType = mMom.OccultManager.CurrentOccultTypes;

            mRobot = robot.CreatedSim;
            mRobotDescriptionID = robot.SimDescriptionId;

            mDad = null;
            DadDescriptionId = dna.SimDescriptionId;
            mDadDeathType = dna.DeathStyle;
            mDadWasGhostFromPotion = mRobot != null && mRobot.BuffManager.HasElement(BuffNames.TheUndead);
            mDadOccultType = dna.OccultManager.CurrentOccultTypes;

            mChanceOfRandomOccultMutation = kBaseChanceOfBabyHavingRandomOccultMutation;
            mHourOfPregnancy = 0;
            mRandomGenSeed = RandomUtil.GetInt(2147483647);
            Initialize();
        }

        public new void ApplyInitialMutationFactors()
        {
            if (mMom.BuffManager != null && mMom.BuffManager.HasElement(BuffNames.MagicInTheAir))
            {
                mChanceOfRandomOccultMutation += BuffMagicInTheAir.kRandomOccultMutationChanceIncrease;
                mMom.BuffManager.RemoveElement(BuffNames.MagicInTheAir);
            }

            if (mRobot.BuffManager != null && mRobot.BuffManager.HasElement(BuffNames.MagicInTheAir))
            {
                mChanceOfRandomOccultMutation += BuffMagicInTheAir.kRandomOccultMutationChanceIncrease;
                mRobot.BuffManager.RemoveElement(BuffNames.MagicInTheAir);
            }

            if (mMom.LotCurrent != null && Mom.LotCurrent.IsHouseboatLot() && RandomUtil.RandomChance(TraitTuning.kSailorChanceOfForcedTraitOnHouseboat))
                SetForcedBabyTrait(TraitNames.Sailor);
        }

        public override void ClearPregnancyData()
        {
            if (mContractionBroadcast != null)
                mContractionBroadcast.Dispose();

            if (mMom != null)
            {
                mMom.RemoveAlarm(PreggersAlarm);
                mMom.RemoveAlarm(mContractionsAlarm);
                Mom.SimDescription.SetPregnancy(0f);
                // Remove TakeToHospital interaction here!
                UnrequestPregnantWalkStyle();

                if (!mMom.HasBeenDestroyed)
                {
                    // Remove BabyIsComing moodlet here!

                    if (!mMom.SimDescription.IsVampire)
                    {
                        Motive motive = mMom.Motives.GetMotive(CommodityKind.Hunger);

                        if (motive != null)
                            motive.PregnantMotiveDecay = false;
                    }

                    Motive motive2 = mMom.Motives.GetMotive(CommodityKind.Bladder);

                    if (motive2 != null)
                        motive2.PregnantMotiveDecay = false;
                }

                mMom.SimDescription.NullOutPregnancy();
            }
        }

        public override bool Continue(Sim mom, bool momJustCreated)
        {
            if (mom == null)
                return false;

            if (momJustCreated)
                mHasRequestedWalkStyle = false;

            mMom = mom;
            mDad = null;
            mTimeMoodSampled = SimClock.CurrentTime();
            mMom.MoodManager.MoodChanged += new MoodManager.MoodChangedCallback(MoodManager_MoodChanged);
            ActiveTopic.AddToSim(mom, "Pregnancy");

            if (momJustCreated)
                PreggersAlarm = mom.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(HourlyCallback), 1f, TimeUnit.Hours,
                    "Hourly Human Surrogate Pregnancy Update Alarm", AlarmType.AlwaysPersisted);

            mom.SimDescription.Pregnancy = this;
            int arg_C0_0 = mHourOfPregnancy;
            int arg_BF_0 = kHourToShowPregnantBuff;

            if (mHourOfPregnancy >= kHourToShowPregnantBuff)
            {
                if (mHourOfPregnancy < kHourToStartContractions)
                    mMom.BuffManager.AddElement(BuffNames.Pregnant, Origin.FromPregnancy);

                ActiveTopic.AddToSim(mMom, "Announce Pregnancy");
            }

            if (mHourOfPregnancy >= kHourToStartWalkingPregnant)
            {
                ActiveTopic.AddToSim(mMom, "Pregnant", mMom.SimDescription);
                RequestPregnantWalkStyle();
            }

            if (mHourOfPregnancy >= kHourToStartContractions)
            {
                // Add BabyIsComing moodlet here!

                if (mContractionBroadcast != null)
                    mContractionBroadcast.Dispose();

                mContractionBroadcast = new ReactionBroadcaster(mMom, kContractionBroadcasterParams,
                    new ReactionBroadcaster.BroadcastCallback(StartReaction), new ReactionBroadcaster.BroadcastCallback(CancelReaction));
                // Add TakeToHospital interaction here!
                InteractionInstance entry = HaveContraction.Singleton.CreateInstance(mMom, mMom, 
                    new InteractionPriority(InteractionPriorityLevel.High, 10f), false, false);
                mMom.InteractionQueue.Add(entry);
                mContractionsAlarm = mMom.AddAlarmRepeating(5f, TimeUnit.Minutes, new AlarmTimerCallback(TriggerContraction), 5f, TimeUnit.Minutes,
                    "Trigger Contractions Alarm", AlarmType.AlwaysPersisted);
            }

            if (mHourOfPregnancy == kHoursOfPregnancy)
                mMom.AddAlarm(1f, TimeUnit.Minutes, new AlarmTimerCallback(HaveTheBaby), "Re-have the baby", AlarmType.AlwaysPersisted);

            SetPregoBlendShape();

            if (mMom.SimDescription.IsVisuallyPregnant)
                TryToGiveLeave();

            return true;
        }

        public override List<Sim> CreateNewborns(float bonusMoodPoints, bool interactive, bool homeBirth)
        {
            SimDescription robot = mRobot.SimDescription;
            MiniSimDescription miniBot = null;

            if (robot == null)
            {
                robot = SimDescription.Find(mRobotDescriptionID);

                if (robot == null)
                {
                    miniBot = MiniSimDescription.Find(mRobotDescriptionID);

                    if (miniBot != null)
                    {
                        robot = miniBot.UnpackSim();

                        if (robot != null)
                        {
                            Household household = Household.Create(false);

                            if (household != null)
                            {
                                household.AddTemporary(robot);
                                robot.Genealogy.SetSimDescription(robot);
                            }
                        }
                        else
                            miniBot = null;
                    }
                }
            }

            SimDescription father = SimDescription.Find(DadDescriptionId);
            MiniSimDescription miniDad = null;

            if (father == null)
            {
                miniDad = MiniSimDescription.Find(DadDescriptionId);

                if (miniDad != null)
                {
                    father = miniDad.UnpackSim();

                    if (father == null)
                        miniDad = null;
                }
            }

            float averageMoodForBirth = GetAverageMoodForBirth(robot, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numSims = 0;
            int numPets = 0;
            mMom.Household.GetNumberOfSimsAndPets(true, out numSims, out numPets);
            int numBirth = GetNumForBirth(father, pregoRandom, numSims, numPets);
            Random gen = new Random(mRandomGenSeed);
            List<Sim> list = new List<Sim>();

            for (int i = 0; i < numBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription babyDesc;

                if (!mBabyCustomizeData.IsBabyCustomized)
                    babyDesc = Genetics.MakeBaby(father, mMom.SimDescription, gender, averageMoodForBirth, pregoRandom, interactive);
                else
                    babyDesc = Genetics.MakeCustomizedBaby(father, mMom.SimDescription, gender, averageMoodForBirth, pregoRandom, 
                        interactive, mBabyCustomizeData);

                babyDesc.WasCasCreated = false;
                mMom.Household.Add(babyDesc);
                Sim baby = babyDesc.Instantiate(Vector3.Empty);
                baby.SetPosition(mMom.Position);

                if (homeBirth)
                    TotallyHideBaby(baby);

                list.Add(baby);
                CheckForGhostBaby(baby);

                if (baby.SimDescription.IsPlayableGhost)
                {
                    EventTracker.SendEvent(EventTypeId.kHadGhostBaby, mMom, baby);

                    if (mRobot != null)
                        EventTracker.SendEvent(EventTypeId.kHadGhostBaby, mRobot, baby);
                }

                if (i == 0)
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBaby, baby.SimDescription));

                MidlifeCrisisManager.OnHadChild(mMom.SimDescription);
                EventTracker.SendEvent(EventTypeId.kNewOffspring, mMom, baby);
                EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mMom);

                if (mRobot != null)
                {
                    MidlifeCrisisManager.OnHadChild(mRobot.SimDescription);
                    EventTracker.SendEvent(EventTypeId.kNewOffspring, mRobot, baby);
                    EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mRobot);
                }

                EventTracker.SendEvent(EventTypeId.kChildBornOrAdopted, null, baby);

                if (baby.Household.IsActive && ((mRobot != null && !mRobot.TraitManager.HasElement(TraitNames.FutureSim)) || mRobot == null)
                    && !mMom.TraitManager.HasElement(TraitNames.FutureSim) && baby.TraitManager.HasElement(TraitNames.FutureSim))
                    baby.TraitManager.RemoveElement(TraitNames.FutureSim);
            }

            if (miniBot != null)
            {
                robot.Household.Destroy();
                robot.Household.RemoveTemporary(robot);
                robot.Dispose(true, true);
            }

            if (mMom.Household != null)
                mMom.Household.InvalidateThumbnail();

            switch (numBirth)
            {
                case 2:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTwins, mMom.SimDescription));

                    if (mRobot != null)
                        EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTwins, mRobot.SimDescription));
                    break;

                case 3:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTriplets, mMom.SimDescription));

                    if (mRobot != null)
                        EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTriplets, mRobot.SimDescription));
                    break;

                default:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabySingle, mMom.SimDescription));

                    if (mRobot != null)
                        EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabySingle, mRobot.SimDescription));
                    break;
            }

            return list;
        }

        public new List<SimDescription> CreateNewbornsBeforePacking(float bonusMoodPoints, bool addToFamily, int numSims, int numPets)
        {
            SimDescription father;
            MiniSimDescription miniDad = null;

            if (mDad == null || mDad.HasBeenDestroyed)
            {
                father = SimDescription.Find(DadDescriptionId);

                if (father == null)
                {
                    miniDad = MiniSimDescription.Find(DadDescriptionId);

                    if (miniDad != null)
                    {
                        father = miniDad.UnpackSim();
                        father.Genealogy.SetSimDescription(father);
                    }
                }
            }
            else
                father = mDad.SimDescription;

            SimDescription robot;
            MiniSimDescription miniBot = null;

            if (mRobot == null || mRobot.HasBeenDestroyed)
            {
                robot = SimDescription.Find(mRobotDescriptionID);

                if (robot == null)
                {
                    miniBot = MiniSimDescription.Find(mRobotDescriptionID);

                    if (miniBot != null)
                    {
                        robot = miniBot.UnpackSim();
                        robot.Genealogy.SetSimDescription(robot);
                    }
                }
            }
            else
                robot = mRobot.SimDescription;

            float averageMoodForBirth = GetAverageMoodForBirth(robot, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numforBirth = GetNumForBirth(father, pregoRandom, numSims, numPets);
            Random gen = new Random(mRandomGenSeed);
            List<SimDescription> list = new List<SimDescription>();

            for (int i = 0; i < numforBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription babyDesc = Genetics.MakeDescendant(father, mMom.SimDescription, CASAgeGenderFlags.Child, gender, averageMoodForBirth, 
                    pregoRandom, false, true, true, mMom.SimDescription.HomeWorld, false);
                babyDesc.WasCasCreated = false;

                if (addToFamily)
                {
                    mMom.Household.Add(babyDesc);
                    Sim baby = babyDesc.Instantiate(mMom.Position);
                    CheckForGhostBaby(baby);
                }

                list.Add(babyDesc);
            }

            if (miniDad != null)
                father.Dispose(true, true);

            if (miniBot != null)
                robot.Dispose(true, true);

            return list;
        }

        public override int GetNumForBirth(SimDescription dadDescription, Random pregoRandom, int numSimMembers, int numPetMembers)
        {
            int result = 1;

            try
            {
                int desiredNumChildren = 1;

                if (mMom.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    mMom.TraitManager.RemoveElement(TraitNames.WishedForLargeFamily);
                    desiredNumChildren = 4;
                }

                if (mRobot.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    mRobot.TraitManager.RemoveElement(TraitNames.WishedForLargeFamily);
                    desiredNumChildren = 4;
                }

                if (desiredNumChildren != 4)
                {
                    mMultipleBabiesMultiplier = Math.Min(mMultipleBabiesMultiplier, kMaxBabyMultiplier);

                    if (mMom.HasTrait(TraitNames.FertilityTreatment))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    else if (mMom.BuffManager != null && mMom.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;

                    if (mRobot.HasTrait(TraitNames.FertilityTreatment))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    else if (mRobot.BuffManager != null && mRobot.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;

                    if (dadDescription != null)
                    {
                        if (dadDescription.HasTrait(TraitNames.FertilityTreatment))
                            mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    }
                }

                double num2 = pregoRandom.NextDouble();

                if (num2 < kChanceOfTwins * mMultipleBabiesMultiplier)
                {
                    desiredNumChildren++;

                    if (num2 < kChanceOfTriplets * mMultipleBabiesMultiplier)
                    {
                        desiredNumChildren++;

                        if (num2 < Woohooer.Settings.mChanceOfQuads * mMultipleBabiesMultiplier)
                            desiredNumChildren++;
                    }
                }

                result = desiredNumChildren;
            }
            catch (Exception e)
            {
                Common.Exception(Mom.SimDescription, mRobot.SimDescription, e);
                result = 1;
            }

            return result;
        }

        /*
         *  <NOTES>
         *      May need custom birth interactions for human surrogate pregnancies, which need to replace the ones used here.
         *  </NOTES>
         */
        public override void HaveTheBaby()
        {
            string msg = mMom.FullName + Common.NewLine + "HumanSurrogatePregnancy.HaveTheBaby" + Common.NewLine +
                " - Initiating Birth Sequence" + Common.NewLine;

            if (mContractionBroadcast != null)
                mContractionBroadcast.Dispose();

            mMom.RemoveAlarm(PreggersAlarm);
            mMom.RemoveAlarm(mContractionsAlarm);

            msg += " - Pregnancy Alarm Removed" + Common.NewLine + " - Contraction Alarm Removed" + Common.NewLine;

            if (mMom.InteractionQueue.HasInteractionOfType(HaveBabyHomeEx.Singleton))
            {
                msg += " - Already Birthing at Home";
                Common.DebugNotify(msg);

                return;
            }

            if (mMom.InteractionQueue.HasInteractionOfType(HaveBabyHospitalEx.Singleton))
            {
                msg += " - Already Birthing at Hospital";

                foreach (InteractionInstance current in mMom.InteractionQueue.InteractionList)
                {
                    HaveBabyHospitalEx haveBabyHospital = current as HaveBabyHospitalEx;

                    if (haveBabyHospital != null)
                    {
                        haveBabyHospital.CancellableByPlayer = false;
                        haveBabyHospital.BabyShouldBeBorn = true;

                        Common.DebugNotify(msg);

                        return;
                    }
                }
            }

            msg += " - Checking for Hospitals" + Common.NewLine;

            List<RabbitHole> hospitals = RabbitHole.GetRabbitHolesOfType(RabbitHoleType.Hospital);
            float distance = mMom.LotHome.GetDistanceToObject(mMom);
            RabbitHole hospital = null;

            foreach (RabbitHole current2 in hospitals)
            {
                float distanceToHospital = current2.RabbitHoleProxy.GetDistanceToObject(mMom);

                if (distanceToHospital < distance)
                {
                    distance = distanceToHospital;
                    hospital = current2;
                }
            }

            InteractionInstance instance;

            if (hospital != null)
            {
                msg += " - Birthing at Hospital";

                instance = HaveBabyHospitalEx.Singleton.CreateInstance(hospital, mMom, new InteractionPriority(InteractionPriorityLevel.Pregnancy),
                    false, false);
                ((HaveBabyHospitalEx)instance).BabyShouldBeBorn = true;
            }
            else
            {
                msg += " - Birthing at Home";

                instance = HaveBabyHomeEx.Singleton.CreateInstance(mMom.LotHome, mMom, new InteractionPriority(InteractionPriorityLevel.Pregnancy),
                    false, false);
            }

            Common.DebugNotify(msg);

            mMom.InteractionQueue.Add(instance);
            ActiveTopic.AddToSim(mMom, "Recently Had Baby");
        }

        public override void HourlyCallback()
        {
            if (GameUtils.IsOnVacation() || GameUtils.IsUniversityWorld())
            {
                Common.DebugNotify(mMom.FullName + Common.NewLine + "HumanSurrogatePregnancy.HourlyCallback" + Common.NewLine + " - Pregnancy Paused");
                return;
            }

            mHourOfPregnancy++;

            string msg = mMom.FullName + Common.NewLine + "HumanSurrogatePregnancy.HourlyCallback" + Common.NewLine + 
                " - Hour: " + mHourOfPregnancy + Common.NewLine;

            if (mHourOfPregnancy == kHourToStartPregnantMotives)
                mMom.BuffManager.AddElement(BuffNames.Nauseous, Origin.FromUnknown);

            if (mHourOfPregnancy < kHourToShowPregnantBuff && mHourOfPregnancy > kHourToStartPregnantMotives)
                mMom.BuffManager.AddElement(BuffNames.Nauseous, Origin.FromUnknown);

            if (mMom.Household.IsTouristHousehold)
            {
                msg += " - Foreign Sim" + Common.NewLine;

                ForeignVisitorsSituation foreignVisitorsSituation = ForeignVisitorsSituation.TryGetForeignVisitorsSituation(mMom);

                if (mHourOfPregnancy == kForeignSimDisplaysTNS && foreignVisitorsSituation != null)
                    StyledNotification.Show(new StyledNotification.Format(Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:ForeignBabyIsComingTNS",
                        new object[] { mMom }), StyledNotification.NotificationStyle.kGameMessagePositive), "glb_tns_baby_coming_r2");

                if (mHourOfPregnancy == kForeignSimLeavesWorld)
                {
                    if (foreignVisitorsSituation != null)
                        foreignVisitorsSituation.MakeGuestGoHome(mMom);
                    else if (mMom.SimDescription.AssignedRole != null)
                        mMom.SimDescription.AssignedRole.RemoveSimFromRole();
                }

                if (mHourOfPregnancy > kForeignSimLeavesWorld)
                {
                    Common.DebugNotify(msg);

                    mHourOfPregnancy--;
                    return;
                }
            }

            if (mHourOfPregnancy == kHourToShowPregnantBuff)
            {
                msg += " - Adding Pregnant Buff" + Common.NewLine;
                Common.DebugNotify(msg);

                InteractionInstance interactionInstance = ShowPregnancyEx.Singleton.CreateInstance(mMom, mMom, 
                    new InteractionPriority(InteractionPriorityLevel.ESRB), false, false);
                interactionInstance.Hidden = true;
                mMom.InteractionQueue.AddNext(interactionInstance);
                return;
            }

            if (mHourOfPregnancy >= kHourToStartWalkingPregnant)
            {
                ActiveTopic.AddToSim(mMom, "Pregnant", mMom.SimDescription);
                RequestPregnantWalkStyle();
            }

            if (mHourOfPregnancy == kHourToStartContractions)
            {
                msg += " - Starting Labor" + Common.NewLine;

                for (int i = 0; i < kNumberOfPuddlesForWaterBreak; i++)
                    PuddleManager.AddPuddle(mMom.PositionOnFloor);

                if (mMom.IsSelectable)
                {
                    StyledNotification.Show(new StyledNotification.Format(Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:BabyIsComingTNS",
                        new object[] { mMom }), StyledNotification.NotificationStyle.kGameMessageNegative), "glb_tns_baby_coming_r2");
                }

                mMom.BuffManager.RemoveElement(BuffNames.Pregnant);
                mMom.BuffManager.AddElement(BuffNames.BabyIsComing, Origin.FromPregnancy);

                if (mContractionBroadcast != null)
                    mContractionBroadcast.Dispose();

                mContractionBroadcast = new ReactionBroadcaster(mMom, kContractionBroadcasterParams, 
                    new ReactionBroadcaster.BroadcastCallback(StartReaction), new ReactionBroadcaster.BroadcastCallback(CancelReaction));
                mMom.AddInteraction(TakeToHospital.Singleton);
                InteractionInstance entry = HaveContraction.Singleton.CreateInstance(mMom, mMom, new InteractionPriority(InteractionPriorityLevel.High, 10f), false, false);
                mMom.InteractionQueue.Add(entry);
                mContractionsAlarm = mMom.AddAlarmRepeating(5f, TimeUnit.Minutes, new AlarmTimerCallback(TriggerContraction), 
                    5f, TimeUnit.Minutes, "Trigger Contractions Alarm", AlarmType.AlwaysPersisted);
                EventTracker.SendEvent(EventTypeId.kPregnancyContractionsStarted, mMom);
            }

            if (mHourOfPregnancy == kHoursOfPregnancy)
            {
                msg += " - Having the Baby";
                HaveTheBaby();
            }

            SetPregoBlendShape();

            Common.DebugNotify(msg);
        }

        public override void PregnancyComplete(List<Sim> newborns, List<Sim> followers)
        {
            if (mMom.IsSelectable)
                Audio.StartObjectSound(mMom.ObjectId, "sting_baby_born", false);

            Tutorialette.TriggerLesson(Lessons.Babies, mMom);
            EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kHadBaby, mMom, mRobot, this, newborns));

            if (mRobot != null)
                EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kHadBaby, mRobot, mMom, this, newborns));

            mMom.RemoveInteractionByType(TakeToHospital.Singleton);
            mMom.BuffManager.RemoveElement(BuffNames.BabyIsComing);
            UnrequestPregnantWalkStyle();

            if (!mMom.SimDescription.IsVampire)
            {
                Motive motive = mMom.Motives.GetMotive(CommodityKind.Hunger);

                if (motive != null)
                    motive.PregnantMotiveDecay = false;
            }

            Motive motive2 = mMom.Motives.GetMotive(CommodityKind.Bladder);

            if (motive2 != null)
                motive2.PregnantMotiveDecay = false;

            BuffNames buffNames = BuffNames.ItsABoy;

            switch (newborns.Count)
            {
                case 1:
                    if (newborns[0].SimDescription.Gender == CASAgeGenderFlags.Female)
                        buffNames = BuffNames.ItsAGirl;
                    break;

                case 2:
                    buffNames = BuffNames.ItsTwins;
                    break;

                case 3:
                    buffNames = BuffNames.ItsTriplets;
                    break;

                case 4:
                    buffNames = CommonPregnancy.sItsQuadruplets;
                    break;
            }

            mMom.BuffManager.AddElement(buffNames, Origin.FromNewBaby);

            if (mMom.HasTrait(TraitNames.Dramatic))
                mMom.PlayReaction(ReactionTypes.JoyousCrying, ReactionSpeed.NowOrLater);

            if (mRobot != null && !mRobot.HasBeenDestroyed && mRobot.BuffManager.AddElement(buffNames, Origin.FromNewBaby))
            {
                if (mRobot.HasTrait(TraitNames.Dramatic))
                    mRobot.PlayReaction(ReactionTypes.JoyousCrying, ReactionSpeed.NowOrLater);

                if (mRobot.IsSelectable && !mMom.Household.Contains(mRobot.SimDescription))
                {
                    string titleText = null;

                    switch (buffNames)
                    {
                        case BuffNames.ItsABoy:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffBoy", new object[]
                            {
                                mRobot,
                                mMom,
                                newborns[0]
                            });
                            break;

                        case BuffNames.ItsAGirl:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffGirl", new object[]
                            {
                                mRobot,
                                mMom,
                                newborns[0]
                            });
                            break;

                        case BuffNames.ItsTwins:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffTwins", new object[]
                            {
                                mRobot,
                                mMom,
                                newborns[0],
                                newborns[1]
                            });
                            break;

                        case BuffNames.ItsTriplets:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffTriplets", new object[]
                            {
                                mRobot,
                                mMom,
                                newborns[0],
                                newborns[1],
                                newborns[2]
                            });
                            break;

                        default:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffTriplets", new object[]
                            {
                                mRobot,
                                mMom,
                                newborns[0],
                                newborns[1],
                                newborns[2],
                                newborns[3]
                            });
                            break;
                    }

                    StyledNotification.Show(new StyledNotification.Format(titleText, StyledNotification.NotificationStyle.kGameMessagePositive), 
                        "glb_tns_baby_coming_r2");
                }
            }

            if (followers != null)
            {
                foreach (Sim current in followers)
                {
                    if (current.SimDescription.ToddlerOrAbove)
                        current.BuffManager.AddElement(buffNames, Origin.FromNewBaby);
                }
            }

            foreach (Sim current2 in newborns)
            {
                if (!mMom.Household.IsActive)
                    current2.SimDescription.AgingState.AgeUpWithinNDays(kMaxDaysAsNPCBaby);

                current2.SimDescription.PushAgingEnabledToAgingManager();
            }

            mMom.SimDescription.NullOutPregnancy();
        }

        // ShouldImpregnate methods replaced by methods elsewhere

        public static Pregnancy Start(Sim s1, Sim s2, SimDescription s3)
        {
            if (s1.IsFemale)
                return StartInternal(s1, s2, s3);
            else
                return StartInternal(s2, s1, s3);
        }

        public static Pregnancy Start (Sim female, SimDescription male, SimDescription maleDNA)
        {
            return StartInternal(female, male, maleDNA);
        }

        private static Pregnancy StartInternal(Sim woman, Sim robot, SimDescription dna)
        {
            return StartInternal(woman, robot.SimDescription, dna);
        }

        private static Pregnancy StartInternal(Sim woman, SimDescription robot, SimDescription dna)
        {
            if ((woman.SimDescription.IsPlantSim || dna.IsPlantSim) && !Woohooer.Settings.mAllowPlantSimPregnancy)
            {
                IGameObject gameObject = GlobalFunctions.CreateObjectOutOfWorld("forbiddenFruit", ProductVersion.EP9, 
                    "Sims3.Gameplay.Objects.Gardening.ForbiddenFruit", null);

                if (gameObject != null)
                {
                    woman.Inventory.TryToAdd(gameObject);
                    Audio.StartSound("sting_baby_conception");
                }

                return null;
            }
            else
            {
                if (woman.SimDescription.Pregnancy != null)
                    return null;

                if (!woman.Household.IsTouristHousehold && woman.LotHome == null)
                    return null;

                if (!woman.Household.CanAddSpeciesToHousehold(woman.SimDescription.Species, 1, true))
                    return null;

                if (woman.SimDescription.AgingState != null && woman.SimDescription.AgingState.IsAgingInProgress())
                    return null;

                if (Stylist.IsStyleeJobTargetOfAnyStyler(woman))
                    return null;

                AgingManager.Singleton.CancelAgingAlarmsForSim(woman.SimDescription.AgingState);

                if (woman.IsHuman)
                {
                    HumanPregnancyProxy pregnancy = new HumanPregnancyProxy(woman, robot, dna);
                    pregnancy.PreggersAlarm = woman.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(pregnancy.HourlyCallback), 1f, TimeUnit.Hours,
                        "Hourly Human Surrogate Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
                    woman.SimDescription.Pregnancy = pregnancy;
                    EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kGotPregnant, woman, (robot != null) ? robot.CreatedSim : null, pregnancy, null));
                    (woman.SimDescription.Pregnancy as HumanPregnancyProxy).ApplyInitialMutationFactors();

                    return woman.SimDescription.Pregnancy;
                }
                else
                {
                    Common.DebugNotify("HumanPregnancyProxy.StartInternal" + Common.NewLine + " - How did you even get here?!");
                    return null;
                }
            }
        }
    }
}
