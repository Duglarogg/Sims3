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
    public class RobotPregnancyProxy : Pregnancy
    {
        ulong RobotMomDescriptionID;
        ulong RobotDadDescriptionID;
        ulong MomDescriptionID;

        public RobotPregnancyProxy(Sim robot, Sim father, SimDescription dnaF, SimDescription dnaM)
        {
            mMom = robot;
            RobotMomDescriptionID = robot.SimDescription.SimDescriptionId;
            mMomWasGhostFromPotion = robot.BuffManager.HasElement(BuffNames.TheUndead);

            MomDescriptionID = dnaF.SimDescriptionId;
            mMomDeathType = dnaF.DeathStyle;
            mMomOccultType = dnaF.OccultManager.CurrentOccultTypes;

            if (father.IsRobot)
            {
                mDad = father;
                RobotDadDescriptionID = father.SimDescription.SimDescriptionId;
                mDadWasGhostFromPotion = father.BuffManager.HasElement(BuffNames.TheUndead);

                DadDescriptionId = dnaM.SimDescriptionId;
                mDadDeathType = dnaM.DeathStyle;
                mDadOccultType = dnaM.OccultManager.CurrentOccultTypes;
            }
            else
            {
                RobotDadDescriptionID = 0uL;

                mDad = father;
                DadDescriptionId = father.SimDescription.SimDescriptionId;
                mDadDeathType = father.SimDescription.DeathStyle;
                mDadWasGhostFromPotion = father.BuffManager.HasElement(BuffNames.TheUndead);
                mDadOccultType = father.OccultManager.CurrentOccultTypes;
            }

            mChanceOfRandomOccultMutation = kBaseChanceOfBabyHavingRandomOccultMutation;
            mHourOfPregnancy = 0;
            mRandomGenSeed = RandomUtil.GetInt(2147483647);
            Initialize();
        }

        public RobotPregnancyProxy(Sim robot, SimDescription father, SimDescription dnaF, SimDescription dnaM)
        {
            mMom = robot;
            RobotMomDescriptionID = robot.SimDescription.SimDescriptionId;
            mMomWasGhostFromPotion = robot.BuffManager.HasElement(BuffNames.TheUndead);

            MomDescriptionID = dnaF.SimDescriptionId;
            mMomDeathType = dnaF.DeathStyle;
            mMomOccultType = dnaF.OccultManager.CurrentOccultTypes;

            if (father.IsRobot)
            {
                mDad = father.CreatedSim;
                RobotDadDescriptionID = father.SimDescriptionId;
                mDadWasGhostFromPotion = mDad != null && mDad.BuffManager.HasElement(BuffNames.TheUndead);

                DadDescriptionId = dnaM.SimDescriptionId;
                mDadDeathType = dnaM.DeathStyle;
                mDadOccultType = dnaM.OccultManager.CurrentOccultTypes;
            }
            else
            {
                RobotDadDescriptionID = 0uL;

                mDad = father.CreatedSim;
                DadDescriptionId = father.SimDescriptionId;
                mDadDeathType = father.DeathStyle;
                mDadWasGhostFromPotion = mDad != null && mDad.BuffManager.HasElement(BuffNames.TheUndead);
                mDadOccultType = father.OccultManager.CurrentOccultTypes;
            }

            mChanceOfRandomOccultMutation = kBaseChanceOfBabyHavingRandomOccultMutation;
            mHourOfPregnancy = 0;
            mRandomGenSeed = RandomUtil.GetInt(2147483647);
            Initialize();
        }

        public new void CheckForDad()
        {
            SimDescription simDescription;

            if (RobotDadDescriptionID != 0uL)
                simDescription = SimDescription.Find(RobotDadDescriptionID);
            else
                simDescription = SimDescription.Find(DadDescriptionId);

            if (simDescription != null && simDescription.IsMale && simDescription.TeenOrAbove)
                mDad = simDescription.CreatedSim;
        }

        public override void ClearPregnancyData()
        {
            if (mContractionBroadcast != null)
                mContractionBroadcast.Dispose();

            if (mMom != null)
            {
                mMom.RemoveAlarm(PreggersAlarm);
                UnrequestPregnantWalkStyle();

                if (!mMom.HasBeenDestroyed)
                {
                    if (!mMom.SimDescription.IsVampire)
                    {
                        Motive motive = mMom.Motives.GetMotive(CommodityKind.Hunger);

                        if (motive != null)
                            motive.PregnantMotiveDecay = false;
                    }

                    Motive motive2 = mMom.Motives.GetMotive(CommodityKind.Bladder);

                    if (motive2 != null)
                        motive2.PregnantMotiveDecay = false;

                    mMom.SimDescription.NullOutPregnancy();
                }
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
            SimDescription simDescription;

            if (RobotDadDescriptionID != 0uL)
                simDescription = SimDescription.Find(RobotDadDescriptionID);
            else
                simDescription = SimDescription.Find(DadDescriptionId);

            if (simDescription != null && simDescription.IsMale && simDescription.TeenOrAbove)
                mDad = simDescription.CreatedSim;

            mTimeMoodSampled = SimClock.CurrentTime();
            mMom.MoodManager.MoodChanged += new MoodManager.MoodChangedCallback(MoodManager_MoodChanged);
            ActiveTopic.AddToSim(mom, "Pregnancy");

            if (momJustCreated)
                PreggersAlarm = mom.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(HourlyCallback), 1f, TimeUnit.Hours,
                    "Hourly Robot Pregnancy Update Alarm", AlarmType.AlwaysPersisted);

            mom.SimDescription.Pregnancy = this;
            int arg_C0_0 = mHourOfPregnancy;

            if (mHourOfPregnancy >= 0)
            {
                if (mHourOfPregnancy < Woohooer.Settings.mRobotHoursOfPregnancy)
                    mMom.BuffManager.AddElement(BuffNames.Pregnant, Origin.FromPregnancy);

                ActiveTopic.AddToSim(mMom, "Announce Pregnancy");

                if (mDad != null && !mDad.HasBeenDestroyed)
                    ActiveTopic.AddToSim(mDad, "Pregnant", mDad.SimDescription);
            }

            if (mHourOfPregnancy >= Woohooer.Settings.mRobotHourToStartWalkingPregnant)
            {
                ActiveTopic.AddToSim(mMom, "Pregnant", mMom.SimDescription);
                RequestPregnantWalkStyle();
            }

            // Robots will not have a labor stage.

            if (mHourOfPregnancy == Woohooer.Settings.mRobotHoursOfPregnancy)
                mMom.AddAlarm(1f, TimeUnit.Minutes, new AlarmTimerCallback(HaveTheBaby), "Have the baby", AlarmType.AlwaysPersisted);

            TryToGiveLeave();

            return true;
        }

        public new void CreateBabyBeforePacking()
        {
            if (GameUtils.IsOnVacation() || GameUtils.IsUniversityWorld())
            {
                if (mMom.Household != null)
                {
                    int numSims = 0, numPets = 0;
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
                MiniSimDescription miniSimDescription = MiniSimDescription.Find(RobotMomDescriptionID);

                if (miniSimDescription != null)
                {
                    int numSims = 1, numPets = 0;
                    List<ulong> list = new List<ulong>();

                    if (miniSimDescription.HouseholdMembers != null)
                    {
                        numSims = miniSimDescription.NumSimMemberIncludingPregnant;
                        numPets = miniSimDescription.NumPetMemberIncludingPregnant;
                        list.AddRange(miniSimDescription.HouseholdMembers);
                    }
                    else
                        list.Add(miniSimDescription.SimDescriptionId);

                    List<SimDescription> newborns = CreateNewbornsBeforePacking(0f, false, numSims, numPets);

                    while (newborns.Count > 0)
                    {
                        SimDescription simDescription = newborns[0];
                        MiniSimDescription.AddMiniSim(simDescription);
                        MiniSimDescription miniSimDescription2 = MiniSimDescription.Find(simDescription.SimDescriptionId);
                        miniSimDescription2.mMotherDescId = miniSimDescription.SimDescriptionId;
                        list.Add(simDescription.SimDescriptionId);
                        newborns.RemoveAt(0);
                        simDescription.Dispose();
                    }

                    foreach (ulong current3 in list)
                    {
                        MiniSimDescription miniSimDescription4 = MiniSimDescription.Find(current3);

                        if (miniSimDescription4 != null)
                            miniSimDescription4.UpdateHouseholdMembers(list);
                    }
                }
            }

            mMom.SimDescription.ClearPregnancyData();
        }

        public override List<Sim> CreateNewborns(float bonusMoodPoints, bool interactive, bool homeBirth)
        {
            SimDescription geneticMom = null, geneticDad = null, robotDad = null;
            MiniSimDescription miniMom = null, miniDad = null, miniRoboDad = null;
            List<Sim> results;

            GetParentDescriptions(ref geneticMom, ref miniMom, ref geneticDad, ref miniDad, ref robotDad, ref miniRoboDad);

            if (RobotDadDescriptionID != 0uL)
            {
                results = CreateNewborns(geneticMom, robotDad, geneticDad, bonusMoodPoints, interactive, homeBirth);

                if (miniRoboDad != null)
                {
                    robotDad.Household.Destroy();
                    robotDad.Household.RemoveTemporary(robotDad);
                    robotDad.Dispose(true, true);
                }
            }
            else
            {
                results = CreateNewborns(geneticMom, geneticDad, bonusMoodPoints, interactive, homeBirth);

                if (miniDad != null)
                {
                    geneticDad.Household.Destroy();
                    geneticDad.Household.RemoveTemporary(geneticDad);
                    geneticDad.Dispose(true, true);
                }
            }

            switch (results.Count)
            {
                case 2:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTwins, mMom.SimDescription));

                    if (mDad != null)
                        EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTwins, mDad.SimDescription));

                    break;

                case 3:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTriplets, mMom.SimDescription));

                    if (mDad != null)
                        EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTriplets, mDad.SimDescription));

                    break;

                default:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabySingle, mMom.SimDescription));

                    if (mDad != null)
                        EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabySingle, mDad.SimDescription));

                    break;
            }

            if (mMom.Household != null)
                mMom.Household.InvalidateThumbnail();

            return results;
        }

        // For robo mom with meat dad
        private List<Sim> CreateNewborns(SimDescription mother, SimDescription father, float bonusMoodPoints, bool interactive, bool homeBirth)
        {
            float averageMoodForBirth = GetAverageMoodForBirth(father, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numSims = 0, numPets = 0;
            mMom.Household.GetNumberOfSimsAndPets(true, out numSims, out numPets);
            int numForBirth = GetNumForBirth(mother, father, pregoRandom, numSims, numPets);
            Random gen = new Random(mRandomGenSeed);
            List<Sim> list = new List<Sim>();

            for (int i = 0; i < numForBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription babyDesc;

                if (!mBabyCustomizeData.IsBabyCustomized)
                    babyDesc = Genetics.MakeBaby(father, mother, gender, averageMoodForBirth, pregoRandom, interactive);
                else
                    babyDesc = Genetics.MakeCustomizedBaby(father, mother, gender, averageMoodForBirth, pregoRandom, interactive, mBabyCustomizeData);

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

                    if (mDad != null)
                        EventTracker.SendEvent(EventTypeId.kHadGhostBaby, mDad, baby);
                }

                if (i == 0)
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBaby, baby.SimDescription));

                MidlifeCrisisManager.OnHadChild(mMom.SimDescription);
                EventTracker.SendEvent(EventTypeId.kNewOffspring, mMom, baby);
                EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mMom);

                if (mDad != null)
                {
                    MidlifeCrisisManager.OnHadChild(mDad.SimDescription);
                    EventTracker.SendEvent(EventTypeId.kNewOffspring, mDad, baby);
                    EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mDad);
                }

                EventTracker.SendEvent(EventTypeId.kChildBornOrAdopted, null, baby);

                if (baby.Household.IsActive && ((mDad != null && !mDad.TraitManager.HasElement(TraitNames.FutureSim)) || mDad == null)
                    && !mMom.TraitManager.HasElement(TraitNames.FutureSim) && baby.TraitManager.HasElement(TraitNames.FutureSim))
                    baby.TraitManager.RemoveElement(TraitNames.FutureSim);
            }

            return list;
        }

        // For robo mom with robo dad
        private List<Sim> CreateNewborns(SimDescription mother, SimDescription robot, SimDescription father, float bonusMoodPoints, bool interactive, bool homeBirth)
        {
            float averageMoodForBirth = GetAverageMoodForBirth(robot, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numSims = 0, numPets = 0;
            mMom.Household.GetNumberOfSimsAndPets(true, out numSims, out numPets);
            int numBirth = GetNumForBirth(mother, father, pregoRandom, numSims, numPets);
            Random gen = new Random(mRandomGenSeed);
            List<Sim> list = new List<Sim>();

            for (int i = 0; i < numBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription babyDesc;

                if (!mBabyCustomizeData.IsBabyCustomized)
                    babyDesc = Genetics.MakeBaby(father, mother, gender, averageMoodForBirth, pregoRandom, interactive);
                else
                    babyDesc = Genetics.MakeCustomizedBaby(father, mother, gender, averageMoodForBirth, pregoRandom, interactive, mBabyCustomizeData);

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

                    if (mDad != null)
                        EventTracker.SendEvent(EventTypeId.kHadGhostBaby, mDad, baby);
                }

                if (i == 0)
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBaby, baby.SimDescription));

                MidlifeCrisisManager.OnHadChild(mMom.SimDescription);
                EventTracker.SendEvent(EventTypeId.kNewOffspring, mMom, baby);
                EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mMom);

                if (mDad != null)
                {
                    MidlifeCrisisManager.OnHadChild(mDad.SimDescription);
                    EventTracker.SendEvent(EventTypeId.kNewOffspring, mDad, baby);
                    EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mDad);
                }

                EventTracker.SendEvent(EventTypeId.kChildBornOrAdopted, null, baby);

                if (baby.Household.IsActive && ((mDad != null && !mDad.TraitManager.HasElement(TraitNames.FutureSim)) || mDad == null)
                    && !mMom.TraitManager.HasElement(TraitNames.FutureSim) && baby.TraitManager.HasElement(TraitNames.FutureSim))
                    baby.TraitManager.RemoveElement(TraitNames.FutureSim);
            }

            return list;
        }

        public new List<SimDescription> CreateNewbornsBeforePacking(float bonusMoodPoints, bool addToFamily, int numSims, int numPets)
        {
            SimDescription geneticMom = null, geneticDad = null, robotDad = null;
            MiniSimDescription miniMom = null, miniDad = null, miniRoboDad = null;
            List<SimDescription> results;

            GetParentDescriptions(ref geneticMom, ref miniMom, ref geneticDad, ref miniDad, ref robotDad, ref miniRoboDad);

            if (RobotDadDescriptionID != 0uL)
                results = CreateNewbornsBeforePacking(geneticMom, robotDad, geneticDad, bonusMoodPoints, addToFamily, numSims, numPets);
            else
                results = CreateNewbornsBeforePacking(geneticMom, geneticDad, bonusMoodPoints, addToFamily, numSims, numPets);

            if (miniMom != null)
                geneticMom.Dispose(true, true);

            if (miniDad != null)
                geneticDad.Dispose(true, true);

            if (miniRoboDad != null)
                robotDad.Dispose(true, true);

            return results;
        }

        // For robo mom with meat dad
        private List<SimDescription> CreateNewbornsBeforePacking(SimDescription mother, SimDescription father, float bonusMoodPoints, bool addToFamily, int numSims, int numPets)
        {
            float averageMoodForBirth = GetAverageMoodForBirth(father, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numBirth = GetNumForBirth(mother, father, pregoRandom, numSims, numPets);
            Random gen = new Random(mRandomGenSeed);
            List<SimDescription> list = new List<SimDescription>();

            for (int i = 0; i < numBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription baby = CommonSurrogatePregnancy.MakeDescendant(father, mother, null, mMom.SimDescription, CASAgeGenderFlags.Baby,
                    gender, averageMoodForBirth, pregoRandom, false, true, true, mMom.SimDescription.HomeWorld, false);
                baby.WasCasCreated = false;

                if (addToFamily)
                {
                    mMom.Household.Add(baby);
                    Sim sim = baby.Instantiate(mMom.Position);
                    CheckForGhostBaby(sim);
                }

                list.Add(baby);
            }

            return list;
        }

        // For robo mom with robo dad
        private List<SimDescription> CreateNewbornsBeforePacking(SimDescription mother, SimDescription robot, SimDescription father, float bonusMoodPoints, bool addToFamily, int numSims, int numPets)
        {
            float averageMoodForBirth = GetAverageMoodForBirth(robot, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numBirth = GetNumForBirth(mother, father, pregoRandom, numSims, numPets);
            Random gen = new Random(mRandomGenSeed);
            List<SimDescription> list = new List<SimDescription>();

            for (int i = 0; i < numBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription baby = CommonSurrogatePregnancy.MakeDescendant(father, mother, robot, mMom.SimDescription, CASAgeGenderFlags.Baby,
                    gender, averageMoodForBirth, pregoRandom, false, true, true, mMom.SimDescription.HomeWorld, false);
                baby.WasCasCreated = false;

                if (addToFamily)
                {
                    mMom.Household.Add(baby);
                    Sim sim = baby.Instantiate(mMom.Position);
                    CheckForGhostBaby(sim);
                }

                list.Add(baby);
            }

            return list;
        }

        // Robo mom with meat dad
        private int GetNumForBirth(SimDescription geneticMom, SimDescription geneticDad, Random pregoRandom, int numSims, int numPets)
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

                if (mDad.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    mDad.TraitManager.RemoveElement(TraitNames.WishedForLargeFamily);
                    desiredNumChildren = 4;
                }

                if (desiredNumChildren != 4)
                {
                    mMultipleBabiesMultiplier = Math.Min(mMultipleBabiesMultiplier, kMaxBabyMultiplier);

                    if (geneticMom != null && geneticMom.HasTrait(TraitNames.FertilityTreatment))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    else if (mMom.BuffManager != null && mMom.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;

                    if (geneticDad != null && geneticMom.HasTrait(TraitNames.FertilityTreatment))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    else if (mDad.BuffManager != null && mDad.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;

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
                }

                result = desiredNumChildren;

            }
            catch (Exception e)
            {
                Common.Exception(mMom.SimDescription, mDad.SimDescription, e);
            }

            return result;
        }

        private void GetParentDescriptions(ref SimDescription geneticMom, ref MiniSimDescription miniMom, ref SimDescription geneticDad,
            ref MiniSimDescription miniDad, ref SimDescription robotDad, ref MiniSimDescription miniRoboDad)
        {
            geneticMom = SimDescription.Find(MomDescriptionID);

            if (geneticMom == null)
            {
                miniMom = MiniSimDescription.Find(MomDescriptionID);

                if (miniMom != null)
                {
                    geneticMom = miniMom.UnpackSim();

                    if (geneticMom == null)
                        miniMom = null;
                }
            }

            if (RobotDadDescriptionID != 0uL)
            {
                robotDad = mDad.SimDescription;

                if (robotDad == null)
                {
                    robotDad = SimDescription.Find(RobotDadDescriptionID);

                    if (robotDad == null)
                    {
                        miniRoboDad = MiniSimDescription.Find(RobotDadDescriptionID);

                        if (miniRoboDad != null)
                        {
                            robotDad = miniRoboDad.UnpackSim();

                            if (robotDad != null)
                            {
                                Household household = Household.Create(false);

                                if (household != null)
                                {
                                    household.AddTemporary(robotDad);
                                    robotDad.Genealogy.SetSimDescription(robotDad);
                                }
                            }
                            else
                                miniRoboDad = null;
                        }
                    }
                }

                geneticDad = SimDescription.Find(DadDescriptionId);

                if (geneticDad == null)
                {
                    miniDad = MiniSimDescription.Find(DadDescriptionId);

                    if (miniDad != null)
                    {
                        geneticDad = miniDad.UnpackSim();

                        if (geneticDad == null)
                            miniDad = null;
                    }
                }
            }
            else
            {
                geneticDad = mDad.SimDescription;

                if (geneticDad == null)
                {
                    geneticDad = SimDescription.Find(DadDescriptionId);

                    if (geneticDad == null)
                    {
                        miniDad = MiniSimDescription.Find(DadDescriptionId);

                        if (miniDad != null)
                        {
                            geneticDad = miniDad.UnpackSim();

                            if (geneticDad != null)
                            {
                                Household household = Household.Create(false);

                                if (household != null)
                                {
                                    household.AddTemporary(geneticDad);
                                    geneticDad.Genealogy.SetSimDescription(geneticDad);
                                }
                            }
                            else
                                miniDad = null;
                        }
                    }
                }
            }

        }

        // Will use custom birth interaction for robots since they have no labor phase; this method will need a re-write once its implemented
        public override void HaveTheBaby()
        {
            string msg = mMom.FullName + Common.NewLine + "RobotPregnancy.HaveTheBaby" + Common.NewLine +
                " - Initiating Birth Sequence" + Common.NewLine;

            if (mContractionBroadcast != null)
                mContractionBroadcast.Dispose();

            mMom.RemoveAlarm(PreggersAlarm);

            msg += " - Pregnancy Alarm Removed" + Common.NewLine;

            if (mMom.InteractionQueue.HasInteractionOfType(HaveBabyHome.Singleton))
            {
                msg += " - Already Birthing at Home";
                Common.DebugNotify(msg);

                return;
            }

            if (mMom.InteractionQueue.HasInteractionOfType(HaveBabyHospital.Singleton))
            {
                msg += " - Already Birthing at Hospital";

                foreach (InteractionInstance current in mMom.InteractionQueue.InteractionList)
                {
                    HaveBabyHospital haveBabyHospital = current as HaveBabyHospital;

                    if (haveBabyHospital != null)
                    {
                        haveBabyHospital.CancellableByPlayer = false;
                        haveBabyHospital.BabyShouldBeBorn = true;

                        Common.DebugNotify(msg);

                        return;
                    }
                }
            }

            msg += " - Check for Hospitals" + Common.NewLine;

            List<RabbitHole> hospitals = RabbitHole.GetRabbitHolesOfType(RabbitHoleType.Hospital);
            float num = mMom.LotHome.GetDistanceToObject(mMom);
            RabbitHole hospital = null;

            foreach (RabbitHole current2 in hospitals)
            {
                float distanceToObject = current2.RabbitHoleProxy.GetDistanceToObject(mMom);

                if (distanceToObject < num)
                {
                    num = distanceToObject;
                    hospital = current2;
                }
            }

            InteractionInstance interactionInstance;

            if (hospital != null)
            {
                msg += " - Birthing at Hospital";

                interactionInstance = HaveBabyHospital.Singleton.CreateInstance(hospital, mMom,
                    new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false);
                (interactionInstance as HaveBabyHospital).BabyShouldBeBorn = true;
            }
            else
            {
                msg += " - Birthing at Home";

                interactionInstance = HaveBabyHome.Singleton.CreateInstance(mMom.LotHome, mMom,
                    new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false);
            }

            mMom.InteractionQueue.Add(interactionInstance);
            ActiveTopic.AddToSim(mMom, "Recently Had Baby");

            Common.DebugNotify(msg);
        }

        public override void HourlyCallback()
        {
            if (GameUtils.IsOnVacation() || GameUtils.IsUniversityWorld())
            {
                Common.DebugNotify(mMom.FullName + Common.NewLine + "RobotPregnancy.HourlyCallback" + Common.NewLine + " - Pregnancy Paused");
                return;
            }

            mHourOfPregnancy++;

            string msg = mMom.FullName + Common.NewLine + "RobotPregnancy.HourlyCallback" + Common.NewLine + " - Hour: " + mHourOfPregnancy + Common.NewLine;

            if (mMom.Household.IsTouristHousehold)
            {
                msg += " - Foreign Sim" + Common.NewLine;

                ForeignVisitorsSituation foreignVisitorsSituation = ForeignVisitorsSituation.TryGetForeignVisitorsSituation(mMom);

                if (mHourOfPregnancy == Woohooer.Settings.mForeignRobotDisplayTNS && foreignVisitorsSituation != null)
                    StyledNotification.Show(new StyledNotification.Format(Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:ForeignBabyIsComingTNS",
                        new object[] { mMom }), StyledNotification.NotificationStyle.kGameMessagePositive), "glb_tns_baby_coming_r2");

                if (mHourOfPregnancy == Woohooer.Settings.mForeignRobotLeavesWorld)
                {
                    if (foreignVisitorsSituation != null)
                        foreignVisitorsSituation.MakeGuestGoHome(mMom);
                    else if (mMom.SimDescription.AssignedRole != null)
                        mMom.SimDescription.AssignedRole.RemoveSimFromRole();
                }

                if (mHourOfPregnancy > Woohooer.Settings.mForeignRobotLeavesWorld)
                {
                    Common.DebugNotify(msg);

                    mHourOfPregnancy--;
                    return;
                }
            }

            if (mHourOfPregnancy >= Woohooer.Settings.mRobotHourToStartWalkingPregnant)
            {
                ActiveTopic.AddToSim(mMom, "Pregnant", mMom.SimDescription);
                RequestPregnantWalkStyle();
            }

            if (mHourOfPregnancy == Woohooer.Settings.mRobotHoursOfPregnancy)
            {
                msg += " - Having the Baby";
                HaveTheBaby();
            }

            Common.DebugNotify(msg);
        }

        public override void PregnancyComplete(List<Sim> newborns, List<Sim> followers)
        {
            if (mMom.IsSelectable)
                Audio.StartObjectSound(mMom.ObjectId, "sting_baby_born", false);

            Tutorialette.TriggerLesson(Lessons.Babies, mMom);
            EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kHadBaby, mMom, mDad, this, newborns));

            if (mDad != null)
                EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kHadBaby, mDad, mMom, this, newborns));

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
                    if (newborns[0].IsFemale)
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

            if (mDad != null && !mDad.HasBeenDestroyed && mDad.BuffManager.AddElement(buffNames, Origin.FromNewBaby))
            {
                if (mDad.HasTrait(TraitNames.Dramatic))
                    mDad.PlayReaction(ReactionTypes.JoyousCrying, ReactionSpeed.NowOrLater);

                if (mDad.IsSelectable && !mMom.Household.Contains(mDad.SimDescription))
                {
                    string titleText = null;

                    switch (buffNames)
                    {
                        case BuffNames.ItsABoy:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffBoy", new object[]
                            {
                                mDad,
                                mMom,
                                newborns[0]
                            });
                            break;

                        case BuffNames.ItsAGirl:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffGirl", new object[]
                            {
                                mDad,
                                mMom,
                                newborns[0]
                            });
                            break;

                        case BuffNames.ItsTwins:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffTwins", new object[]
                            {
                                mDad,
                                mMom,
                                newborns[0],
                                newborns[1]
                            });
                            break;

                        case BuffNames.ItsTriplets:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffTriplets", new object[]
                            {
                                mDad,
                                mMom,
                                newborns[0],
                                newborns[1],
                                newborns[2]
                            });
                            break;

                        default:
                            titleText = Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:DadBabyBuffTriplets", new object[]
                            {
                                mDad,
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

        // Robo mom with meat dad
        public static Pregnancy Start(Sim s1, Sim s2, SimDescription s3)
        {
            if (s1.IsFemale)
                return StartInternal(s1, s2, s3);
            else
                return StartInternal(s2, s1, s3);
        }

        public static Pregnancy Start(Sim female, SimDescription male, SimDescription dnaFemale)
        {
            return StartInternal(female, male, dnaFemale);
        }

        private static Pregnancy StartInternal(Sim femBot, Sim male, SimDescription dnaF)
        {
            return StartInternal(femBot, male.SimDescription, dnaF);
        }

        private static Pregnancy StartInternal(Sim femBot, SimDescription male, SimDescription dnaF)
        {
            // Start robo surrogate pregnancy here!
            if ((dnaF.IsPlantSim || male.IsPlantSim) && !Woohooer.Settings.mAllowPlantSimPregnancy)
            {
                IGameObject gameObject = GlobalFunctions.CreateObjectOutOfWorld("forbiddenFruit", ProductVersion.EP9,
                    "Sims3.Gameplay.Objects.Gardening.ForbiddenFruit", null);

                if (gameObject != null)
                {
                    femBot.Inventory.TryToAdd(gameObject);
                    Audio.StartSound("sting_baby_conception");
                }

                return null;
            }
            else
            {
                if (femBot.SimDescription.Pregnancy != null)
                    return null;

                if (!femBot.Household.IsTouristHousehold && femBot.LotHome == null)
                    return null;

                if (!femBot.Household.CanAddSpeciesToHousehold(femBot.SimDescription.Species, 1, true))
                    return null;

                if (femBot.SimDescription.AgingState != null && femBot.SimDescription.AgingState.IsAgingInProgress())
                    return null;

                if (Stylist.IsStyleeJobTargetOfAnyStyler(femBot))
                    return null;

                AgingManager.Singleton.CancelAgingAlarmsForSim(femBot.SimDescription.AgingState);

                if (femBot.IsHuman)
                {
                    RobotPregnancyProxy pregnancy = new RobotPregnancyProxy(femBot, male, dnaF, null);
                    pregnancy.PreggersAlarm = femBot.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(pregnancy.HourlyCallback), 1f, TimeUnit.Hours,
                        "Hourly Robot Surrogate Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
                    femBot.SimDescription.Pregnancy = pregnancy;
                    EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kGotPregnant, femBot, (male != null) ? male.CreatedSim : null, pregnancy, null));
                    (femBot.SimDescription.Pregnancy as RobotPregnancyProxy).ApplyInitialMutationFactors();

                    return femBot.SimDescription.Pregnancy;
                }
                else
                {
                    Common.DebugNotify("RobotPregnancyProxy.StartInternal" + Common.NewLine + " - How did you even get here?!");
                    return null;
                }
            }
        }

        // Robo mom with robo dad
        public static Pregnancy Start(Sim s1, Sim s2, SimDescription s3, SimDescription s4)
        {
            if (s1.IsFemale)
            {
                if (s3.IsFemale)
                    return StartInternal(s1, s2, s3, s4);
                else
                    return StartInternal(s1, s2, s4, s3);
            }
            else
            {
                if (s3.IsFemale)
                    return StartInternal(s2, s1, s3, s4);
                else
                    return StartInternal(s2, s1, s4, s3);
            }


            return null;
        }

        public static Pregnancy Start(Sim female, SimDescription male, SimDescription dnaFemale, SimDescription dnaMale)
        {
            return StartInternal(female, male, dnaFemale, dnaMale);
        }

        private static Pregnancy StartInternal(Sim femBot, Sim manBot, SimDescription dnaF, SimDescription dnaM)
        {
            return StartInternal(femBot, manBot.SimDescription, dnaF, dnaM);
        }

        private static Pregnancy StartInternal(Sim femBot, SimDescription manBot, SimDescription dnaF, SimDescription dnaM)
        {
            // Start robo surrogate pregnancy here!

            if ((dnaF.IsPlantSim || dnaM.IsPlantSim) && !Woohooer.Settings.mAllowPlantSimPregnancy)
            {
                IGameObject gameObject = GlobalFunctions.CreateObjectOutOfWorld("forbiddenFruit", ProductVersion.EP9,
                    "Sims3.Gameplay.Objects.Gardening.ForbiddenFruit", null);

                if (gameObject != null)
                {
                    femBot.Inventory.TryToAdd(gameObject);
                    Audio.StartSound("sting_baby_conception");
                }

                return null;
            }
            else
            {
                if (femBot.SimDescription.Pregnancy != null)
                    return null;

                if (!femBot.Household.IsTouristHousehold && femBot.LotHome == null)
                    return null;

                if (!femBot.Household.CanAddSpeciesToHousehold(femBot.SimDescription.Species, 1, true))
                    return null;

                if (femBot.SimDescription.AgingState != null && femBot.SimDescription.AgingState.IsAgingInProgress())
                    return null;

                if (Stylist.IsStyleeJobTargetOfAnyStyler(femBot))
                    return null;

                AgingManager.Singleton.CancelAgingAlarmsForSim(femBot);

                if (femBot.IsHuman)
                {
                    RobotPregnancyProxy pregnancy = new RobotPregnancyProxy(femBot, manBot, dnaF, dnaM);
                    pregnancy.PreggersAlarm = femBot.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(pregnancy.HourlyCallback), 1f, TimeUnit.Hours,
                        "Hourly Robot Surrogate Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
                    femBot.SimDescription.Pregnancy = pregnancy;
                    EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kGotPregnant, femBot, (manBot != null) ? manBot.CreatedSim : null, pregnancy, null));
                    (femBot.SimDescription.Pregnancy as RobotPregnancyProxy).ApplyInitialMutationFactors();

                    return femBot.SimDescription.Pregnancy;
                }
                else
                {
                    Common.DebugNotify("RobotPregnancyProxy.StartInternal" + Common.NewLine + " - How did you get here?!");
                    return null;
                }
            }
        }
    }
}
