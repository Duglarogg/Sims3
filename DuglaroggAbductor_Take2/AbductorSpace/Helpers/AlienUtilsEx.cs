using Duglarogg.CommonSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Helpers
{
    public class AlienUtilsEx
    {
        static CASAgeGenderFlags[] kValidAges = new CASAgeGenderFlags[]
        {
            CASAgeGenderFlags.Teen,
            CASAgeGenderFlags.YoungAdult,
            CASAgeGenderFlags.Adult,
            CASAgeGenderFlags.Elder
        };

        static AlarmTimerCallback kActivityCallback = new AlarmTimerCallback(AlienActivityCallback);
        static AlarmTimerCallback kRefreshCallback = new AlarmTimerCallback(AlienRefreshCallback);

        static AlienUtilsEx()
        { }

        static void AlienActivityCallback()
        {
            if (Abductor.Settings.mDebugging)
            {
                StyledNotification.Format format = new StyledNotification.Format("Alien Visitation Alarm triggered!",
                    StyledNotification.NotificationStyle.kDebugAlert);
                StyledNotification.Show(format);
            }

            if (Household.AlienHousehold == null || Household.AlienHousehold.NumMembers == 0)
            {
                Logger.Append("Alien Activity Alarm: Alien Household Null or Empty");
                return;
            }

            float chance = GetActivityChance();

            if (!RandomUtil.RandomChance(chance))
            {
                Logger.Append("Alien Activity Alarm: Chance Fail " + chance);
                ResetAbductionHelper();
                return;
            }

            List<SimDescription> aliens = GetAliens();

            if (aliens == null)
            {
                Logger.Append("Alien Activity Alarm: No valid aliens");
                ResetAbductionHelper();
                return;
            }

            SimDescription alien = RandomUtil.GetRandomObjectFromList<SimDescription>(aliens);

            chance = GetAbductionChance();

            if (RandomUtil.RandomChance(chance))
            {
                chance = GetAbductionChance(true);

                Lot target;
                Sim abductee;

                if (IsActiveTarget(chance))
                {
                    List<Sim> abductees = GetAbductees(Household.ActiveHousehold);

                    if (abductees == null)
                    {
                        Logger.Append("Alien Abduction: No abductees");
                        ResetAbductionHelper();
                        return;
                    }

                    abductee = RandomUtil.GetRandomObjectFromList<Sim>(abductees);
                    target = abductee.LotCurrent;
                }
                else
                {
                    List<Lot> lots = GetLots();

                    if (lots == null)
                    {
                        Logger.Append("Alien Abduction: No lots");
                        ResetAbductionHelper();
                        return;
                    }

                    target = RandomUtil.GetRandomObjectFromList<Lot>(lots);
                    List<Sim> abductees = GetAbductees(target);

                    if (abductees == null)
                    {
                        Logger.Append("Alien Abduction: No abductess");
                        ResetAbductionHelper();
                        return;
                    }

                    abductee = RandomUtil.GetRandomObjectFromList<Sim>(abductees);
                }

                AlienAbductionSituationEx.Create(alien, abductee, target);
            }
            else
            {
                Logger.Append("Alien Abduction: Chance Fail " + chance);

                chance = GetVisitChance();

                if (RandomUtil.RandomChance(chance))
                {
                    chance = GetVisitChance(true, alien);

                    Lot farthestLot, target;
                    Sim visitor;

                    if (IsActiveTarget(chance))
                    {
                        farthestLot = LotManager.GetFarthestLot(Household.ActiveHouseholdLot);
                        target = Household.ActiveHouseholdLot;
                        visitor = alien.InstantiateOffScreen(farthestLot);
                        //AlienSituation.Create(visitor, Household.ActiveHouseholdLot);
                    }
                    else
                    {
                        List<Lot> lots = GetLots();

                        if (lots == null)
                        {
                            Logger.Append("Alien Visit: No lots");
                            ResetAbductionHelper();
                            return;
                        }

                        target = RandomUtil.GetRandomObjectFromList<Lot>(lots);
                        farthestLot = LotManager.GetFarthestLot(target);
                        visitor = alien.InstantiateOffScreen(farthestLot);
                    }

                    AlienSituation.Create(visitor, target);
                }
                else
                {
                    Logger.Append("Alien Visit: Chance Fail " + chance);
                }
            }

            ResetAbductionHelper();
        }

        static void AlienRefreshCallback()
        {
            if (Abductor.Settings.mDebugging)
            {
                StyledNotification.Format format = new StyledNotification.Format("Alien Household Refresh Alarm triggered!",
                    StyledNotification.NotificationStyle.kDebugAlert);
                StyledNotification.Show(format);
            }

            if (Household.AlienHousehold == null)
            {
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
                        element.ForceSkillLevelUp(RandomUtil.GetInt(Abductor.Settings.mAdvancedTechnologySkill[0], Abductor.Settings.mAdvancedTechnologySkill[1]));
                    }
                }

                Household.AlienHousehold.AddSilent(description);
                description.OnHouseholdChanged(Household.AlienHousehold, false);
            }
        }

        static bool CanASimBeAbducted(Household household)
        {
            if (AlienUtils.sAlienAbductionHelper == null)
            {
                return false;
            }

            int num = 0;

            foreach(Sim current in household.mMembers.ActorList)
            {
                if (current.IsHuman && current.SimDescription.TeenOrAbove
                    && !AlienUtils.IsHouseboatAndNotDocked(current.LotCurrent)
                    && CanASimBeAbducted(current.LotCurrent))
                {
                    num++;
                }
            }

            return num > 0;
        }

        static bool CanASimBeAbducted(Lot lot)
        {
            if (AlienUtils.sAlienAbductionHelper == null || AlienUtils.IsHouseboatAndNotDocked(lot) || lot.GetSimsCount() == 0)
            {
                return false;
            }

            bool flag = true;
            int num = 0;

            foreach(Sim current in lot.GetSims())
            {
                if (current.IsHuman)
                {
                    if (current.SimDescription.ToddlerOrBelow)
                    {
                        flag = false;
                    }
                    else if (current.SimDescription.TeenOrAbove)
                    {
                        num++;
                    }
                }
            }

            return (num > 0) && (flag || num >= 2);
        }

        public static bool CheckAlarm(AlarmHandle handle, AlarmTimerCallback callback)
        {
            List<AlarmManager.Timer> list = AlarmManager.Global.mTimers[handle];

            if (list == null || list.Count == 0)
            {
                return false;
            }

            foreach(AlarmManager.Timer current in list)
            {
                if (current.CallBack == callback)
                {
                    return true;
                }
            }

            return false;
        }

        static List<Sim> GetAbductees(Household household)
        {
            List<Sim> list = new List<Sim>();

            foreach(Sim current in household.mMembers.ActorList)
            {
                if (current.IsHuman && current.SimDescription.TeenOrAbove 
                    && current.LotCurrent != null && !AlienUtils.IsHouseboatAndNotDocked(current.LotCurrent))
                {
                    list.Add(current);
                }
            }

            if (list.Count == 0)
            {
                return null;
            }

            return list;
        }

        public static List<Sim> GetAbductees(Lot lot)
        {
            if (AlienUtils.IsHouseboatAndNotDocked(lot))
            {
                return null;
            }

            List<Sim> list = new List<Sim>();
            bool flag = true;

            foreach (Sim current in lot.GetSims())
            {
                if (current.IsHuman)
                {
                    if(current.SimDescription.ToddlerOrBelow)
                    {
                        flag = false;
                    }
                    else if (current.SimDescription.TeenOrAbove)
                    {
                        list.Add(current);
                    }
                }
            }

            if (list.Count == 0 || (!flag && list.Count < 2))
            {
                return null;
            }

            return list;
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
                int bonus = Abductor.Settings.mSpaceRockFoundBonus * AlienUtils.sAlienAbductionHelper.SpaceRocksFound;
                result += Math.Min(bonus, Abductor.Settings.mMaxSpaceRockFoundBonus);
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

        public static List<SimDescription> GetAliens(bool forPregnancy = false)
        {
            if (Household.AlienHousehold == null || Household.AlienHousehold.NumMembers == 0)
            {
                return null;
            }

            List<SimDescription> list = new List<SimDescription>();

            foreach(SimDescription current in Household.AlienHousehold.SimDescriptions)
            {
                if (forPregnancy || current.CreatedSim == null)
                {
                    list.Add(current);
                }
            }

            if (list.Count == 0)
            {
                return null;
            }

            return list;
        }

        static List<Lot> GetLots()
        {
            List<Lot> list = new List<Lot>();

            foreach(Lot current in LotManager.AllLotsWithoutCommonExceptions)
            {
                if (current.LotType != LotType.Tutorial && current != Household.ActiveHouseholdLot)
                {
                    list.Add(current);
                }
            }

            if (list.Count == 0)
            {
                return null;
            }

            return list;
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
                int bonus = Abductor.Settings.mSpaceRockFoundBonus * AlienUtils.sAlienAbductionHelper.SpaceRocksFound;
                result += Math.Min(bonus, Abductor.Settings.mMaxSpaceRockFoundBonus);
            }

            if (isActive && Household.ActiveHousehold != null)
            {
                bool flag = false;

                foreach (SimDescription current in Household.ActiveHousehold.SimDescriptions)
                {
                    Relationship relationship = Relationship.Get(alien, current, false);

                    if (!flag && relationship != null && relationship.LTR.Liking >= AlienUtils.kAlienHouseholdHighLTRThreshold)
                    {
                        result += AlienUtils.kAlienHouseholdVisitChanceActiveHighLTRMod;
                        flag = true;
                    }

                    if (current.Genealogy.Parents.Contains(alien.Genealogy))
                    {
                        result += AlienUtils.kAlienHouseholdVisitChanceOffspring;
                    }
                }

                int count = Household.ActiveHouseholdLot.GetObjects<RockGemMetalBase>(new Predicate<RockGemMetalBase>(AlienUtils.IsSpaceRock)).Count;

                if (count >= AlienUtils.kAlienHouseholdSpaceRocksForBonus)
                {
                    result += AlienUtils.kAlienHouseholdVisitChanceActiveSpaceRockMod;
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

        public static void OnWorldLoadFinished(object sender, EventArgs evtArgs)
        {
            Startup();
        }

        public static void OnWorldQuit(object sender, EventArgs evtArgs)
        {
            Shutdown();
        }

        static void ResetAbductionHelper()
        {
            AlienUtils.sAlienAbductionHelper.SpaceRocksFound = 0;
            AlienUtils.sAlienAbductionHelper.TelescopeUsed = false;
        }

        public static void SetActivityAlarm()
        {
            if (AlienUtils.sAlienVisitationAlarm != AlarmHandle.kInvalidHandle && !CheckAlarm(AlienUtils.sAlienVisitationAlarm, kActivityCallback))
            {
                AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienVisitationAlarm);
                AlienUtils.sAlienVisitationAlarm = AlarmHandle.kInvalidHandle;
            }

            AlienUtils.sAlienVisitationAlarm = AlarmManager.Global.AddAlarmDay(GetActivityHour(), DaysOfTheWeek.All,
                new AlarmTimerCallback(AlienActivityCallback), "Alien Activity Ex Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);
        }

        public static void SetRefreshAlarm()
        {
            if (AlienUtils.sAlienHouseholdRefreshAlarm != AlarmHandle.kInvalidHandle && !CheckAlarm(AlienUtils.sAlienHouseholdRefreshAlarm, kRefreshCallback))
            {
                AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienHouseholdRefreshAlarm);
                AlienUtils.sAlienHouseholdRefreshAlarm = AlarmHandle.kInvalidHandle;
            }

            AlienUtils.sAlienHouseholdRefreshAlarm = AlarmManager.Global.AddAlarmDay(15f, DaysOfTheWeek.All, new AlarmTimerCallback(AlienRefreshCallback),
                "Alien Household Refresh Ex Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);
        }

        public static void Shutdown()
        {
            AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienHouseholdRefreshAlarm);
            AlienUtils.sAlienHouseholdRefreshAlarm = AlarmHandle.kInvalidHandle;

            AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienVisitationAlarm);
            AlienUtils.sAlienVisitationAlarm = AlarmHandle.kInvalidHandle;
        }

        public static void Startup()
        {
            AlienUtils.sAlienTeenFemaleOutfit = ResourceKey.CreateOutfitKeyFromProductVersion("tf_alienOutfit", ProductVersion.EP8);
            AlienUtils.sAlienTeenMaleOutfit = ResourceKey.CreateOutfitKeyFromProductVersion("tm_alienOutfit", ProductVersion.EP8);
            AlienUtils.sAlienAdultFemaleOutfit = ResourceKey.CreateOutfitKeyFromProductVersion("af_alienOutfit", ProductVersion.EP8);
            AlienUtils.sAlienAdultMaleOutfit = ResourceKey.CreateOutfitKeyFromProductVersion("am_alienOutfit", ProductVersion.EP8);
            AlienUtils.sAlienElderFemaleOutfit = ResourceKey.CreateOutfitKeyFromProductVersion("ef_alienOutfit", ProductVersion.EP8);
            AlienUtils.sAlienElderMaleOutfit = ResourceKey.CreateOutfitKeyFromProductVersion("em_alienOutfit", ProductVersion.EP8);

            if (GameUtils.GetCurrentWorldType() != WorldType.Vacation)
            {
                SetActivityAlarm();
                SetRefreshAlarm();

                if (AlienUtils.sAlienAbductionHelper == null)
                {
                    AlienUtils.sAlienAbductionHelper = new AlienUtils.AlienAbductionHelper();
                }
            }
        }
    }
}
