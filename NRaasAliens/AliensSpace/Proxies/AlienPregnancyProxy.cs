﻿using NRaas.AliensSpace.Helpers;
using NRaas.AliensSpace.Interactions;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
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

namespace NRaas.AliensSpace.Proxies
{
    public class AlienPregnancyProxy : Pregnancy
    {
        static Common.MethodStore sWoohooerGetChanceOfQuads = new Common.MethodStore("NRaasWoohooer", "NRaas.Woohooer", "GetChanceOfQuads", new Type[] { });

        public AlienPregnancyProxy(Sim abductee, SimDescription alien)
        {
            mMom = abductee;
            mDad = alien != null ? alien.CreatedSim : null;
            mMomDeathType = mMom.SimDescription.DeathStyle;
            mMomWasGhostFromPotion = mMom.BuffManager.HasElement(BuffNames.TheUndead);
            mMomOccultType = mMom.OccultManager.CurrentOccultTypes;

            if (alien != null)
            {
                DadDescriptionId = alien.SimDescriptionId;
                mDadDeathType = alien.DeathStyle;
                mDadWasGhostFromPotion = mDad != null && mDad.BuffManager.HasElement(BuffNames.TheUndead);
                mDadOccultType = alien.OccultManager.CurrentOccultTypes;
            }

            mChanceOfRandomOccultMutation = kBaseChanceOfBabyHavingRandomOccultMutation;
            mHourOfPregnancy = 0;
            mRandomGenSeed = RandomUtil.GetInt(2147483647);
            Initialize();
        }

        public AlienPregnancyProxy(Pregnancy src)
        {
            CopyPregnancy(this, src);
        }

        public new void ApplyInitialMutationFactors()
        {
            if (mMom.BuffManager != null && mMom.BuffManager.HasElement(BuffNames.MagicInTheAir))
            {
                mChanceOfRandomOccultMutation += BuffMagicInTheAir.kRandomOccultMutationChanceIncrease;
                mMom.BuffManager.RemoveElement(BuffNames.MagicInTheAir);
            }

            if (mMom.LotCurrent != null && mMom.LotCurrent.IsHouseboatLot() && RandomUtil.RandomChance(TraitTuning.kSailorChanceOfForcedTraitOnHouseboat))
            {
                SetForcedBabyTrait(TraitNames.Sailor);
            }
        }

        public static new bool CanAskToDetermineBabyGender(Sim pregnantSim, Sim doctor)
        {
            return pregnantSim.BuffManager.HasElement(BuffsAndTraits.sXenogenesis) && doctor.Occupation is Medical 
                && doctor.Occupation.Level >= Medical.LevelToDetermineGenderOfBaby;
        }

        public new void CancelReaction(Sim sim, ReactionBroadcaster broadcaster)
        {
            sim.InteractionQueue.CancelInteractionByType(ReactToContractionEx.Singleton);
        }

        public new void CheckForDad()
        {
            SimDescription description = SimDescription.Find(DadDescriptionId);

            if (description != null && description.TeenOrAbove)
                mDad = description.CreatedSim;
        }

        public override void CheckForGhostBaby(Sim s)
        {
            if ((mMomWasGhostFromPotion || mDadWasGhostFromPotion) && RandomUtil.RandomChance(kChanceForGhostBaby))
                Urnstones.SimToPlayableGhost(s.SimDescription, SimDescription.DeathType.OldAge);
        }

        public override void ClearPregnancyData()
        {
            if (mContractionBroadcast != null)
                mContractionBroadcast.Dispose();

            if (mMom != null)
            {
                mMom.RemoveAlarm(PreggersAlarm);
                mMom.RemoveAlarm(mContractionsAlarm);
                mMom.SimDescription.SetPregnancy(0f);
                mMom.RemoveInteractionByType(TakeToHospitalEx.Singleton);
                UnrequestPregnantWalkStyle();

                if (!mMom.HasBeenDestroyed)
                {
                    //mMom.BuffManager.RemoveElement(BuffNames.BabyIsComing);
                    mMom.BuffManager.RemoveElement(BuffsAndTraits.sAlienBabyIsComing);

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
            CheckForDad();
            mTimeMoodSampled = SimClock.CurrentTime();
            mMom.MoodManager.MoodChanged += new MoodManager.MoodChangedCallback(MoodManager_MoodChanged);
            ActiveTopic.AddToSim(mom, "Pregnancy");

            if (momJustCreated)
                PreggersAlarm = mom.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(HourlyCallback), 1f, TimeUnit.Hours,
                    "Hourly Alien Pregnancy Update Alarm", AlarmType.AlwaysPersisted);

            mom.SimDescription.Pregnancy = this;
            int arg_C0_0 = mHourOfPregnancy;
            int arg_BF_0 = Aliens.Settings.mPregnancyShow;

            if (mHourOfPregnancy >= Aliens.Settings.mPregnancyShow)
            {
                if (mHourOfPregnancy < Aliens.Settings.mStartLabor)
                    mMom.BuffManager.AddElement(BuffsAndTraits.sXenogenesis, Origin.FromPregnancy);

                ActiveTopic.AddToSim(mMom, "Announce Pregnancy");
            }

            if (mHourOfPregnancy >= Aliens.Settings.mStartWalk)
            {
                ActiveTopic.AddToSim(mMom, "Pregnant", mMom.SimDescription);
                RequestPregnantWalkStyle();
            }

            if (mHourOfPregnancy >= Aliens.Settings.mStartLabor)
            {
                mMom.BuffManager.AddElement(BuffsAndTraits.sAlienBabyIsComing, Origin.FromPregnancy);

                if (mContractionBroadcast != null)
                    mContractionBroadcast.Dispose();

                mContractionBroadcast = new ReactionBroadcaster(mMom, kContractionBroadcasterParams, 
                    new ReactionBroadcaster.BroadcastCallback(StartReaction), new ReactionBroadcaster.BroadcastCallback(CancelReaction));
                mMom.AddInteraction(TakeToHospitalEx.Singleton);
                InteractionInstance entry = HaveContraction.Singleton.CreateInstance(mMom, mMom, 
                    new InteractionPriority(InteractionPriorityLevel.High, 10f), false, false);
                mMom.InteractionQueue.Add(entry);
                mContractionsAlarm = mMom.AddAlarmRepeating(5f, TimeUnit.Minutes, new AlarmTimerCallback(TriggerContraction), 5f, TimeUnit.Minutes,
                    "Trigger Contractions Alarm", AlarmType.AlwaysPersisted);
            }

            if (mHourOfPregnancy == Aliens.Settings.mPregnancyDuration)
                mMom.AddAlarm(1f, TimeUnit.Minutes, new AlarmTimerCallback(HaveTheBaby), "Re-have the baby.", AlarmType.AlwaysPersisted);

            SetPregoBlendShape();

            if (mMom.SimDescription.IsVisuallyPregnant)
                TryToGiveLeave();

            return true;
        }

        public static void CopyPregnancy(Pregnancy dst, Pregnancy src)
        {
            dst.DadDescriptionId = src.DadDescriptionId;
            dst.mBabySexOffset = src.mBabySexOffset;
            dst.mChanceOfRandomOccultMutation = src.mChanceOfRandomOccultMutation;
            dst.mContractionBroadcast = src.mContractionBroadcast;
            dst.mContractionsAlarm = src.mContractionsAlarm;
            dst.mCurrentMoodScore = src.mCurrentMoodScore;
            dst.mDad = src.mDad;
            dst.mDadDeathType = src.mDadDeathType;
            dst.mDadOccultType = src.mDadOccultType;
            dst.mDadWasGhostFromPotion = src.mDadWasGhostFromPotion;
            dst.mDoctorAdviceGivenBonus = src.mDoctorAdviceGivenBonus;
            dst.mFixupForeignPregnancy = src.mFixupForeignPregnancy;
            dst.mForcedTrait = src.mForcedTrait;
            dst.mGender = src.mGender;
            dst.mHasRequestedWalkStyle = src.mHasRequestedWalkStyle;
            dst.mHasShownPregnancy = src.mHasShownPregnancy;
            dst.mHourOfPregnancy = src.mHourOfPregnancy;
            dst.mMom = src.mMom;
            dst.mMomDeathType = src.mMomDeathType;
            dst.mMomOccultType = src.mMomOccultType;
            dst.mMomWasGhostFromPotion = src.mMomWasGhostFromPotion;
            dst.mMultipleBabiesMultiplier = src.mMultipleBabiesMultiplier;
            dst.mPregnancyScore = src.mPregnancyScore;
            dst.mRandomGenSeed = src.mRandomGenSeed;
            dst.mStereoStartTime = src.mStereoStartTime;
            dst.mTimeMoodSampled = src.mTimeMoodSampled;
            dst.mTvStartTime = src.mTvStartTime;
            dst.PreggersAlarm = src.PreggersAlarm;
        }

        public new void CreateBabyBeforePacking()
        {
            if (GameUtils.IsOnVacation() || GameUtils.IsUniversityWorld())
            {
                if (mMom.Household != null)
                {
                    int numSims = 0;
                    int numPets = 0;
                    mMom.Household.GetNumberOfSimsAndPets(true, out numSims, out numPets);
                    List<SimDescription> list = CreateNewbornsBeforePacking(0f, true, numSims, numPets);

                    foreach (SimDescription current in list)
                        MiniSimDescription.AddMiniSim(current);

                    mMom.Household.InvalidateThumbnail();
                    List<SimDescription> allDescriptions = mMom.Household.AllSimDescriptions;

                    foreach (SimDescription current2 in allDescriptions)
                    {
                        MiniSimDescription miniSimDescription = MiniSimDescription.Find(current2.SimDescriptionId);

                        if (miniSimDescription != null)
                            miniSimDescription.UpdateHouseholdMembers(allDescriptions);
                    }

                    Simulator.Sleep(0u);
                }
            }
            else
            {
                MiniSimDescription miniSimDescription2 = MiniSimDescription.Find(mMom.SimDescription.SimDescriptionId);

                if (miniSimDescription2 != null)
                {
                    int numSims2 = 1;
                    int numPets2 = 0;
                    List<ulong> list2 = new List<ulong>();

                    if (miniSimDescription2.HouseholdMembers != null)
                    {
                        numSims2 = miniSimDescription2.NumSimMemberIncludingPregnant;
                        numPets2 = miniSimDescription2.NumPetMemberIncludingPregnant;
                        list2.AddRange(miniSimDescription2.HouseholdMembers);
                    }
                    else
                        list2.Add(miniSimDescription2.SimDescriptionId);

                    List<SimDescription> list3 = CreateNewbornsBeforePacking(0f, false, numSims2, numPets2);

                    while (list3.Count > 0)
                    {
                        SimDescription description = list3[0];
                        MiniSimDescription.AddMiniSim(description);
                        MiniSimDescription miniSimDescription3 = MiniSimDescription.Find(description.SimDescriptionId);
                        miniSimDescription3.mMotherDescId = miniSimDescription2.SimDescriptionId;
                        list2.Add(description.SimDescriptionId);
                        list3.RemoveAt(0);
                        description.Dispose();
                    }

                    foreach (ulong current3 in list2)
                    {
                        MiniSimDescription miniSimDescription4 = MiniSimDescription.Find(current3);

                        if (miniSimDescription4 != null)
                            miniSimDescription4.UpdateHouseholdMembers(list2);
                    }
                }
            }

            mMom.SimDescription.ClearPregnancyData();

            if (!mMom.HasBeenDestroyed)
                mMom.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday);
        }

        public override List<Sim> CreateNewborns(float bonusMoodPoints, bool interactive, bool homeBirth)
        {
            SimDescription alien = null;
            MiniSimDescription miniAlien = null;

            if (mDad != null && !mDad.HasBeenDestroyed)
                alien = mDad.SimDescription;

            if (alien == null)
            {
                alien = SimDescription.Find(DadDescriptionId);

                if (alien == null)
                {
                    miniAlien = MiniSimDescription.Find(DadDescriptionId);

                    if (miniAlien != null)
                    {
                        alien = miniAlien.UnpackSim();

                        if (alien != null)
                        {
                            Household household = Household.Create(false);

                            if (household != null)
                            {
                                household.AddTemporary(alien);
                                alien.Genealogy.SetSimDescription(alien);
                            }
                        }
                        else
                            miniAlien = null;
                    }
                }
            }

            float averageMoodForBirth = GetAverageMoodForBirth(alien, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numSims = 0;
            int numPets = 0;
            mMom.Household.GetNumberOfSimsAndPets(true, out numSims, out numPets);
            int numBirth = GetNumForBirth(alien, pregoRandom, numSims, numPets);
            Random gen = new Random(mRandomGenSeed);
            List<Sim> list = new List<Sim>();

            for (int i = 0; i < numBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription babyDescription = AlienUtilsEx.MakeAlienBaby(alien, mMom.SimDescription, gender, averageMoodForBirth, pregoRandom, interactive);
                mMom.Household.Add(babyDescription);
                Sim baby = babyDescription.Instantiate(Vector3.Empty);
                baby.AddInteraction(ReturnAlienBabyEx.Singleton);
                baby.SetPosition(mMom.Position);

                if (homeBirth)
                    TotallyHideBaby(baby);

                list.Add(baby);
                CheckForGhostBaby(baby);

                if (baby.SimDescription.IsPlayableGhost)
                    EventTracker.SendEvent(EventTypeId.kHadGhostBaby, mMom, baby);

                if (i == 0)
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBaby, baby.SimDescription));

                MidlifeCrisisManager.OnHadChild(mMom.SimDescription);
                EventTracker.SendEvent(EventTypeId.kNewOffspring, mMom, baby);
                EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mMom);

                if (mDad != null)
                {
                    EventTracker.SendEvent(EventTypeId.kNewOffspring, mDad, baby);
                    EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mDad);
                }

                EventTracker.SendEvent(EventTypeId.kChildBornOrAdopted, null, baby);
            }

            if (miniAlien != null)
            {
                alien.Household.Destroy();
                alien.Household.RemoveTemporary(alien);
                alien.Dispose(true, true);
            }

            if (mMom.Household != null)
                mMom.Household.InvalidateThumbnail();

            switch (numBirth)
            {
                case 2:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTwins, mMom.SimDescription));
                    break;

                case 3:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTriplets, mMom.SimDescription));
                    break;

                default:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabySingle, mMom.SimDescription));
                    break;
            }

            return list;
        }

        public new List<SimDescription> CreateNewbornsBeforePacking(float bonusMoodPoints, bool bAddToFamily, int householdSimMembers, int householdPetMembers)
        {
            MiniSimDescription miniDescription = null;
            SimDescription description;

            if (mDad == null || mDad.HasBeenDestroyed)
            {
                description = SimDescription.Find(DadDescriptionId);

                if (description == null)
                {
                    miniDescription = MiniSimDescription.Find(DadDescriptionId);

                    if (miniDescription != null)
                    {
                        description = miniDescription.UnpackSim();
                        description.Genealogy.SetSimDescription(description);
                    }
                }
            }
            else
                description = mDad.SimDescription;

            float averageMoodForBirth = GetAverageMoodForBirth(description, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numForBirth = GetNumForBirth(description, pregoRandom, householdSimMembers, householdPetMembers);
            Random gen = new Random(mRandomGenSeed);
            List<SimDescription> list = new List<SimDescription>();

            for (int i = 0; i < numForBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription description2 = AlienUtilsEx.MakeAlienBaby(description, mMom.SimDescription, gender, averageMoodForBirth, pregoRandom, false);
                description2.WasCasCreated = false;

                if (bAddToFamily)
                {
                    mMom.Household.Add(description2);
                    Sim sim = description2.Instantiate(mMom.Position);
                    CheckForGhostBaby(sim);
                }

                list.Add(description2);
            }

            if (miniDescription != null)
                description.Dispose(true, true);

            return list;
        }

        public new float GetAverageMoodForBirth(SimDescription alienDescription, float bonusMoodPoints)
        {
            mMom.MoodManager.MoodChanged -= new MoodManager.MoodChangedCallback(MoodManager_MoodChanged);
            MoodManager_MoodChanged();
            float num = mPregnancyScore / 4320f;
            num += kPregnancyBookBonusDad * 2;
            num += kPregnancyBookBonusMom * (float)NumPregnancyBooksRead(mMom.SimDescription);
            num += mDoctorAdviceGivenBonus;
            num += bonusMoodPoints;

            return num;
        }

        public override int GetNumForBirth(SimDescription dadDescription, Random pregoRandom, int numSimMembers, int numPetMembers)
        {
            int result = 1;

            try
            {
                int num = 1;

                if (Aliens.Settings.mUseFertility)
                {
                    mMultipleBabiesMultiplier = Math.Min(mMultipleBabiesMultiplier, kMaxBabyMultiplier);

                    if (mMom.HasTrait(TraitNames.FertilityTreatment))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    else if (mMom.BuffManager != null && mMom.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;

                    double num2 = pregoRandom.NextDouble();

                    int max = sWoohooerGetChanceOfQuads.Valid ? 4 : 3;

                    if (mMom.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                    {
                        mMom.TraitManager.RemoveElement(TraitNames.WishedForLargeFamily);
                        num = max;
                    }

                    if (num != max)
                    {
                        if (num2 < (double)(kChanceOfTwins * mMultipleBabiesMultiplier))
                        {
                            num++;

                            if (num2 < (double)(kChanceOfTriplets * mMultipleBabiesMultiplier))
                            {
                                num++;

                                if (sWoohooerGetChanceOfQuads.Valid)
                                {
                                    if (num2 < (double)(sWoohooerGetChanceOfQuads.Invoke<float>(new Type[] { }) * mMultipleBabiesMultiplier))
                                        num++;
                                }
                            }
                        }
                    }
                }

                result = num;
            }
            catch (Exception exception)
            {
                Common.Exception(Mom.SimDescription, dadDescription, exception);
                result = 1;
            }

            return result;
        }

        public override void HaveTheBaby()
        {
            string msg = mMom.FullName + Common.NewLine +
                "AlienPregnancy.HaveTheBaby" + Common.NewLine + 
                " - Initiating Birth Sequence" + Common.NewLine;

            if (mContractionBroadcast != null)
                mContractionBroadcast.Dispose();

            mMom.RemoveAlarm(PreggersAlarm);
            mMom.RemoveAlarm(mContractionsAlarm);

            msg += " - Pregnancy Alarm Removed" + Common.NewLine +
                " - Contraction Alarm Removed" + Common.NewLine;

            if (mMom.InteractionQueue.HasInteractionOfType(HaveAlienBabyHome.Singleton))
            {
                msg += " - Already Birthing at Home";
                Common.DebugNotify(msg);

                return;
            }

            if (mMom.InteractionQueue.HasInteractionOfType(HaveAlienBabyHospital.Singleton))
            {
                msg += " - Already Birthing at Hospital";

                foreach (InteractionInstance current in mMom.InteractionQueue.InteractionList)
                {
                    HaveAlienBabyHospital haveBabyHosptial = current as HaveAlienBabyHospital;

                    if (haveBabyHosptial != null)
                    {
                        haveBabyHosptial.CancellableByPlayer = false;
                        haveBabyHosptial.BabyShouldBeBorn = true;

                        Common.DebugNotify(msg);

                        return;
                    }
                }
            }

            msg += " - Checking for Hospitals" + Common.NewLine;

            List<RabbitHole> rabbitHoles = RabbitHole.GetRabbitHolesOfType(RabbitHoleType.Hospital);
            float num = mMom.LotHome.GetDistanceToObject(mMom);
            RabbitHole rabbitHole = null;

            foreach (RabbitHole current2 in rabbitHoles)
            {
                float distanceToObject = current2.RabbitHoleProxy.GetDistanceToObject(mMom);

                if (distanceToObject < num)
                {
                    num = distanceToObject;
                    rabbitHole = current2;
                }
            }

            InteractionInstance instance;

            if (rabbitHole != null)
            {
                msg += " - Birthing at Hospital";

                instance = HaveAlienBabyHospital.Singleton.CreateInstance(rabbitHole, mMom,
                    new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false);
                ((HaveAlienBabyHospital)instance).BabyShouldBeBorn = true;
            }
            else
            {
                msg += " - Birthing at Home";

                instance = HaveAlienBabyHome.Singleton.CreateInstance(mMom.LotHome, mMom,
                    new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false);
            }

            mMom.InteractionQueue.Add(instance);
            ActiveTopic.AddToSim(mMom, "Recently Had Baby");

            Common.DebugNotify(msg);
        }

        public override void HourlyCallback()
        {
            if (GameUtils.IsOnVacation() || GameUtils.IsUniversityWorld())
            {
                Common.DebugNotify("AlienPregnancy.HourlyCallback - Pregnancy paused");
                return;
            }

            mHourOfPregnancy++;

            string msg = mMom.FullName + Common.NewLine
                + "AlienPregnancy.HourlyCallback" + Common.NewLine
                + " - Hour: " + mHourOfPregnancy + Common.NewLine;

            if (mHourOfPregnancy == Aliens.Settings.mPregnancyShow)
            {
                msg += " - Showing Pregnancy";

                InteractionInstance instance = ShowAlienPregnancy.Singleton.CreateInstance(mMom, mMom,
                    new InteractionPriority(InteractionPriorityLevel.ESRB), false, false);
                instance.Hidden = true;
                mMom.InteractionQueue.AddNext(instance);

                Common.DebugNotify(msg);

                return;
            }

            if (mMom.Household.IsTouristHousehold)
            {
                msg += " - Mother is Tourist" + Common.NewLine;

                ForeignVisitorsSituation situation = ForeignVisitorsSituation.TryGetForeignVisitorsSituation(mMom);

                if (mHourOfPregnancy == Aliens.Settings.mForeignShowTNS && situation != null)
                    StyledNotification.Show(new StyledNotification.Format(
                        Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:ForeignBabyIsComingTNS",
                        new object[] { mMom }), StyledNotification.NotificationStyle.kGameMessagePositive));

                if (mHourOfPregnancy == Aliens.Settings.mForeignLeaves)
                {
                    msg += " - Sending Pregnant Tourist Home";

                    if (situation != null)
                        situation.MakeGuestGoHome(mMom);
                    else if (mMom.SimDescription.AssignedRole != null)
                        mMom.SimDescription.AssignedRole.RemoveSimFromRole();

                    Common.DebugNotify(msg);
                }

                if (mHourOfPregnancy > Aliens.Settings.mForeignLeaves)
                {
                    msg += " - Walking Back 1 Hour";

                    mHourOfPregnancy--;

                    Common.DebugNotify(msg);

                    return;
                }
            }

            if (mHourOfPregnancy >= Aliens.Settings.mStartWalk)
            {
                ActiveTopic.AddToSim(mMom, "Pregnant", mMom.SimDescription);
                RequestPregnantWalkStyle();
            }

            if (mHourOfPregnancy == Aliens.Settings.mStartLabor)
            {
                msg += " - Beginning Labor" + Common.NewLine;

                for (int i = 0; i < Aliens.Settings.mNumPuddles; i++)
                    PuddleManager.AddPuddle(mMom.PositionOnFloor);

                if (mMom.IsSelectable)
                    StyledNotification.Show(new StyledNotification.Format(Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:BabyIsComingTNS",
                        new object[] { mMom }), StyledNotification.NotificationStyle.kGameMessageNegative), "glb_tns_baby_coming_r2");

                mMom.BuffManager.RemoveElement(BuffsAndTraits.sXenogenesis);
                mMom.BuffManager.AddElement(BuffsAndTraits.sAlienBabyIsComing, Origin.FromPregnancy);

                if (mContractionBroadcast != null)
                    mContractionBroadcast.Dispose();

                mContractionBroadcast = new ReactionBroadcaster(mMom, kContractionBroadcasterParams,
                    new ReactionBroadcaster.BroadcastCallback(StartReaction), new ReactionBroadcaster.BroadcastCallback(CancelReaction));
                mMom.AddInteraction(TakeToHospitalEx.Singleton);
                InteractionInstance entry = HaveContraction.Singleton.CreateInstance(mMom, mMom,
                    new InteractionPriority(InteractionPriorityLevel.High, 10f), false, false);
                mMom.InteractionQueue.Add(entry);
                mContractionsAlarm = mMom.AddAlarmRepeating(5f, TimeUnit.Minutes, new AlarmTimerCallback(TriggerContraction), 5f, TimeUnit.Minutes,
                    "Trigger Contractions Alarm", AlarmType.AlwaysPersisted);
                EventTracker.SendEvent(EventTypeId.kPregnancyContractionsStarted, mMom);
            }

            if (mHourOfPregnancy >= Aliens.Settings.mPregnancyDuration)
            {
                msg += " - Delivering Baby" + Common.NewLine;
                HaveTheBaby();
            }

            SetPregoBlendShape();

            Common.DebugNotify(msg);
        }

        public override void PregnancyComplete(List<Sim> newborns, List<Sim> followers)
        {
            if (mMom.IsSelectable)
                Audio.StartObjectSound(mMom.ObjectId, "sting_baby_born_alien", false);

            Tutorialette.TriggerLesson(Lessons.Babies, mMom);
            EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kHadBaby, mMom, null, this, newborns));

            mMom.RemoveInteractionByType(TakeToHospitalEx.Singleton);
            mMom.BuffManager.RemoveElement(BuffsAndTraits.sAlienBabyIsComing);
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

            BuffNames buffName = BuffNames.ItsABoy;

            switch (newborns.Count)
            {
                case 1:
                    buffName = newborns[0].IsFemale ? BuffNames.ItsAGirl : BuffNames.ItsABoy;
                    break;

                case 2:
                    buffName = BuffNames.ItsTwins;
                    break;

                case 3:
                    buffName = BuffNames.ItsTriplets;
                    break;

                case 4:
                    buffName = CommonPregnancy.sItsQuadruplets;
                    break;
            }

            mMom.BuffManager.AddElement(buffName, Origin.FromNewBaby);

            if (mMom.TraitManager.HasElement(TraitNames.Dramatic))
                mMom.PlayReaction(ReactionTypes.JoyousCrying, ReactionSpeed.NowOrLater);

            if (followers != null)
                foreach (Sim current in followers)
                {
                    if (current.SimDescription.ToddlerOrAbove)
                        current.BuffManager.AddElement(buffName, Origin.FromNewBaby);
                }

            bool flag = !mMom.Household.IsActive;

            foreach (Sim current2 in newborns)
            {
                if (flag)
                    current2.SimDescription.AgingState.AgeUpWithinNDays(kMaxDaysAsNPCBaby);

                current2.SimDescription.PushAgingEnabledToAgingManager();
            }

            mMom.SimDescription.NullOutPregnancy();
        }

        public override void SetHourOfPregnancy(int hour)
        {
            if (hour >= 0 && hour < Aliens.Settings.mPregnancyDuration)
                mHourOfPregnancy = hour;
        }

        public new void SetPregoBlendShape()
        {
            if (mHourOfPregnancy >= Aliens.Settings.mStartLabor)
                return;

            int num = mHourOfPregnancy - Aliens.Settings.mPregnancyShow;

            if (num >= 0)
            {
                string msg = mMom.FullName + Common.NewLine +
                    "AlienPregnancy.SetPregoBlendShape" + Common.NewLine;

                num = Math.Min(num, Aliens.Settings.mPregnancyMorph);
                float num2 = (float)num / (float)Aliens.Settings.mPregnancyMorph;

                if (num2 == 0f)
                    num2 = 0.01f;

                msg += String.Format(" - Base Morph: {0:0.00}%" + Common.NewLine, num2 * 100f);

                float multiplier = 1.1f;

                if (Aliens.Settings.mUseFertility)
                {
                    float multipleBabiesMultiplier = Math.Min(mMultipleBabiesMultiplier, kMaxBabyMultiplier);

                    if (mMom.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                        multipleBabiesMultiplier = 1000f;
                    else if (mMom.TraitManager.HasElement(TraitNames.FertilityTreatment))
                        multipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    else if (mMom.BuffManager != null && mMom.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                        multipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;

                    if (multipleBabiesMultiplier == 1000f)
                    {
                        msg += " - Twins Chance: 100.00%" + Common.NewLine +
                            " - Trips Chance: 100.00%" + Common.NewLine;

                        if (sWoohooerGetChanceOfQuads.Valid)
                        {
                            msg += " - Quads Chance: 100.00%" + Common.NewLine;

                            multiplier = 1.4f;
                        }
                        else
                            multiplier = 1.3f;
                    }
                    else
                    {
                        float twinChance = kChanceOfTwins * multipleBabiesMultiplier;
                        float tripChance = kChanceOfTriplets * multipleBabiesMultiplier;
                        float quadChance = sWoohooerGetChanceOfQuads.Invoke<float>(new object[0]) * multipleBabiesMultiplier;

                        msg += String.Format(" - Twins Chance: {0:0.00}%" + Common.NewLine, twinChance * 100f);
                        msg += String.Format(" - Trips Chance: {0:0.00}%" + Common.NewLine, tripChance * 100f);

                        if (sWoohooerGetChanceOfQuads.Valid)
                            msg += String.Format(" - Quad Chance: {0:0.00}%" + Common.NewLine, quadChance * 100f);

                        if (twinChance >= 1f)
                        {
                            multiplier = 1.2f;

                            if (tripChance >= 1f)
                            {
                                multiplier = 1.3f;

                                if (sWoohooerGetChanceOfQuads.Valid)
                                {
                                    if (quadChance >= 1f)
                                        multiplier = 1.4f;
                                    else
                                        multiplier = 1.3f + (0.1f * quadChance);
                                }
                            }
                            else
                                multiplier = 1.2f + (0.1f * tripChance);
                        }
                        else
                        {
                            multiplier = 1.1f + (0.1f * twinChance);
                        }
                    }
                }
                
                num2 *= multiplier;

                msg += String.Format(" - Mult Morph: {0:0.00}%" + Common.NewLine, num2 * 100f);

                mMom.SimDescription.SetPregnancy(num2, false);

                msg += String.Format(" - Real Morph: {0:0.00}%" + Common.NewLine, mMom.SimDescription.mCurrentShape.Pregnant * 100f);

                if (RandomUtil.RandomChance01(Aliens.Settings.mBackacheChance))
                    mMom.BuffManager.AddElement(BuffNames.Backache, Origin.FromPregnancy);

                Common.DebugNotify(msg);
            }
        }

        public static bool ShouldImpregnate(Sim abductee, SimDescription alien)
        {
            if (!CommonPregnancy.CanGetPregnant(abductee, true, out string reason))
            {
                Common.DebugNotify("Alien Pregnancy: Auto Fail - " + reason);
                return false;
            }

            float chance = CommonPregnancy.sGetChanceOfSuccess(abductee, alien);

            return RandomUtil.RandomChance(chance);
        }

        public static new bool Start(Sim abductee, Sim alien)
        {
            return StartInternal(abductee, alien);
        }

        public static new bool Start(Sim abductee, SimDescription alien)
        {
            return StartInternal(abductee, alien);
        }

        public static new bool StartInternal(Sim abductee, Sim alien)
        {
            return StartInternal(abductee, alien.SimDescription);
        }

        public static new bool StartInternal(Sim abductee, SimDescription alien)
        {
            if (!CommonPregnancy.CanGetPregnant(abductee, true, out string reason))
            {
                Common.DebugNotify("Alien Pregnancy: Auto Fail - " + reason);
                return false;
            }

            AgingManager.Singleton.CancelAgingAlarmsForSim(abductee.SimDescription.AgingState);

            if (abductee.IsHuman)
            {
                AlienPregnancyProxy pregnancy = new AlienPregnancyProxy(abductee, alien);
                pregnancy.PreggersAlarm = abductee.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(pregnancy.HourlyCallback),
                    1f, TimeUnit.Hours, "Hourly Alien Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
                abductee.SimDescription.Pregnancy = pregnancy;
                EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kGotPregnant, abductee, null, pregnancy, null));
            }
            else
            {
                Common.DebugNotify("Alien Pregnancy: Abductee is Pet");
                return false;
            }

            ((AlienPregnancyProxy)abductee.SimDescription.Pregnancy).ApplyInitialMutationFactors();

            return true;
        }

        public new void StartReaction(Sim sim, ReactionBroadcaster broadcaster)
        {
            if (sim.SimDescription.ChildOrAbove)
                sim.InteractionQueue.Add(ReactToContractionEx.Singleton.CreateInstance(mMom, sim, 
                    new InteractionPriority(InteractionPriorityLevel.Pregnancy, -1f), true, true));
        }
    }
}
