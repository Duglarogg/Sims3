using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.CAS;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Helpers
{
    public abstract class AlienUtilsEx : Common.IWorldLoadFinished, Common.IWorldQuit
    {
        static ResourceKey[] AlienSkinTones = new ResourceKey[]
        {
            new ResourceKey(ResourceUtils.HashString64("AlienEP08 Skin"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Aqua Skin Tone"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Blue Skin"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Gold Skin Tone"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Purple Skine Tone"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Alien Skin"), 55867754u, 0u)
        };

        static Color[] AlienHairColors = new Color[]
        {
            new Color(174, 222, 181),
            new Color(186, 219, 202),
            new Color(144, 183, 151),
            new Color(207, 242, 214),
            new Color(144, 183, 151),
            new Color(174, 222, 181),
            new Color(186, 219, 202),
            new Color(144, 183, 151),
            new Color(207, 242, 214),
            new Color(144, 183, 151)
        };

        static AlarmTimerCallback visitationCallback = new AlarmTimerCallback(AlienVisitationCallback);
        static AlarmTimerCallback refreshCallback = new AlarmTimerCallback(AlienRefreshCallback);

        static AlienUtilsEx()
        {
            AlienUtils.kAlienHouseholdValidAges = new CASAgeGenderFlags[]
            {
                CASAgeGenderFlags.Teen,
                CASAgeGenderFlags.YoungAdult,
                CASAgeGenderFlags.Adult,
                CASAgeGenderFlags.Elder
            };
        }

        public static void AlienVisitationCallback()
        {
            if (Abductor.Settings.Debugging)
            {
                StyledNotification.Format format = new StyledNotification.Format("Alien Visitation Alarm triggered!",
                    StyledNotification.NotificationStyle.kDebugAlert);
                StyledNotification.Show(format);
            }

            // Cache alien household
            Household alienHousehold = Household.AlienHousehold;

            // If alien household is null or empty, stop
            if (alienHousehold == null || alienHousehold.NumMembers == 0)
            {
                Common.DebugNotify("Alien Activity: Alien Household Null or Empty");
                return;
            }

            // Determine the chance of alien activity
            float chance = CalculateAlienActivityChance();

            // Check for alien activity
            if (!RandomUtil.RandomChance(chance))
            {
                // Activity chance failed; stop
                Common.DebugNotify("Alien Activity: Chance Fail " + chance);
                ResetAbductionHelper();
                return;
            }

            // Activity chance passed; get list of valid aliens
            List<SimDescription> aliens = GetValidAliens();

            // If no valid aliens, stop
            if(aliens == null)
            {
                Common.DebugNotify("Alien Visitation: No Valid Aliens");
                ResetAbductionHelper();
                return;
            }

            // Randomly select a valid alien
            SimDescription alien = RandomUtil.GetRandomObjectFromList<SimDescription>(aliens);

            // Determine chance for an abduction
            chance = CalculateAlienAbductionChance();

            // Check for alien abduction
            if (!RandomUtil.RandomChance(chance))
            {
                // Abduction chance failed; move on to alien visitation
                Common.DebugNotify("Alien Abduction: Chance Fail " + chance);
            }
            else
            {
                // Abduction chance passed; determine chance active household is the target
                chance = CalculateAlienAbductionChance(true);

                // Check if active household is abduction target
                if (IsActiveHouseholdTarget(chance) && CanASimBeAbducted(Household.ActiveHousehold))
                {
                    // Active househod is the target; get a list of valid abductees
                    List<Sim> validAbductees = GetValidAbductees(Household.ActiveHousehold);

                    // If abductee list is null, stop
                    if (validAbductees == null)
                    {
                        Common.DebugNotify("Alien Abduction: No Valid Abductees");
                        ResetAbductionHelper();
                        return;
                    }

                    // Randomly select an active household Sim for abduction
                    Sim abductee = RandomUtil.GetRandomObjectFromList<Sim>(validAbductees);

                    // Start an alien abduction situation on the abductee's current lot
                    Lot lot = abductee.LotCurrent;

                    // Attempt to abduct the selected Sim
                    if (lot != null)
                    {
                        // Start an alien abduction situation on the abductee's lot
                        AlienAbductionSituationEx.Create(alien, abductee, lot);
                    }
                    else
                    {
                        Common.DebugNotify("Alien Abduction: Abductee Lot is Null");
                    }
                }
                else
                {
                    // Active household is not target; get a list of valid lots
                    List<Lot> validLots = GetValidLots();

                    // If there are no valid lots, give up and return
                    if (validLots == null)
                    {
                        Common.DebugNotify("Alien Abduction: No Valid Lots");
                        ResetAbductionHelper();
                        return;
                    }

                    // Randomly select a lot
                    Lot lot = RandomUtil.GetRandomObjectFromList<Lot>(validLots);

                    // Get valid abductees on the selected lot
                    List<Sim> validAbductees = GetValidAbductees(lot);

                    // If there are no valid abductees, give up and return
                    if (validAbductees == null)
                    {
                        Common.DebugNotify("Alien Abduction: No Valid Abductees");
                        ResetAbductionHelper();
                        return;
                    }

                    // Randomly select an abductee
                    Sim abductee = RandomUtil.GetRandomObjectFromList<Sim>(validAbductees);

                    // Start an alien abduction situation on a random, valid lot
                    AlienAbductionSituationEx.Create(alien, abductee, lot);
                }

                ResetAbductionHelper();
                return;
            }

            // Abduction check failed; check for visitation
            chance = CalculateAlienVisitationChance();

            // Check for alien visitation
            if(!RandomUtil.RandomChance(chance))
            {
                // Visitation chance failed; stop
                Common.DebugNotify("Alien Visitation: Chance Fail " + chance);
                ResetAbductionHelper();
                return;
            }
            else
            {
                // Visitation check passed; determine chance active household is the target
                chance = CalculateAlienVisitationChance(true, alien);

                // Check if the active household is the visitation target
                if (IsActiveHouseholdTarget(chance))
                {
                    // Visitation chance passed

                    // Instantiate the visitor at the farthest lot from the active lot
                    Lot farthestLot = LotManager.GetFarthestLot(Household.ActiveHousehold.LotHome);
                    Sim visitor = alien.InstantiateOffScreen(farthestLot);

                    // Start alien visitation situation on active lot
                    AlienSituation.Create(visitor, Household.ActiveHousehold.LotHome);
                }
                else
                {
                    // Visitation chance failed; get a list of valid lots
                    List<Lot> lots = GetValidLots();

                    // If no valid lots, stop
                    if (lots == null)
                    {
                        Common.DebugNotify("Alien Visitation: No Valid Lots");
                        ResetAbductionHelper();
                        return;
                    }

                    // Randomly select a lot
                    Lot lot = RandomUtil.GetRandomObjectFromList<Lot>(lots);

                    // Instantiate alien on farthest lot from target
                    Lot fartherLot = LotManager.GetFarthestLot(lot);
                    Sim visitor = alien.InstantiateOffScreen(fartherLot);

                    // Start alien visitation situation on the selected lot
                    AlienSituation.Create(visitor, lot);
                }

                // Alien activity completed!
                ResetAbductionHelper();
                return;
            }
        }

        public static void AlienRefreshCallback()
        {
            if (Abductor.Settings.Debugging)
            {
                StyledNotification.Format format = new StyledNotification.Format("Alien Household Refresh Alarm triggered!", 
                    StyledNotification.NotificationStyle.kDebugAlert);
                StyledNotification.Show(format);
            }

            if (Household.AlienHousehold == null) return;

            if(Household.AlienHousehold.NumMembers < AlienUtils.kAlienHouseholdNumMembers)
            {
                CASAgeGenderFlags age = RandomUtil.GetRandomObjectFromList<CASAgeGenderFlags>(AlienUtils.kAlienHouseholdValidAges);
                CASAgeGenderFlags gender = RandomUtil.CoinFlip() ? CASAgeGenderFlags.Male : CASAgeGenderFlags.Female;
                SimDescription description = MakeAlien(age, gender, GameUtils.GetCurrentWorld(), 1f, true);
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

                if (Abductor.Settings.mFutureSim)
                {
                    description.TraitManager.AddElement(TraitNames.FutureSim);
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

        public static float CalculateAlienAbductionChance(bool isActive = false)
        {
            float result = isActive ? Abductor.Settings.mActiveAbductionChance : Abductor.Settings.mBaseAbductionChance;

            if (result <= 0)
            {
                Common.DebugNotify("Alien Abduction: Disabled");
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
            {
                result += Abductor.Settings.mTelescopeUsedBonus;
            }

            if (AlienUtils.sAlienAbductionHelper.SpaceRocksFound > 0)
            {
                result += Math.Min(AlienUtils.sAlienAbductionHelper.SpaceRocksFound * Abductor.Settings.mSpaceRockFoundBonus, Abductor.Settings.mMaxSpaceRockFoundBonus);
            }

            return result;
        }

        public static float CalculateAlienActivityChance()
        {
            float result = Abductor.Settings.mBaseActivityChance;

            if (result <= 0)
            {
                Common.DebugNotify("Alien Activity: Disabled");
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
            {
                result += Abductor.Settings.mTelescopeUsedBonus;
            }

            return result;
        }

        public static float CalculateAlienVisitationChance(bool isActive = false, SimDescription alien = null)
        {
            float result = isActive ? Abductor.Settings.mActiveVisitationChance : Abductor.Settings.mBaseVisitationChance;

            if (result <= 0)
            {
                Common.DebugNotify("Alien Visitation: Disabled");
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
            {
                result += Abductor.Settings.mTelescopeUsedBonus;
            }

            if (AlienUtils.sAlienAbductionHelper.SpaceRocksFound > 0)
            {
                result += Math.Min(AlienUtils.sAlienAbductionHelper.SpaceRocksFound * Abductor.Settings.mSpaceRockFoundBonus, Abductor.Settings.mMaxSpaceRockFoundBonus);
            }

            if (isActive)
            {
                result += CheckActiveLotForSpaceRockBonus();
                result += CheckActiveHouseholdForLTRBonus(alien);
            }

            return result;
        }

        public static bool CanASimBeAbducted(Household household)
        {
            if (AlienUtils.sAlienAbductionHelper == null) return false;

            int num = 0;

            foreach (Sim current in household.LotHome.GetSims())
            {
                if (current.IsHuman)
                {
                    if (current.SimDescription.TeenOrAbove)
                    {
                        if (!AlienUtils.IsHouseboatAndNotDocked(current.LotCurrent))
                        {
                            num++;
                        }
                    }
                }
            }

            return (num > 0);
        }

        public static bool CanASimBeAbducted(Lot lot)
        {
            if (AlienUtils.sAlienAbductionHelper == null) return false;

            if (AlienUtils.IsHouseboatAndNotDocked(lot)) return false;

            if (lot.GetSimsCount() == 0) return false;

            bool flag = false;
            int num = 0;

            foreach (Sim current in lot.GetSims())
            {
                if (current.SimDescription.ToddlerOrBelow) flag = true;
                else if (current.SimDescription.TeenOrAbove) num++;
            }

            return !flag || num >= 2;
        }

        public static bool CheckAlarm(AlarmHandle handle, AlarmTimerCallback callback)
        {
            List<AlarmManager.Timer> list = AlarmManager.Global.mTimers[handle];

            foreach(AlarmManager.Timer current in list)
            {
                if (current.CallBack == callback)
                {
                    return true;
                }
            }

            return false;
        }
       
        public static float CheckActiveHouseholdForLTRBonus(SimDescription alien)
        {
            float result = 0;
            bool flag = false;

            foreach(SimDescription current in Household.ActiveHousehold.SimDescriptions)
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

            return result;
        }

        public static float CheckActiveLotForSpaceRockBonus()
        {
            if (Household.ActiveHousehold == null || Household.ActiveHousehold.LotHome == null) return 0;

            int count = Household.ActiveHousehold.LotHome.GetObjects<RockGemMetalBase>(new Predicate<RockGemMetalBase>(AlienUtils.IsSpaceRock)).Count;

            if (count >= AlienUtils.kAlienHouseholdSpaceRocksForBonus)
            {
                return AlienUtils.kAlienHouseholdVisitChanceActiveSpaceRockMod;
            }
            else
            {
                return 0;
            }
        }

        public static List<Sim> GetValidAbductees(Household household)
        {
            List<Sim> list = new List<Sim>();

            foreach (Sim current in household.mMembers.ActorList)
            {
                if (current.IsHuman && current.SimDescription.TeenOrAbove)
                {
                    if (!AlienUtils.IsHouseboatAndNotDocked(current.LotCurrent))
                    {
                        list.Add(current);
                    }
                }
            }

            if (list.Count == 0) return null;
            else return list;
        }

        public static List<Sim> GetValidAbductees(Lot lot)
        {
            List<Sim> list = new List<Sim>();

            foreach (Sim current in lot.GetSims())
            {
                if (current.IsHuman && current.SimDescription.TeenOrAbove && !current.Household.IsTouristHousehold)
                {
                    if (!AlienUtils.IsHouseboatAndNotDocked(current.LotCurrent))
                    {
                        list.Add(current);
                    }
                }
            }

            if (list.Count == 0) return null;
            else return list;
        }

        public static List<SimDescription> GetValidAliens()
        {
            List<SimDescription> list = new List<SimDescription>();

            if (Household.AlienHousehold != null)
            {
                foreach (SimDescription current in Household.AlienHousehold.SimDescriptions)
                {
                    if (current.CreatedSim == null)
                    {
                        list.Add(current);
                    }
                }
            }

            if (list.Count == 0) return null;
            else return list;
        }

        public static List<Lot> GetValidLots()
        {
            List<Lot> list = new List<Lot>();

            foreach (Lot current in LotManager.AllLotsWithoutCommonExceptions)
            {
                if (current.LotType != LotType.Tutorial && current != Household.ActiveHousehold.LotHome)
                {
                    if (!AlienUtils.IsHouseboatAndNotDocked(current))
                    {
                        list.Add(current);
                    }
                }
            }

            if (list.Count == 0) return null;
            else return list;
        }

        public static bool IsActiveHouseholdTarget(float chance)
        {
            if (Household.ActiveHousehold == null) return false;

            if (Household.ActiveHouseholdLot == null) return false;

            if (AlienUtils.IsHouseboatAndNotDocked(Household.ActiveHouseholdLot)) return false;

            return RandomUtil.RandomChance(chance);
        }

        public static SimDescription MakeAlien(CASAgeGenderFlags age, CASAgeGenderFlags gender, WorldName homeworld, float alienDNAPercentage, bool assignRandomTraits)
        {
            ResourceKey skinTone = AlienSkinTones[RandomUtil.GetInt(0, AlienSkinTones.Length - 1)];
            float skinToneIndex;

            if (skinTone == AlienSkinTones[0])
            {
                skinToneIndex = 1f;
            }
            else if (skinTone == AlienSkinTones[1] || skinTone == AlienSkinTones[3] || skinTone == AlienSkinTones[4])
            {
                skinToneIndex = 0.25f + (0.5f * RandomUtil.GetFloat(1f));
            }
            else if (skinTone == AlienSkinTones[2])
            {
                skinToneIndex = 0.5f + (0.5f * RandomUtil.GetFloat(1f));
            }
            else
            {
                skinToneIndex = 0.25f + (0.75f * RandomUtil.GetFloat(1f));
            }

            SimBuilder sb = new SimBuilder();
            sb.Age = age;
            sb.Gender = gender;
            sb.Species = CASAgeGenderFlags.Human;
            sb.SkinTone = skinTone;
            sb.SkinToneIndex = skinToneIndex;
            sb.TextureSize = 1024u;
            sb.UseCompression = true;
            bool flag = (gender == CASAgeGenderFlags.Female);

            float num = flag ? Genetics.kAlienHeadWide[0] : Genetics.kAlienHeadWide[1];

            if (num != 0f)
            {
                ResourceKey key = new ResourceKey(ResourceUtils.HashString64("HeadWide"), 56144010u, 0u);
                sb.SetFacialBlend(key, num);
            }

            num = flag ? Genetics.kAlienJawChinSquare[0] : Genetics.kAlienJawChinSquare[1];

            if (num != 0f)
            {
                ResourceKey key2 = new ResourceKey(ResourceUtils.HashString64("JawChinSquare"), 56144010u, 0u);
                sb.SetFacialBlend(key2, num);
            }

            num = flag ? Genetics.kAlienNoseScaleDown[0] : Genetics.kAlienNoseScaleDown[1];

            if (num != 0f)
            {
                ResourceKey key3 = new ResourceKey(ResourceUtils.HashString64("NoseScaleDown"), 56144010u, 0u);
                sb.SetFacialBlend(key3, num);
            }

            num = flag ? Genetics.kAlienNoseDown[0] : Genetics.kAlienNoseDown[1];

            if (num != 0f)
            {
                ResourceKey key4 = new ResourceKey(ResourceUtils.HashString64("NoseDown"), 56144010u, 0u);
                sb.SetFacialBlend(key4, num);
            }

            num = flag ? Genetics.kAlienNoseThin[0] : Genetics.kAlienNoseThin[1];

            if (num != 0f)
            {
                ResourceKey key5 = new ResourceKey(ResourceUtils.HashString64("NoseThin"), 56144010u, 0u);
                sb.SetFacialBlend(key5, num);
            }

            num = flag ? Genetics.kAlienNoseBridgeRotate[0] : Genetics.kAlienNoseBridgeRotate[1];

            if (num != 0f)
            {
                ResourceKey key6 = new ResourceKey(ResourceUtils.HashString64("NoseBridgeRotate"), 56144010u, 0u);
                sb.SetFacialBlend(key6, num);
            }

            num = flag ? Genetics.kAlienNoseBridgeOut[0] : Genetics.kAlienNoseBridgeOut[1];

            if (num != 0f)
            {
                ResourceKey key7 = new ResourceKey(ResourceUtils.HashString64("NoseBridgeOut"), 56144010u, 0u);
                sb.SetFacialBlend(key7, num);
            }

            num = flag ? Genetics.kAlienNoseBridgeScaleUp[0] : Genetics.kAlienNoseBridgeScaleUp[1];

            if (num != 0f)
            {
                ResourceKey key8 = new ResourceKey(ResourceUtils.HashString64("NoseBridgeScaleUp"), 56144010u, 0u);
                sb.SetFacialBlend(key8, num);
            }

            num = flag ? Genetics.kAlienNoseNostrilsUp[0] : Genetics.kAlienNoseNostrilsUp[1];

            if (num != 0f)
            {
                ResourceKey key9 = new ResourceKey(ResourceUtils.HashString64("NoseNostrilsUp"), 56144010u, 0u);
                sb.SetFacialBlend(key9, num);
            }

            num = flag ? Genetics.kAlienNoseNostrilScale[0] : Genetics.kAlienNoseNostrilScale[1];

            if (num != 0f)
            {
                ResourceKey key10 = new ResourceKey(ResourceUtils.HashString64("NoseNostrilScale"), 56144010u, 0u);
                sb.SetFacialBlend(key10, num);
            }

            num = flag ? Genetics.kAlienNoseTipScaleDown[0] : Genetics.kAlienNoseTipScaleDown[1];

            if (num != 0f)
            {
                ResourceKey key11 = new ResourceKey(ResourceUtils.HashString64("NoseTipScaleDown"), 56144010u, 0u);
                sb.SetFacialBlend(key11, num);
            }

            num = flag ? Genetics.kAlienEyesBrowCurve[0] : Genetics.kAlienEyesBrowCurve[1];

            if (num != 0f)
            {
                ResourceKey key12 = new ResourceKey(ResourceUtils.HashString64("EyesBrowCurve"), 56144010u, 0u);
                sb.SetFacialBlend(key12, num);
            }

            num = flag ? Genetics.kAlienEyesBrowRotate[0] : Genetics.kAlienEyesBrowRotate[1];

            if (num != 0f)
            {
                ResourceKey key13 = new ResourceKey(ResourceUtils.HashString64("EyesBrowRotate"), 56144010u, 0u);
                sb.SetFacialBlend(key13, num);
            }

            num = flag ? Genetics.kAlienMouthLipUpperDefinition[0] : Genetics.kAlienMouthLipUpperDefinition[1];

            if (num != 0f)
            {
                ResourceKey key14 = new ResourceKey(ResourceUtils.HashString64("MouthLipUpperDefinition"), 56144010u, 0u);
                sb.SetFacialBlend(key14, num);
            }

            num = flag ? Genetics.kAlienMouthCurveDown[0] : Genetics.kAlienMouthCurveDown[1];

            if (num != 0f)
            {
                ResourceKey key15 = new ResourceKey(ResourceUtils.HashString64("MouthCurveDown"), 56144010u, 0u);
                sb.SetFacialBlend(key15, num);
            }

            num = flag ? Genetics.kAlienMouthIn[0] : Genetics.kAlienMouthIn[1];

            if (num != 0f)
            {
                ResourceKey key16 = new ResourceKey(ResourceUtils.HashString64("MouthIn"), 56144010u, 0u);
                sb.SetFacialBlend(key16, num);
            }

            num = flag ? Genetics.kAlienTranslateMouthDown[0] : Genetics.kAlienTranslateMouthDown[1];

            if (num != 0f)
            {
                ResourceKey key17 = new ResourceKey(ResourceUtils.HashString64("TranslateMouthDown"), 56144010u, 0u);
                sb.SetFacialBlend(key17, num);
            }

            num = flag ? Genetics.kAlienMouthThin[0] : Genetics.kAlienMouthThin[1];

            if (num != 0f)
            {
                ResourceKey key18 = new ResourceKey(ResourceUtils.HashString64("MouthThin"), 56144010u, 0u);
                sb.SetFacialBlend(key18, num);
            }

            num = flag ? Genetics.kAlienEyeAlienCorrector[0] : Genetics.kAlienEyeAlienCorrector[1];

            if (num != 0f)
            {
                ResourceKey key19 = new ResourceKey(ResourceUtils.HashString64("EyeAlienCorrector"), 56144010u, 0u);
                sb.SetFacialBlend(key19, num);
            }

            num = flag ? Genetics.kAlienEyeAlien[0] : Genetics.kAlienEyeAlien[1];

            if (num != 0f)
            {
                ResourceKey key20 = new ResourceKey(ResourceUtils.HashString64("EyeAlien"), 56144010u, 0u);
                sb.SetFacialBlend(key20, num);
            }

            num = flag ? Genetics.kAlienJawChinDown[0] : Genetics.kAlienJawChinDown[1];

            if (num != 0f)
            {
                ResourceKey key21 = new ResourceKey(ResourceUtils.HashString64("JawChinDown"), 56144010u, 0u);
                sb.SetFacialBlend(key21, num);
            }

            num = flag ? Genetics.kAlienJawChinScaleDown[0] : Genetics.kAlienJawChinScaleDown[1];

            if (num != 0f)
            {
                ResourceKey key22 = new ResourceKey(ResourceUtils.HashString64("JawChinScaleDown"), 56144010u, 0u);
                sb.SetFacialBlend(key22, num);
            }

            num = flag ? Genetics.kAlienJawThin[0] : Genetics.kAlienJawThin[1];

            if (num != 0f)
            {
                ResourceKey key23 = new ResourceKey(ResourceUtils.HashString64("JawThin"), 56144010u, 0u);
                sb.SetFacialBlend(key23, num);
            }

            num = flag ? Genetics.kAlienJawCheeksIn[0] : Genetics.kAlienJawCheeksIn[1];

            if (num != 0f)
            {
                ResourceKey key24 = new ResourceKey(ResourceUtils.HashString64("JawCheeksIn"), 56144010u, 0u);
                sb.SetFacialBlend(key24, num);
            }

            num = flag ? Genetics.kAlienJawCheekSharpness[0] : Genetics.kAlienJawCheekSharpness[1];

            if (num != 0f)
            {
                ResourceKey key25 = new ResourceKey(ResourceUtils.HashString64("JawCheekSharpness"), 56144010u, 0u);
                sb.SetFacialBlend(key25, num);
            }

            num = flag ? Genetics.kAlienEarsRotateY[0] : Genetics.kAlienEarsRotateY[1];

            if (num != 0f)
            {
                ResourceKey key26 = new ResourceKey(ResourceUtils.HashString64("EarsRotateY"), 56144010u, 0u);
                sb.SetFacialBlend(key26, num);
            }

            num = flag ? Genetics.kAlienEarPoint[0] : Genetics.kAlienEarPoint[1];

            if (num != 0f)
            {
                ResourceKey key27 = new ResourceKey(ResourceUtils.HashString64("EarPoint"), 56144010u, 0u);
                sb.SetFacialBlend(key27, num);
            }

            num = flag ? Genetics.kAlienHeadProfileOut[0] : Genetics.kAlienHeadProfileOut[1];

            if (num != 0f)
            {
                ResourceKey key28 = new ResourceKey(ResourceUtils.HashString64("HeadProfileOut"), 56144010u, 0u);
                sb.SetFacialBlend(key28, num);
            }

            num = flag ? Genetics.kAlienEyesLashesThin[0] : Genetics.kAlienEyesLashesThin[1];

            if (num != 0f)
            {
                ResourceKey key29 = new ResourceKey(ResourceUtils.HashString64("EyesLashesThin"), 56144010u, 0u);
                sb.SetFacialBlend(key29, num);
            }

            num = flag ? Genetics.kAlienJawCheeksBoneDown[0] : Genetics.kAlienJawCheeksBoneDown[1];

            if (num != 0f)
            {
                ResourceKey key30 = new ResourceKey(ResourceUtils.HashString64("JawCheeksBoneDown"), 56144010u, 0u);
                sb.SetFacialBlend(key30, num);
            }

            num = flag ? Genetics.kAlienJawChinRound[0] : Genetics.kAlienJawChinRound[1];

            if (num != 0f)
            {
                ResourceKey key31 = new ResourceKey(ResourceUtils.HashString64("JawChinRound"), 56144010u, 0u);
                sb.SetFacialBlend(key31, num);
            }

            num = flag ? Genetics.kAlienNoseTipDepthIn[0] : Genetics.kAlienNoseTipDepthIn[1];

            if (num != 0f)
            {
                ResourceKey key32 = new ResourceKey(ResourceUtils.HashString64("NoseTipDepthIn"), 56144010u, 0u);
                sb.SetFacialBlend(key32, num);
            }

            SimDescription alienDescription = Genetics.MakeSim(sb, age, gender, skinTone, skinToneIndex, AlienHairColors, homeworld, 4294967295u, true);

            if (alienDescription != null)
            {
                alienDescription.FirstName = SimUtils.GetRandomAlienGivenName(alienDescription.IsMale);
                alienDescription.LastName = SimUtils.GetRandomAlienFamilyName();
                alienDescription.SetAlienDNAPercentage(alienDNAPercentage);
                alienDescription.VoicePitchModifier = RandomUtil.GetFloat(1.2f, 1.6f);
                alienDescription.VoiceVariation = (VoiceVariationType)RandomUtil.GetInt(2);

                if (assignRandomTraits)
                {
                    Genetics.AssignRandomTraits(alienDescription);
                }

                if (alienDescription.TeenOrAbove)
                {
                    string s = "a";

                    if (age != CASAgeGenderFlags.Teen)
                    {
                        if (age == CASAgeGenderFlags.Elder)
                        {
                            s = "e";
                        }
                    }
                    else
                    {
                        s = "t";
                    }

                    string s2 = alienDescription.IsFemale ? "f" : "m";
                    string name = s + s2 + "_alienOutfit";
                    ResourceKey key33 = ResourceKey.CreateOutfitKeyFromProductVersion(name, ProductVersion.EP8);
                    SimOutfit outfit = OutfitUtils.CreateOutfitForSim(alienDescription, key33, OutfitCategories.Everyday, OutfitCategories.Everyday, false);

                    if (outfit != null)
                    {
                        alienDescription.AddOutfit(outfit, OutfitCategories.Everyday, true);
                    }

                    outfit = OutfitUtils.CreateOutfitForSim(alienDescription, key33, OutfitCategories.Formalwear, OutfitCategories.Formalwear, false);

                    if (outfit != null)
                    {
                        alienDescription.AddOutfit(outfit, OutfitCategories.Formalwear, true);
                    }

                    outfit = OutfitUtils.CreateOutfitForSim(alienDescription, key33, OutfitCategories.Outerwear, OutfitCategories.Outerwear, false);

                    if (outfit != null)
                    {
                        alienDescription.AddOutfit(outfit, OutfitCategories.Outerwear, true);
                    }
                }
            }

            return alienDescription;
        }

        public static SimDescription MakeAlienBaby(SimDescription alien, SimDescription abductee, CASAgeGenderFlags gender, float averageMood, Random pregoRandom, bool interactive)
        {
            SimDescription baby = MakeAlien(CASAgeGenderFlags.Baby, gender, GameUtils.GetCurrentWorld(), 1f, interactive);

            if (baby != null)
            {
                if (interactive)
                {
                    baby.FirstName = string.Empty;
                }

                baby.LastName = alien.LastName;
                Genetics.AssignTraits(baby, alien, abductee, interactive, averageMood, pregoRandom);
                baby.TraitManager.AddHiddenElement(AbductionBuffs.sAlienChild);

                if (Abductor.Settings.mFutureSim)
                {
                    baby.TraitManager.AddHiddenElement(TraitNames.FutureSim);
                }

                baby.CelebrityManager.SetBabyLevel(Genetics.AssignBabyCelebrityLevel(null, abductee));
                abductee.Genealogy.AddChild(baby.Genealogy);

                if (alien != null)
                {
                    alien.Genealogy.AddChild(baby.Genealogy);
                }

                /* WISHLIST-------------------------------------------------------------------------------------- */
                // Link to Hybrid to allow alien children to inherit one or more occult types from abductee.
                /* ---------------------------------------------------------------------------------------------- */
            }

            return baby;
        }

        public void OnWorldLoadFinished()
        {
            //Startup();
        }

        public void OnWorldQuit()
        {
            //AlienUtils.Shutdown();
        }

        public static float RandomVisitationHour()
        {
            return (Abductor.Settings.mEarliestVisitHour + RandomUtil.GetInt(0, Abductor.Settings.mVisitWindow));
        }

        public static void ResetAbductionHelper()
        {
            AlienUtils.sAlienAbductionHelper.SpaceRocksFound = 0;
            AlienUtils.sAlienAbductionHelper.TelescopeUsed = false;
        }

        public static void ResetAlienVisitationAlarm()
        {
            if (AlienUtils.sAlienVisitationAlarm != AlarmHandle.kInvalidHandle && !CheckAlarm(AlienUtils.sAlienVisitationAlarm, visitationCallback))
            {
                AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienVisitationAlarm);
                AlienUtils.sAlienVisitationAlarm = AlarmHandle.kInvalidHandle;
            }

            AlienUtils.sAlienVisitationAlarm = AlarmManager.Global.AddAlarmDay(RandomVisitationHour(), DaysOfTheWeek.All, 
                new AlarmTimerCallback(AlienVisitationCallback), "Alien Visitation Ex Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);
        }

        public static void ResetAlienRefreshAlarm()
        {
            if (AlienUtils.sAlienHouseholdRefreshAlarm != AlarmHandle.kInvalidHandle && !CheckAlarm(AlienUtils.sAlienHouseholdRefreshAlarm, refreshCallback))
            {
                AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienHouseholdRefreshAlarm);
                AlienUtils.sAlienHouseholdRefreshAlarm = AlarmHandle.kInvalidHandle;
            }

            AlienUtils.sAlienHouseholdRefreshAlarm = AlarmManager.Global.AddAlarmDay(15f, DaysOfTheWeek.All,
                new AlarmTimerCallback(AlienRefreshCallback), "Alien Household Refresh Ex Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);
        }

        public static void Startup()
        {
            if (GameUtils.GetCurrentWorldType() != WorldType.Vacation)
            {
                ResetAlienVisitationAlarm();
                ResetAlienRefreshAlarm();
            }
        }
    }
}
