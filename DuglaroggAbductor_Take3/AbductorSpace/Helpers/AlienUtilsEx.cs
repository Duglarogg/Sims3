//using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Passport;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Helpers
{
    public class AlienUtilsEx
    {
        public static BuffNames sBabyIsComing = unchecked((BuffNames)ResourceUtils.HashString64("DuglaroggBabyIsComing"));
        public static BuffNames sXenogenesis = unchecked((BuffNames)ResourceUtils.HashString64("DuglaroggXenogenesis"));

        public static TraitNames sAlienChild = unchecked((TraitNames)ResourceUtils.HashString64("DuglaroggAlienChild"));
        public static TraitNames sAlienPregnancy = unchecked((TraitNames)ResourceUtils.HashString64("DuglaroggAlienPregnancy"));
        public static TraitNames sAlreadyExamined = unchecked((TraitNames)ResourceUtils.HashString24("DuglaroggAlreadyExamined"));

        static AlarmTimerCallback sActivityCallback = new AlarmTimerCallback(ActivityCallback);
        static AlarmTimerCallback sRefreshCallback = new AlarmTimerCallback(RefreshCallback);

        static void ActivityCallback()
        {
            Logger.Append("Alien Activity Alarm Triggered");

            if (Household.AlienHousehold == null || Household.AlienHousehold.NumMembers == 0)
            {
                Logger.Append("Alien Activity: Alien Household Null or Empty");
                return;
            }

            float chance = GetActivityChance();

            if (RandomUtil.RandomChance(chance))
            {
                Logger.Append("Alien Activity: Chance Passed " + chance);

                List<SimDescription> aliens = GetAliens();

                if (aliens == null)
                {
                    Logger.Append("Alien Activity: No valid aliens.");
                    return;
                }

                SimDescription alien = RandomUtil.GetRandomObjectFromList<SimDescription>(aliens);

                chance = GetAbductionChance();

                if (RandomUtil.RandomChance(chance))
                {
                    Logger.Append("Alien Abduction: Chance Passed " + chance);

                    chance = GetAbductionChance(true);

                    Lot targetLot;
                    Sim abductee;

                    if (IsActiveTarget(chance))
                    {
                        Logger.Append("Alien Abduction (Active): Chance Passed " + chance);

                        List<Sim> abductees = GetAbductees(Household.ActiveHousehold);

                        if (abductees == null)
                        {
                            Logger.Append("Alien Abduction (Active): No Valid Abductees");
                            ResetAbductionHelper();
                            return;
                        }

                        abductee = RandomUtil.GetRandomObjectFromList<Sim>(abductees);

                        if (!CanSimBeAbducted(abductee))
                        {
                            Logger.Append("Alien Abduction (Active): Can't Abduct Target");
                            ResetAbductionHelper();
                            return;
                        }

                        targetLot = abductee.LotCurrent;
                    }
                    else
                    {
                        Logger.Append("Alien Abduction (Active): Chance Failed " + chance);

                        List<Lot> lots = GetLots();

                        if (lots == null)
                        {
                            Logger.Append("Alien Abduction (Non-Active): No Valid Lots");
                            ResetAbductionHelper();
                            return;
                        }

                        targetLot = RandomUtil.GetRandomObjectFromList<Lot>(lots);

                        List<Sim> abductees = GetAbductees(targetLot);

                        if (abductees == null)
                        {
                            Logger.Append("Alien Abduction (Non-Active): No Valid Abductees");
                            ResetAbductionHelper();
                            return;
                        }

                        abductee = RandomUtil.GetRandomObjectFromList<Sim>(abductees);

                        if (!CanSimBeAbducted(abductee))
                        {
                            Logger.Append("Alien Abduction (Non-Active): Can't Abduct Target");
                            ResetAbductionHelper();
                            return;
                        }
                    }

                    AlienAbductionSituationEx.Create(alien, abductee, targetLot);
                }
                else
                {
                    Logger.Append("Alien Abduction: Chance Failed " + chance);

                    chance = GetVisitChance();

                    if (RandomUtil.RandomChance(chance))
                    {
                        Logger.Append("Alien Visit: Chance Passed " + chance);

                        chance = GetVisitChance(true, alien);

                        Lot farthestLot, targetLot;
                        Sim visitor;

                        if (IsActiveTarget(chance))
                        {
                            Logger.Append("Alien Visit (Active): Chance Passed " + chance);

                            targetLot = Household.ActiveHouseholdLot;
                            farthestLot = LotManager.GetFarthestLot(targetLot);
                            visitor = alien.InstantiateOffScreen(farthestLot);
                        }
                        else
                        {
                            Logger.Append("Alien Visit (Active): Chance Failed " + chance);

                            List<Lot> lots = GetLots();

                            if (lots == null)
                            {
                                Logger.Append("Alien Visit (Non-Active): No Valid Lots");
                                ResetAbductionHelper();
                                return;
                            }

                            targetLot = RandomUtil.GetRandomObjectFromList<Lot>(lots);
                            farthestLot = LotManager.GetFarthestLot(targetLot);
                            visitor = alien.InstantiateOffScreen(farthestLot);
                        }

                        AlienSituation.Create(visitor, targetLot);
                    }
                    else
                    {
                        Logger.Append("Alien Visit: Chance Failed " + chance);
                    }
                }
            }
            else
            {
                Logger.Append("Alien Activity: Chance Failed " + chance);
            }

            ResetAbductionHelper();
        }

        static bool CanGetPregnant(Sim sim)
        {
            if (IsPassporter(sim.SimDescription))
            {
                return false;
            }

            int numHumans, numPets;
            sim.SimDescription.Household.GetNumberOfSimsAndPets(true, out numHumans, out numPets);

            if (!Household.CanSpeciesGetAddedToHousehold(sim.SimDescription.Species, numHumans, numPets))
            {
                return false;
            }

            if (IsSkinJob(sim.SimDescription) || sim.BuffManager.HasTransformBuff() || sim.SimDescription.IsPregnant || sim.SimDescription.IsVisuallyPregnant)
            {
                return false;
            }

            if ((sim.Household != null && sim.Household.IsTouristHousehold) || sim.LotHome == null || IsLampGenie(sim.SimDescription))
            {
                return false;
            }
            else if (sim.SimDescription.IsDueToAgeUp() || (sim.SimDescription.AgingState != null && sim.SimDescription.AgingState.IsAgingInProgress()))
            {
                return false;
            }

            return true;
        }

        public static bool CanSimBeAbducted(Sim sim)
        {
            bool flag = false;
            int num = 0;

            foreach (Sim current in sim.LotCurrent.GetSims())
            {
                if (current.IsHuman)
                {
                    if (current.SimDescription.ToddlerOrBelow)
                    {
                        flag = true;
                    }
                    else if (current.SimDescription.TeenOrAbove)
                    {
                        num++;
                    }
                }
            }

            return !flag || (num >= 2);
        }

        static bool CheckAlarm(AlarmHandle handle, AlarmTimerCallback callback)
        {
            if (handle != AlarmHandle.kInvalidHandle)
            {
                List<AlarmManager.Timer> list = AlarmManager.Global.mTimers[handle];

                if (list != null && list.Count > 0)
                {
                    foreach (AlarmManager.Timer current in list)
                    {
                        if (current.CallBack == callback)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        static List<Sim> GetAbductees(Household household)
        {
            if (household != null && household.NumMembers > 0)
            {
                List<Sim> list = new List<Sim>();

                foreach (Sim current in household.mMembers.ActorList)
                {
                    if (current.IsHuman && current.SimDescription.TeenOrAbove && current.LotCurrent != null
                        && !AlienUtils.IsHouseboatAndNotDocked(current.LotCurrent))
                    {
                        list.Add(current);
                    }
                }

                return (list.Count > 0) ? list : null;
            }

            return null;
        }

        public static List<Sim> GetAbductees(Lot lot)
        {
            if (lot != null && !AlienUtils.IsHouseboatAndNotDocked(lot))
            {
                List<Sim> list = new List<Sim>();

                foreach (Sim current in lot.GetSims())
                {
                    if (current.IsHuman && current.SimDescription.TeenOrAbove)
                    {
                        list.Add(current);
                    }
                }

                return (list.Count > 0) ? list : null;
            }

            return null;
        }

        static float GetAbductionChance(bool isActive = false)
        {
            float result = isActive ? Abductor.Settings.mActiveAbductionChance : Abductor.Settings.mBaseAbductionChance;

            if (result <= 0)
            {
                Logger.Append("Alien Abduction: Disabled");                
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
            {
                result += Abductor.Settings.mTelescopeUsedBonus;
            }

            if (AlienUtils.sAlienAbductionHelper.SpaceRocksFound > 0)
            {
                result += Math.Min(Abductor.Settings.mSpaceRockBonus * AlienUtils.sAlienAbductionHelper.SpaceRocksFound, Abductor.Settings.mMaxSpaceRockBonus);
            }

            return result;
        }

        static float GetActivityChance()
        {
            float result = Abductor.Settings.mBaseActivityChance;

            if (result <= 0)
            {
                Logger.Append("Alien Activity: Disabled");
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
            {
                result += Abductor.Settings.mTelescopeUsedBonus;
            }

            return result;
        }

        static int GetActivityHour()
        {
            return (Abductor.Settings.mHourToStartActivity + RandomUtil.GetInt(0, Abductor.Settings.mHoursOfActivity));
        }

        public static List<SimDescription> GetAliens(bool forActivity = true)
        {
            if (Household.AlienHousehold != null && Household.AlienHousehold.NumMembers > 0)
            {
                List<SimDescription> list = new List<SimDescription>();

                foreach (SimDescription current in Household.AlienHousehold.SimDescriptions)
                {
                    if (current.CreatedSim == null || !forActivity)
                    {
                        list.Add(current);
                    }
                }

                return (list.Count > 0) ? list : null;
            }

            return null;
        }

        static float GetImpregnationChance(Sim abductee, SimDescription alien)
        {
            float result = Abductor.Settings.mImpregnantionChance;

            if (result <= 0)
            {
                Logger.Append("Alien Pregnancy: Disabled");
                return 0;
            }

            if (Abductor.Settings.mUseFertility)
            {
                if ((abductee.BuffManager != null && abductee.BuffManager.HasElement(BuffNames.ATwinkleInTheEye)) 
                    || abductee.TraitManager.HasElement(TraitNames.FertilityTreatment))
                {
                    result += TraitTuning.kFertilityBabyMakingChanceIncrease;
                }

                if (abductee.BuffManager != null && abductee.BuffManager.HasElement(BuffNames.MagicInTheAir))
                {
                    result += BuffMagicInTheAir.kBabyMakingChanceIncrease;
                }

                if (abductee.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    result += 100f;
                    abductee.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                }

                if ((alien.CreatedSim != null && alien.CreatedSim.BuffManager != null && alien.CreatedSim.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                    || alien.TraitManager.HasElement(TraitNames.FertilityTreatment))
                {
                    result += TraitTuning.kFertilityBabyMakingChanceIncrease;
                }

                if (alien.CreatedSim != null && alien.CreatedSim.BuffManager != null && alien.CreatedSim.BuffManager.HasElement(BuffNames.MagicInTheAir))
                {
                    result += BuffMagicInTheAir.kBabyMakingChanceIncrease;
                }

                if (alien.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    result += 100f;

                    if (alien.CreatedSim != null)
                    {
                        alien.CreatedSim.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                    }
                }

                if (GameUtils.IsInstalled(ProductVersion.EP7) && SimClock.IsNightTime() && SimClock.IsFullMoon())
                {
                    result += Pregnancy.kFullMoonImprovedBabyChance * 100f;
                }
            }

            return result;
        }

        static List<Lot> GetLots()
        {
            List<Lot> list = new List<Lot>();

            foreach (Lot current in LotManager.AllLotsWithoutCommonExceptions)
            {
                if (current.LotType != LotType.Tutorial && current != Household.ActiveHouseholdLot)
                {
                    list.Add(current);
                }
            }

            return (list.Count > 0) ? list : null;
        }

        static float GetVisitChance(bool isActive = false, SimDescription alien = null)
        {
            float result = isActive ? Abductor.Settings.mActiveVisitChance : Abductor.Settings.mBaseVisitChance;

            if (result <= 0)
            {
                Logger.Append("Alien Visit: Disabled");
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
            {
                result += Abductor.Settings.mTelescopeUsedBonus;
            }

            if (AlienUtils.sAlienAbductionHelper.SpaceRocksFound > 0)
            {
                result += Math.Min(Abductor.Settings.mSpaceRockBonus * AlienUtils.sAlienAbductionHelper.SpaceRocksFound, Abductor.Settings.mMaxSpaceRockBonus);
            }

            if (isActive && Household.ActiveHousehold != null && Household.ActiveHouseholdLot != null)
            {
                foreach (SimDescription current in Household.ActiveHousehold.SimDescriptions)
                {
                    Relationship relationship = Relationship.Get(alien, current, false);

                    if (relationship != null && relationship.LTR.Liking >= Abductor.Settings.mHighLTRThreshold)
                    {
                        result += Abductor.Settings.mHighLTRBonus;
                    }

                    if (current.Genealogy.Parents.Contains(alien.Genealogy))
                    {
                        result += Abductor.Settings.mOffspringBonus;
                    }
                }

                int count = Household.ActiveHouseholdLot.GetObjects<RockGemMetalBase>(new Predicate<RockGemMetalBase>(AlienUtils.IsSpaceRock)).Count;

                if (count >= Abductor.Settings.mSpaceRockThreshold)
                {
                    result += Abductor.Settings.mSpaceRockVisitBonus;
                }
            }

            return result;
        }

        static bool IsActiveTarget(float chance)
        {
            if (Household.ActiveHousehold == null)
            {
                return false;
            }

            return RandomUtil.RandomChance(chance);
        }

        public static bool IsImpregnationSuccessful(Sim abductee, Sim alien)
        {
            if (!CanGetPregnant(abductee))
            {
                Logger.Append("Alien Pregnancy: Abductee Can't Get Pregnant");
                return false;
            }

            float chance = GetImpregnationChance(abductee, alien.SimDescription);

            if (!RandomUtil.RandomChance(chance))
            {
                Logger.Append("Alien Pregnancy: Impregnation Chance Failed " + chance);
                return false;
            }

            Logger.Append("Alien Pregnancy: Impregnation Chance Passed " + chance);
            return true;
        }

        public static bool IsLampGenie(SimDescription sim)
        {
            if (!sim.IsGenie) return false;

            OccultGenie occultType = sim.OccultManager.GetOccultType(OccultTypes.Genie) as OccultGenie;

            if (occultType == null) return false;

            return occultType.IsTiedToLamp;
        }

        public static bool IsPassporter(SimDescription sim)
        {
            if (sim == null) return false;

            if (sim.HasFlags(SimDescription.FlagField.IsTravelingForPassport)) return true;

            if (sim.HasFlags(SimDescription.FlagField.IsAwayForPassport)) return true;

            if (Passport.Instance != null)
            {
                if (Passport.Instance.IsHostedSim(sim)) return true;
            }

            if (Passport.IsPassportSim(sim)) return true;

            return false;
        }

        public static bool IsSkinJob(SimDescription sim)
        {
            if (sim.CreatedByService != null && sim.CreatedByService.ServiceType == ServiceType.GrimReaper)
            {
                return true;
            }
            else if (sim.IsMummy)
            {
                return true;
            }
            else if (sim.IsRobot)
            {
                return true;
            }
            else if (sim.IsBonehilda)
            {
                return true;
            }

            return false;
        }

        public static void OnWorldLoadFinished(object sender, EventArgs evtArgs)
        {
            if (GameUtils.GetCurrentWorldType() != WorldType.Vacation)
            {
                AlienUtils.kAlienHouseholdValidAges = new CASAgeGenderFlags[]
                {
                    CASAgeGenderFlags.Teen,
                    CASAgeGenderFlags.YoungAdult,
                    CASAgeGenderFlags.Adult,
                    CASAgeGenderFlags.Elder
                };

                if (AlienUtils.sAlienVisitationAlarm != AlarmHandle.kInvalidHandle && !CheckAlarm(AlienUtils.sAlienVisitationAlarm, sActivityCallback))
                {
                    AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienVisitationAlarm);
                    AlienUtils.sAlienVisitationAlarm = AlarmHandle.kInvalidHandle;
                }

                AlienUtils.sAlienVisitationAlarm = AlarmManager.Global.AddAlarmDay(GetActivityHour(), DaysOfTheWeek.All, new AlarmTimerCallback(ActivityCallback),
                    "Alien Activity Ex Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);

                if (AlienUtils.sAlienHouseholdRefreshAlarm != AlarmHandle.kInvalidHandle && !CheckAlarm(AlienUtils.sAlienHouseholdRefreshAlarm, sRefreshCallback))
                {
                    AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienHouseholdRefreshAlarm);
                    AlienUtils.sAlienHouseholdRefreshAlarm = AlarmHandle.kInvalidHandle;
                }

                AlienUtils.sAlienHouseholdRefreshAlarm = AlarmManager.Global.AddAlarmDay(15f, DaysOfTheWeek.All, new AlarmTimerCallback(RefreshCallback),
                    "Alien Household Refresh Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);
            }
        }

        public static void OnWorldQuit(object sender, EventArgs evtArgs)
        {
            AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienVisitationAlarm);
            AlienUtils.sAlienVisitationAlarm = AlarmHandle.kInvalidHandle;

            AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienHouseholdRefreshAlarm);
            AlienUtils.sAlienHouseholdRefreshAlarm = AlarmHandle.kInvalidHandle;
        }

        static void RefreshCallback()
        {
            Logger.Append("Alien Household Refresh Alarm Triggered");

            if (Household.AlienHousehold == null)
            {
                Logger.Append("Alien Refresh: Alien Household is Null");
                return;
            }

            if (Household.AlienHousehold.NumMembers < AlienUtils.kAlienHouseholdNumMembers)
            {
                CASAgeGenderFlags age = RandomUtil.GetRandomObjectFromList<CASAgeGenderFlags>(AlienUtils.kAlienHouseholdValidAges);
                CASAgeGenderFlags gender = RandomUtil.CoinFlip() ? CASAgeGenderFlags.Male : CASAgeGenderFlags.Female;
                SimDescription description = AlienGenetics.MakeAlien(age, gender, GameUtils.GetCurrentWorld(), 1f, true);
                description.SkillManager.AddElement(SkillNames.Logic);
                description.SkillManager.AddElement(SkillNames.Handiness);

                Skill element = description.SkillManager.GetElement(SkillNames.Logic);

                if (element != null)
                {
                    element.ForceSkillLevelUp(RandomUtil.GetInt(Abductor.Settings.mLogicSkill[0], Abductor.Settings.mLogicSkill[1]));
                }

                element = description.SkillManager.GetElement(SkillNames.Handiness);

                if (element != null)
                {
                    element.ForceSkillLevelUp(RandomUtil.GetInt(Abductor.Settings.mHandinessSkill[0], Abductor.Settings.mHandinessSkill[1]));
                }

                if (age == CASAgeGenderFlags.Teen)
                {
                    description.SkillManager.AddElement(SkillNames.LearnToDrive);

                    element = description.SkillManager.GetElement(SkillNames.LearnToDrive);

                    if (element != null)
                    {
                        element.ForceSkillLevelUp(SkillManager.GetMaximumSupportedSkillLevel(SkillNames.LearnToDrive));
                    }
                }

                if (GameUtils.IsInstalled(ProductVersion.EP11) && Abductor.Settings.mFutureSim)
                {
                    description.TraitManager.AddHiddenElement(TraitNames.FutureSim);
                    description.SkillManager.AddElement(SkillNames.Future);

                    element = description.SkillManager.GetElement(SkillNames.Future);

                    if (element != null)
                    {
                        element.ForceSkillLevelUp(RandomUtil.GetInt(Abductor.Settings.mAdvancedTechSkill[0], Abductor.Settings.mAdvancedTechSkill[1]));
                    }
                }

                Household.AlienHousehold.AddSilent(description);
                description.OnHouseholdChanged(Household.AlienHousehold, false);
            }
        }

        static void ResetAbductionHelper()
        {
            AlienUtils.sAlienAbductionHelper.SpaceRocksFound = 0;
            AlienUtils.sAlienAbductionHelper.TelescopeUsed = false;
        }
    }
}
