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

/* <NOTES>
 *  static AlienUtilsEx()
 *      Setting valid ages for NPC alien generation could be done through tuning files instead
 *      
 *  CanASimBeAbucted(...)
 *      Currently not referenced by anything; candidate for culling.
 *      
 *  MakeAlien(...)
 *      May make this method private, as it should not be called outside AlienRefreshCallback and MakeAlienBaby methods
 *  
 *  MakeAlienBaby(...)
 *      Need to add the custom "Alien Child" hidden trait to alien babies.
 *      
 *  ResetAlienActivityAlarm()
 *      May need to uncomment a for-loop that clears outs the sAlienActivityAlarm array before creating new alarms
 */

namespace NRaas.AliensSpace.Helpers
{
    public abstract class AlienUtilsEx : Common.IWorldLoadFinished, Common.IWorldQuit
    {
        static ResourceKey[] AlienSkinTones = new ResourceKey[]
        {
            new ResourceKey(ResourceUtils.HashString64("AlienEP08"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Aqua Skin Tone"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Blue Skin"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Gold Skin Tone"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Purple Skin Tone"), 55867754u, 0u),
            new ResourceKey(ResourceUtils.HashString64("Alien Skin"), 55867754u, 0u)
        };

        static Color[] AlienHairColors = new Color[]
        {
            new Color(174, 222, 181),
            new Color(186, 219, 202),
            new Color(144, 183, 151),
            new Color(207, 242, 214),
            new Color(144, 183, 151)
        };

        static AlarmHandle[] sAlienActivityAlarm = new AlarmHandle[24];
        static int cooldown = 0;

        static AlarmTimerCallback refreshCallback = new AlarmTimerCallback(AlienRefreshCallback);

        static AlienUtilsEx()
        {
            // <NOTE> This could be done through tuning files instead
            AlienUtils.kAlienHouseholdValidAges = new CASAgeGenderFlags[]
            {
                CASAgeGenderFlags.Teen,
                CASAgeGenderFlags.YoungAdult,
                CASAgeGenderFlags.Adult,
                CASAgeGenderFlags.Elder
            };

            for (int i = 0; i < 24; i++)
            {
                sAlienActivityAlarm[i] = AlarmHandle.kInvalidHandle;
            }
        }

        public static void AlienActivityCallback()
        {
            Common.DebugNotify("Alien Activity: Alarm Triggered");

            if (cooldown > 0)
            {
                cooldown = -1;
                Common.DebugNotify("Alien Activity: On Cooldown: " + cooldown.ToString() + " hours to go");
                return;
            }

            int hour = SimClock.Hours24;

            if (hour >= Aliens.Settings.mEarliestHour && hour <= (Aliens.Settings.mEarliestHour + Aliens.Settings.mActivityWindow) % 24)
            {
                if (Household.AlienHousehold == null || Household.AlienHousehold.NumMembers == 0)
                {
                    Common.DebugNotify("Alien Activity: Alien Household is Empty");
                    return;
                }

                float chance = GetActivityChance();

                if (!RandomUtil.RandomChance(chance))
                {
                    Common.DebugNotify("Alien Activity: Activity Roll Failed - " + chance + " %");
                    return;
                }

                List<SimDescription> aliens = GetValidAliens();

                if (aliens == null)
                {
                    Common.DebugNotify("Alien Activity: No valid aliens");
                    return;
                }

                SimDescription alien = RandomUtil.GetRandomObjectFromList(aliens);

                chance = GetAbductionChance();

                if (RandomUtil.RandomChance(chance))
                {
                    Common.DebugNotify("Alien Activity: Abduction Roll Pass - " + chance + "%");

                    /* <WISHLIST>
                     *      For now, abductions will only target the active household.  This is due to how alien babies are treated
                     *      by NRaasStoryProgression.  If the abductee is married, then they are treated as though they had committed
                     *      adultery with the alien baby as proof of the deed.  This can result undesired break ups/divorces, etc.  I
                     *      will need to study StoryProgression to determine how to allow alien pregnancies in non-active households
                     *      without the adultery drama.
                     */

                    List<Sim> validAbductees = GetValidAbductees(Household.ActiveHousehold);

                    if (validAbductees == null)
                    {
                        Common.DebugNotify("Alien Activity: Abduction Fail - No valid abductees");
                        return;
                    }

                    Sim abductee = RandomUtil.GetRandomObjectFromList(validAbductees);
                    Lot lot = abductee.LotCurrent;

                    if (lot == null)
                    {
                        Common.DebugNotify("Alien Activity: Abduction Fail - Abductee not on valid lot");
                        return;
                    }

                    // <NOTE> Start AlienAbductionSituationEx on abductee's current lot
                    cooldown = Aliens.Settings.mActivityCooldown;
                    ResetAbductionHelper();
                }
                else
                {
                    Common.DebugNotify("Alien Activity: Abduction Roll Fail - " + chance + "%");

                    chance = GetVisitationChance(Household.ActiveHousehold, alien);

                    if (RandomUtil.RandomChance(chance))
                    {
                        Common.DebugNotify("Alien Activity: Visit Active Roll Pass - " + chance + "%");

                        Sim visitor = alien.InstantiateOffScreen(LotManager.GetFarthestLot(Household.ActiveHousehold.LotHome));
                        AlienSituation.Create(visitor, Household.ActiveHousehold.LotHome);
                        cooldown = Aliens.Settings.mActivityCooldown;
                        ResetAbductionHelper();
                    }
                    else
                    {
                        Common.DebugNotify("Alien Activity: Visit Active Roll Fail - " + chance + "%");

                        List<Lot> lots = GetValidLots();

                        if (lots == null)
                        {
                            Common.DebugNotify("Alien Activity: Visit Fail - No valid lots");
                            return;
                        }

                        Lot lot = RandomUtil.GetRandomObjectFromList(lots);
                        Sim visitor = alien.InstantiateOffScreen(LotManager.GetFarthestLot(lot));
                        AlienSituation.Create(visitor, lot);
                        cooldown = Aliens.Settings.mActivityCooldown;
                        ResetAbductionHelper();
                    }
                }
            }
            else
                Common.DebugNotify("Alien Activity Alarm: Outside active hours");
        }

        public static void AlienRefreshCallback()
        {
            if (Aliens.Settings.Debugging)
                StyledNotification.Show(new StyledNotification.Format("Alien Household Refresh Triggered!",
                    StyledNotification.NotificationStyle.kDebugAlert));

            if (Household.AlienHousehold == null)
                return;

            if (Household.AlienHousehold.NumMembers < AlienUtils.kAlienHouseholdNumMembers)
            {
                CASAgeGenderFlags age = RandomUtil.GetRandomObjectFromList(AlienUtils.kAlienHouseholdValidAges);
                CASAgeGenderFlags gender = RandomUtil.CoinFlip() ? CASAgeGenderFlags.Male : CASAgeGenderFlags.Female;
                SimDescription description = MakeAlien(age, gender, GameUtils.GetCurrentWorld(), 1f, true);

                Skill element = null;

                element = description.SkillManager.AddElement(SkillNames.Logic);

                if (element != null)
                    element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mLogicSkill[0], Aliens.Settings.mLogicSkill[1]));

                element = description.SkillManager.AddElement(SkillNames.Handiness);

                if (element != null)
                    element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mHandinessSkill[0], Aliens.Settings.mHandinessSkill[1]));

                if (age == CASAgeGenderFlags.Teen)
                {
                    element = description.SkillManager.AddElement(SkillNames.LearnToDrive);

                    if (element != null)
                        element.ForceSkillLevelUp(SkillManager.GetMaximumSupportedSkillLevel(SkillNames.LearnToDrive));
                }

                if (Aliens.Settings.mFutureSim)
                {
                    description.TraitManager.AddElement(TraitNames.FutureSim);

                    element = description.SkillManager.AddElement(SkillNames.Future);

                    if (element != null)
                        element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mFutureSkill[0], Aliens.Settings.mFutureSkill[0]));
                }

                Household.AlienHousehold.AddSilent(description);
                description.OnHouseholdChanged(Household.AlienHousehold, false);
            }
        }

        /*
        private static bool CanASimBeAbducted(Household household)
        {
            if (AlienUtils.sAlienAbductionHelper == null)
                return false;

            int num = 0;

            foreach (Sim current in household.mMembers.SimList)
            {
                if (current.SimDescription.TeenOrAbove && current.LotCurrent != null && !AlienUtils.IsHouseboatAndNotDocked(current.LotCurrent))
                    num++;
            }

            return num > 0;
        }
        */

        public static bool CheckAlarm(AlarmHandle handle, AlarmTimerCallback callback)
        {
            List<AlarmManager.Timer> list = AlarmManager.Global.mTimers[handle];

            foreach (AlarmManager.Timer current in list)
            {
                if (current.CallBack == callback)
                    return true;
            }

            return false;
        }

        public static float CheckForLTRBonuses(Household household, SimDescription alien)
        {
            float result = 0;

            foreach (SimDescription current in household.SimDescriptions)
            {
                Relationship relationship = Relationship.Get(alien, current, false);

                if (relationship != null && relationship.LTR.Liking >= Aliens.Settings.mHighLTRThreshold)
                    result += Aliens.Settings.mHighLTRBonus;

                if (current.Genealogy.Parents.Contains(alien.Genealogy))
                    result += Aliens.Settings.mOffspringBonus;
            }

            return result;
        }

        public static float CheckForSpaceRocksOnLot(Lot lot)
        {
            float result = 0;

            if (lot == null)
                return result;

            int count = lot.GetObjects(new Predicate<RockGemMetalBase>(AlienUtils.IsSpaceRock)).Count;

            if (count <= Aliens.Settings.mSpaceRockThreshold)
                result += Aliens.Settings.mSpaceRockBonus;

            return result;
        }

        private static float GetAbductionChance()
        {
            float result = Aliens.Settings.mBaseAbductionChance;

            if (result <= 0)
            {
                Common.DebugNotify("Alien Activity: Abductions disabled");
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
                result += Aliens.Settings.mTelescopeBonus;

            if (AlienUtils.sAlienAbductionHelper.SpaceRocksFound > 0)
                result += Math.Min(AlienUtils.sAlienAbductionHelper.SpaceRocksFound * Aliens.Settings.mSpaceRockFoundBonus, 
                    Aliens.Settings.mMaxSpaceRockBonus);

            return result;
        }

        private static float GetActivityChance()
        {
            float result = Aliens.Settings.mBaseActivityChance;

            if (result <= 0)
            {
                Common.DebugNotify("Alien Activity: All activity disabled");
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
                result += Aliens.Settings.mTelescopeBonus;

            return result;
        }

        public static List<Sim> GetValidAbductees(Household household)
        {
            List<Sim> list = new List<Sim>();

            foreach (Sim current in household.mMembers.SimList)
            {
                if (current.SimDescription.TeenOrAbove && current.LotCurrent != null && !AlienUtils.IsHouseboatAndNotDocked(current.LotCurrent))
                    list.Add(current);
            }

            if (list.Count == 0)
                return null;
            else
                return list;
        }

        public static List<SimDescription> GetValidAliens()
        {
            List<SimDescription> list = new List<SimDescription>();

            if (Household.AlienHousehold != null)
                foreach(SimDescription current in Household.AlienHousehold.SimDescriptions)
                {
                    if (current.CreatedSim == null)
                        list.Add(current);
                }

            if (list.Count == 0)
                return null;
            else
                return list;
        }

        public static List<Lot> GetValidLots()
        {
            List<Lot> list = new List<Lot>();

            foreach (Lot current in LotManager.AllLotsWithoutCommonExceptions)
            {
                if (current.LotType != LotType.Tutorial && current != Household.ActiveHousehold.LotHome && !AlienUtils.IsHouseboatAndNotDocked(current))
                    list.Add(current);
            }

            if (list.Count == 0)
                return null;
            else
                return list;
        }

        public static float GetVisitationChance(Household household, SimDescription alien)
        {
            float result = Aliens.Settings.mBaseVisitChance;

            if (result <= 0)
            {
                Common.DebugNotify("Alien Activity: Visitations Disabled");
                return 0;
            }

            if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
                result += Aliens.Settings.mTelescopeBonus;

            if (AlienUtils.sAlienAbductionHelper.SpaceRocksFound > 0)
                result += Math.Min(AlienUtils.sAlienAbductionHelper.SpaceRocksFound * Aliens.Settings.mSpaceRockFoundBonus, 
                    Aliens.Settings.mMaxSpaceRockBonus);

            result += CheckForLTRBonuses(household, alien);
            result += CheckForSpaceRocksOnLot(household.LotHome);

            return result;
        }

        // <NOTE> May make this method private </NOTE>
        public static SimDescription MakeAlien(CASAgeGenderFlags age, CASAgeGenderFlags gender, WorldName homeworld, float alienDNAPercentage, bool assignRandomTraits)
        {
            ResourceKey skinTone = RandomUtil.GetRandomObjectFromList(AlienSkinTones);
            float skinToneIndex;

            if (skinTone == AlienSkinTones[0])
                skinToneIndex = 1f;
            else if (skinTone == AlienSkinTones[1] || skinTone == AlienSkinTones[3] || skinTone == AlienSkinTones[4])
                skinToneIndex = 0.25f + (0.50f * RandomUtil.GetFloat(1f));
            else if (skinTone == AlienSkinTones[2])
                skinToneIndex = 0.50f + (0.50f * RandomUtil.GetFloat(1f));
            else
                skinToneIndex = 0.25f + (0.75f * RandomUtil.GetFloat(1f));

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
                    Genetics.AssignRandomTraits(alienDescription);

                if (alienDescription.TeenOrAbove)
                {
                    string s = "a";

                    if (age != CASAgeGenderFlags.Teen)
                    {
                        if (age == CASAgeGenderFlags.Elder)
                            s = "e";
                    }
                    else
                        s = "t";

                    string s2 = alienDescription.IsFemale ? "f" : "m";
                    string name = s + s2 + "_alienOutfit";
                    ResourceKey key33 = ResourceKey.CreateOutfitKeyFromProductVersion(name, ProductVersion.EP8);
                    SimOutfit outfit = OutfitUtils.CreateOutfitForSim(alienDescription, key33, OutfitCategories.Everyday, OutfitCategories.Everyday, false);

                    if (outfit != null)
                        alienDescription.AddOutfit(outfit, OutfitCategories.Everyday, true);

                    outfit = OutfitUtils.CreateOutfitForSim(alienDescription, key33, OutfitCategories.Formalwear, OutfitCategories.Formalwear, false);

                    if (outfit != null)
                        alienDescription.AddOutfit(outfit, OutfitCategories.Formalwear, true);

                    outfit = OutfitUtils.CreateOutfitForSim(alienDescription, key33, OutfitCategories.Outerwear, OutfitCategories.Outerwear, false);

                    if (outfit != null)
                        alienDescription.AddOutfit(outfit, OutfitCategories.Outerwear, true);
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
                    baby.FirstName = string.Empty;

                baby.LastName = alien.LastName;
                Genetics.AssignTraits(baby, alien, abductee, interactive, averageMood, pregoRandom);
                // <NOTE> Add Alien Child hidden trait here! </NOTE>

                if (Aliens.Settings.mFutureSim)
                    baby.TraitManager.AddHiddenElement(TraitNames.FutureSim);

                baby.CelebrityManager.SetBabyLevel(Genetics.AssignBabyCelebrityLevel(null, abductee));
                abductee.Genealogy.AddChild(baby.Genealogy);

                if (alien != null)
                    alien.Genealogy.AddChild(baby.Genealogy);
            }

            return baby;
        }

        public void OnWorldLoadFinished()
        {
            if (GameUtils.GetCurrentWorldType() != WorldType.Vacation)
            {
                ResetAlienRefreshAlarm();
                ResetAlienActivityAlarm();
            }
        }

        public void OnWorldQuit()
        {
            AlienUtils.Shutdown();

            for (int i = 0; i < 24; i++)
            {
                AlarmManager.Global.RemoveAlarm(sAlienActivityAlarm[i]);
                sAlienActivityAlarm[i] = AlarmHandle.kInvalidHandle;
            }            
        }

        private static void ResetAbductionHelper()
        {
            AlienUtils.sAlienAbductionHelper.SpaceRocksFound = 0;
            AlienUtils.sAlienAbductionHelper.TelescopeUsed = false;
        }

        private void ResetAlienActivityAlarm()
        {
            if (AlienUtils.sAlienVisitationAlarm != AlarmHandle.kInvalidHandle)
            {
                AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienVisitationAlarm);
                AlienUtils.sAlienVisitationAlarm = AlarmHandle.kInvalidHandle;
            }

            for (int hour = 0; hour < 24; hour++)
            {
                /*  Uncomment this block if there are issues with the alarms
                if (sAlienActivityAlarm[hour] != AlarmHandle.kInvalidHandle)
                {
                    AlarmManager.Global.RemoveAlarm(sAlienActivityAlarm[hour]);
                    sAlienActivityAlarm[hour] = AlarmHandle.kInvalidHandle;
                }
                */

                sAlienActivityAlarm[hour] = AlarmManager.Global.AddAlarmDay((float)hour, DaysOfTheWeek.All, new AlarmTimerCallback(AlienActivityCallback),
                    "Alien Activity Hourly Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);
            }
        }

        private void ResetAlienRefreshAlarm()
        {
            if (AlienUtils.sAlienHouseholdRefreshAlarm != AlarmHandle.kInvalidHandle && !CheckAlarm(AlienUtils.sAlienHouseholdRefreshAlarm, refreshCallback))
            {
                AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienHouseholdRefreshAlarm);
                AlienUtils.sAlienHouseholdRefreshAlarm = AlarmHandle.kInvalidHandle;
            }

            AlienUtils.sAlienHouseholdRefreshAlarm = AlarmManager.Global.AddAlarmDay(15f, DaysOfTheWeek.All, new AlarmTimerCallback(AlienRefreshCallback),
                "Alien Household Refresh Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);
        }
    }
}
