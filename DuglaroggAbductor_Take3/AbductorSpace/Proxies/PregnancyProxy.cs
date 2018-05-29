using Duglarogg.AbductorSpace.Helpers;
using Duglarogg.AbductorSpace.Interactions;
//using NRaas;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Tutorial;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;


namespace Duglarogg.AbductorSpace.Proxies
{
    public class PregnancyProxy : Pregnancy
    {
        //static MethodStore sWoohooerGetChanceOfQuads = new MethodStore("NRaasWoohooer", "NRaas.Woohooer", "GetChanceOfQuads", null);

        static BuffNames sItsQuadruplets = unchecked((BuffNames)ResourceUtils.HashString64("NRaasItsQuadruplets"));

        public PregnancyProxy(Pregnancy src)
        {
            CopyPregnancy(this, src);
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

        public override List<Sim> CreateNewborns(float bonusMoodPoints, bool interactive, bool homeBirth)
        {
            SimDescription alien = null;
            MiniSimDescription miniAlien = null;

            if (mDad != null && !mDad.HasBeenDestroyed)
            {
                alien = mDad.SimDescription;
            }

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
                    }
                }
            }

            float averageMoodForBirth = GetAverageMoodForBirth(alien, bonusMoodPoints);
            Random pregoRandom = new Random(mRandomGenSeed);
            int numSimMembers = 0;
            int numPetMembers = 0;
            mMom.Household.GetNumberOfSimsAndPets(true, out numSimMembers, out numPetMembers);
            int numForBirth = GetNumForBirth(alien, pregoRandom, numSimMembers, numPetMembers);
            Random gen = new Random(mRandomGenSeed);
            List<Sim> list = new List<Sim>();

            for (int i = 0; i < numForBirth; i++)
            {
                DetermineGenderOfBaby(gen);
                CASAgeGenderFlags gender = mGender;
                mGender = CASAgeGenderFlags.None;
                SimDescription babyDescription = AlienGenetics.MakeAlienBaby(alien, mMom.SimDescription, gender, averageMoodForBirth, pregoRandom, interactive);
                mMom.Household.Add(babyDescription);
                Sim baby = babyDescription.Instantiate(Vector3.Empty);
                baby.SetPosition(mMom.Position);

                if (homeBirth)
                {
                    TotallyHideBaby(baby);
                }

                list.Add(baby);
                CheckForGhostBaby(baby);

                if (baby.SimDescription.IsPlayableGhost)
                {
                    EventTracker.SendEvent(EventTypeId.kHadGhostBaby, mMom, baby);
                }

                if (i == 0)
                {
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBaby, baby.SimDescription));
                }

                MidlifeCrisisManager.OnHadChild(mMom.SimDescription);
                EventTracker.SendEvent(EventTypeId.kNewOffspring, mMom, baby);
                //EventTracker.SendEvent(EventTypeId.kParentAdded, baby, mMom);

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
            {
                mMom.Household.InvalidateThumbnail();
            }

            switch (numForBirth)
            {
                case 1:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabySingle, mMom.SimDescription));
                    break;

                case 2:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTwins, mMom.SimDescription));
                    break;

                case 3:
                    EventTracker.SendEvent(new SimDescriptionEvent(EventTypeId.kNewBabyTriplets, mMom.SimDescription));
                    break;

                case 4:
                    break;
            }

            return list;
        }

        public new float GetAverageMoodForBirth(SimDescription dadDescription, float bonusMoodPoints)
        {
            mMom.MoodManager.MoodChanged -= new MoodManager.MoodChangedCallback(MoodManager_MoodChanged);
            MoodManager_MoodChanged();
            float num = mPregnancyScore / 4320f;

            if (dadDescription != null)
            {
                num += kPregnancyBookBonusDad * 2f;
            }

            num += kPregnancyBookBonusMom * NumPregnancyBooksRead(mMom.SimDescription);
            num += mDoctorAdviceGivenBonus;

            return num + bonusMoodPoints;
        }

        public override int GetNumForBirth(SimDescription dadDescription, Random pregoRandom, int numSimMembers, int numPetMembers)
        {
            try
            {
                int desiredNumChildren = 1;
                int max = 3;
                //int max = sWoohooerGetChanceOfQuads.Valid ? 4 : 3;

                if (mMom.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    mMom.TraitManager.RemoveElement(TraitNames.WishedForLargeFamily);
                    desiredNumChildren = max;
                }

                if ((dadDescription != null) && (dadDescription.TraitManager.HasElement(TraitNames.WishedForLargeFamily)))
                {
                    dadDescription.TraitManager.RemoveElement(TraitNames.WishedForLargeFamily);
                    desiredNumChildren = max;
                }

                if (desiredNumChildren != max)
                {
                    mMultipleBabiesMultiplier = Math.Min(mMultipleBabiesMultiplier, kMaxBabyMultiplier);

                    if (mMom.HasTrait(TraitNames.FertilityTreatment))
                    {
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    }
                    else if (mMom.BuffManager != null && mMom.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                    {
                        mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                    }

                    if (dadDescription != null)
                    {
                        if (dadDescription.HasTrait(TraitNames.FertilityTreatment))
                        {
                            mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                        }
                        else if (dadDescription.CreatedSim != null && dadDescription.CreatedSim.BuffManager != null && dadDescription.CreatedSim.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                        {
                            mMultipleBabiesMultiplier *= TraitTuning.kFertilityMultipleBabiesMultiplier;
                        }
                    }

                    double num2 = pregoRandom.NextDouble();

                    if (num2 < (kChanceOfTwins * mMultipleBabiesMultiplier))
                    {
                        desiredNumChildren++;

                        if (num2 < (kChanceOfTriplets * mMultipleBabiesMultiplier))
                        {
                            desiredNumChildren++;
                            
                            /*
                            if (sWoohooerGetChanceOfQuads.Valid && (num2 < (sWoohooerGetChanceOfQuads.Invoke<float>(null) * mMultipleBabiesMultiplier)))
                            {
                                desiredNumChildren++;
                            }
                            */
                        }
                    }
                }

                return desiredNumChildren;
            }
            catch (Exception e)
            {
                Logger.WriteExceptionLog(e, this, "Duglarogg.AbductorSpace.Proxies.PregnancyProxy.GetNumForBirth()");
                return 1;
            }
        }

        public override void PregnancyComplete(List<Sim> newborns, List<Sim> followers)
        {
            if (mMom.IsSelectable)
            {
                Audio.StartObjectSound(mMom.ObjectId, "sting_baby_born_alien", false);
            }

            Tutorialette.TriggerLesson(Lessons.Babies, mMom);
            EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kHadBaby, mMom, mDad, this, newborns));

            mMom.BuffManager.RemoveElement(AlienUtilsEx.sXenogenesis);
            mMom.BuffManager.RemoveElement(AlienUtilsEx.sBabyIsComing);
            mMom.RemoveInteractionByType(TakeToHospitalEx.Singleton);
            UnrequestPregnantWalkStyle();

            if (mMom.TraitManager.HasElement(AlienUtilsEx.sAlreadyExamined))
            {
                mMom.TraitManager.RemoveElement(AlienUtilsEx.sAlreadyExamined);
            }

            if (!mMom.SimDescription.IsVampire)
            {
                Motive motive = mMom.Motives.GetMotive(CommodityKind.Hunger);

                if (motive != null)
                {
                    motive.PregnantMotiveDecay = false;
                }
            }

            Motive motive2 = mMom.Motives.GetMotive(CommodityKind.Bladder);

            if (motive2 != null)
            {
                motive2.PregnantMotiveDecay = false;
            }

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
                    buffName = sItsQuadruplets;
                    break;
            }

            mMom.BuffManager.AddElement(buffName, Origin.FromNewBaby);

            if (mMom.TraitManager.HasElement(TraitNames.Dramatic))
            {
                mMom.PlayReaction(ReactionTypes.JoyousCrying, ReactionSpeed.NowOrLater);
            }

            if (followers != null)
            {
                foreach (Sim current in followers)
                {
                    if (current.SimDescription.ToddlerOrAbove)
                    {
                        current.BuffManager.AddElement(buffName, Origin.FromNewBaby);
                    }
                }
            }

            bool flag = !mMom.Household.IsActive;

            foreach (Sim current2 in newborns)
            {
                if (flag)
                {
                    current2.SimDescription.AgingState.AgeUpWithinNDays(kMaxDaysAsNPCBaby);
                }

                current2.SimDescription.PushAgingEnabledToAgingManager();
            }

            mMom.SimDescription.NullOutPregnancy();
        }
    }
}
