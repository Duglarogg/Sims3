using NRaas.CommonSpace.Helpers;
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
using Sims3.Gameplay.Objects.Island;
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
 * 
 *  static AlienUtilsEx()
 *      - The alarm for alien activity may be converted to singular, hourly repeating alarm instead
 *        of 24 daily alarms (one for each hour of the day).
 *      
 *  CanASimBeAbucted(...)
 *      - Currently not referenced by anything; candidate for culling.
 *  
 *  MakeAlienDescendant(...)
 *      - Implement
 * 
 * <WISHLIST>
 * 
 *  AlienActivityCallback()
 *      - Allow for NPC abductions that don't trigger NRaasStoryProgression affair scenarios.
 */

namespace NRaas.AliensSpace.Helpers
{
    public class AlienUtilsEx : Common.IWorldLoadFinished, Common.IWorldQuit
    {
        static AlarmHandle[] sAlienActivityAlarm = new AlarmHandle[24];
        static AlarmHandle sReplaceAlarms = AlarmHandle.kInvalidHandle;
        static int cooldown = 0;
        static AlarmTimerCallback sActivityCallback = new AlarmTimerCallback(AlienActivityCallback);
        static AlarmTimerCallback sRefreshCallback = new AlarmTimerCallback(AlienRefreshCallback);

        static AlienUtilsEx()
        {
            for (int i = 0; i < 24; i++)
            {
                sAlienActivityAlarm[i] = AlarmHandle.kInvalidHandle;
            }
        }

        public static void AlienActivityCallback()
        {
            string msg = "Alien Activity Alarm" + Common.NewLine;

            if (cooldown > 0)
            {
                msg += " - On Cooldown: " + cooldown + " hours remaining";
                Common.DebugNotify(msg);

                cooldown -= 1;

                return;
            }

            if (SimClock.Hours24 >= Aliens.Settings.mEarliestHour 
                || SimClock.Hours24 <= (Aliens.Settings.mEarliestHour + Aliens.Settings.mActivityWindow) % 24)
            {
                if (Household.AlienHousehold == null || Household.AlienHousehold.NumMembers == 0)
                {
                    msg += " - Alien household null or empty";
                    Common.DebugNotify(msg);
                    return;
                }

                float chance = GetActivityChance();

                if (!RandomUtil.RandomChance(chance))
                {
                    msg += " - Activity Roll Fail (" + chance + "%)";
                    Common.DebugNotify(msg);
                    return;
                }

                msg += " - Activity Roll Pass (" + chance + "%)" + Common.NewLine;

                List<SimDescription> aliens = GetValidAliens();

                if (aliens == null)
                {
                    msg += " - No valid aliens";
                    Common.DebugNotify(msg);
                    return;
                }

                SimDescription alien = RandomUtil.GetRandomObjectFromList(aliens);
                chance = GetAbductionChance(true);

                if (RandomUtil.RandomChance(chance))
                {
                    msg += " - Active Abduction Roll Pass (" + chance +"%)" + Common.NewLine;

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
                        msg += " -- No valid abductees";
                        Common.DebugNotify(msg);
                        return;
                    }

                    Sim abductee = RandomUtil.GetRandomObjectFromList(validAbductees);
                    Lot lot = abductee.LotCurrent;

                    if (lot == null)
                    {
                        msg += " -- Abductee lot is null";
                        Common.DebugNotify(msg);
                        return;
                    }

                    msg += " -- Starting active abduction situation";
                    Common.DebugNotify(msg);

                    AlienAbductionSituationEx.Create(alien, abductee, lot);
                    cooldown = Aliens.Settings.mActivityCooldown;
                    ResetAbductionHelper();
                    return;
                }

                msg += " - Active Abduction Roll Fail (" + chance + "%)" + Common.NewLine;

                chance = GetAbductionChance();

                if (RandomUtil.RandomChance(chance))
                {
                    msg += " - NPC Abduction Roll Pass (" + chance + "%)" + Common.NewLine;

                    List<Lot> lots = GetValidLots(true);

                    if (lots == null)
                    {
                        msg += " - No valid lots";
                        Common.DebugNotify(msg);
                        return;
                    }

                    Lot lot = RandomUtil.GetRandomObjectFromList(lots);

                    if (lot == null)
                    {
                        msg += " - Lot is null";
                        Common.DebugNotify(msg);
                        return;
                    }

                    List<Sim> validAbductees = GetValidAbductees(lot);

                    if (validAbductees == null)
                    {
                        msg += " -- No valid abductees";
                        Common.DebugNotify(msg);
                        return;
                    }

                    msg += " -- Starting NPC abduction situation";
                    Common.DebugNotify(msg);

                    Sim abductee = RandomUtil.GetRandomObjectFromList(validAbductees);
                    AlienAbductionSituationEx.Create(alien, abductee, lot);
                    cooldown = Aliens.Settings.mActivityCooldown;
                    return;
                }

                msg += " - NPC Abduction Roll Fail (" + chance + "%)" + Common.NewLine;

                chance = GetVisitationChance(Household.ActiveHousehold, alien);

                if (RandomUtil.RandomChance(chance))
                {
                    msg += " - Visit Active Roll Pass (" + chance + "%)" + Common.NewLine +
                        " -- Starting active visit situation (" + Household.ActiveHousehold.LotHome.Name + ")";
                    Common.DebugNotify(msg);

                    Sim visitor = alien.InstantiateOffScreen(LotManager.GetFarthestLot(Household.ActiveHousehold.LotHome));
                    AlienSituation.Create(visitor, Household.ActiveHousehold.LotHome);
                    cooldown = Aliens.Settings.mActivityCooldown;
                    ResetAbductionHelper();
                }
                else
                {
                    msg += " - Visit Active Roll Fail (" + chance + "%)" + Common.NewLine;

                    List<Lot> lots = GetValidLots();

                    if (lots == null)
                    {
                        msg += " -- No valid lots";
                        Common.DebugNotify(msg);
                        return;
                    }

                    Lot lot = RandomUtil.GetRandomObjectFromList(lots);

                    msg += " -- Starting NPC visit situation (" + lot.Name + ")";
                    Common.DebugNotify(msg);

                    Sim visitor = alien.InstantiateOffScreen(LotManager.GetFarthestLot(lot));
                    AlienSituation.Create(visitor, lot);
                    cooldown = Aliens.Settings.mActivityCooldown;
                }
            }
            else
                msg += " - Outside active hours";

            Common.DebugNotify(msg);
        }

        public static void AlienRefreshCallback()
        {
            string msg = "Alien Household Refresh" + Common.NewLine;

            if (Household.AlienHousehold == null)
            {
                msg += " - Alien household is null";
                Common.DebugNotify(msg);
                return;
            }

            if (Household.AlienHousehold.NumMembers < AlienUtils.kAlienHouseholdNumMembers)
            {
                msg += " - Adding new alien" + Common.NewLine;

                CASAgeGenderFlags age = RandomUtil.GetRandomObjectFromList(Aliens.Settings.mValidAlienAges);
                CASAgeGenderFlags gender = RandomUtil.CoinFlip() ? CASAgeGenderFlags.Male : CASAgeGenderFlags.Female;
                SimDescription description = MakeAlien(age, gender, GameUtils.GetCurrentWorld(), 1f, true);

                if (Aliens.Settings.mAllowOccultAliens && RandomUtil.RandomChance(Aliens.Settings.mOccultAlienChance))
                {
                    msg += " -- Creating occult alien" + Common.NewLine;

                    int numOccults = RandomUtil.GetInt(1, Aliens.Settings.mMaxAlienOccults);
                    List<OccultTypes> validOccults = new List<OccultTypes>(Aliens.Settings.mValidAlienOccults);

                    for (int i = 0; i < numOccults; i++)
                    {
                        if (validOccults.Count == 0)
                            break;

                        OccultTypes type = RandomUtil.GetRandomObjectFromList(validOccults);

                        if (type != OccultTypes.Ghost)
                        {
                            OccultTypeHelper.Add(description, type, false, false);

                            msg += " --- " + OccultTypeHelper.GetLocalizedName(type) + Common.NewLine;
                        }
                        else
                        {
                            SimDescription.DeathType deathType =
                                RandomUtil.GetRandomObjectFromList((SimDescription.DeathType[])Enum.GetValues(typeof(SimDescription.DeathType)));
                            Urnstones.SimToPlayableGhost(description, deathType);

                            msg += " --- " + Urnstones.GetLocalizedString(description.IsFemale, deathType) + Common.NewLine;
                        }

                        validOccults.Remove(type);
                    }
                }

                msg += " -- Adding baseline skills" + Common.NewLine;

                Skill element = null;

                element = description.SkillManager.AddElement(SkillNames.Logic);

                if (element != null)
                    element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mLogicSkill[0], Aliens.Settings.mLogicSkill[1]));

                msg += " --- " + element.Name + Common.NewLine;

                element = description.SkillManager.AddElement(SkillNames.Handiness);

                if (element != null)
                    element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mHandinessSkill[0], Aliens.Settings.mHandinessSkill[1]));

                msg += " --- " + element.Name + Common.NewLine;

                try
                {
                    if (Aliens.Settings.mFutureSim)
                    {
                        msg += " -- Adding Adv Tech skill" + Common.NewLine;

                        description.TraitManager.AddElement(TraitNames.FutureSim);
                        element = description.SkillManager.AddElement(SkillNames.Future);

                        if (element != null)
                            element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mFutureSkill[0], Aliens.Settings.mFutureSkill[1]));
                    }

                }
                catch (Exception e)
                {
                    Common.Exception("AlienUtilsEx.AlienRefresh" + Common.NewLine + " - Failed to add Adv Tech skill", e);
                }

                /*
                if (age == CASAgeGenderFlags.Teen)
                {
                    element = description.SkillManager.AddElement(SkillNames.LearnToDrive);

                    if (element != null)
                        element.ForceSkillLevelUp(SkillManager.GetMaximumSupportedSkillLevel(SkillNames.LearnToDrive));
                }
                */

                try
                {
                    if (Aliens.Settings.mAlienScience)
                    {
                        msg += " -- Adding Science skill" + Common.NewLine;

                        //Sim temp = description.InstantiateOffScreen(LotManager.GetFarthestLot(Household.ActiveHouseholdLot));
                        element = description.SkillManager.AddElement(SkillNames.Science);

                        if (element != null)
                            element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mScienceSkill[0], Aliens.Settings.mScienceSkill[1]));

                        //temp.Destroy();
                    }

                }
                catch (Exception e)
                {
                    Common.Exception("AlienUtilsEx.AlienRefresh" + Common.NewLine + " - Failed to add Science skill", e);
                }

                try
                {
                    if (OccultTypeHelper.HasType(description, OccultTypes.Fairy) || OccultTypeHelper.HasType(description, OccultTypes.PlantSim))
                    {
                        msg += " -- Adding Gardening skill" + Common.NewLine;

                        element = description.SkillManager.AddElement(SkillNames.Gardening);

                        if (element != null)
                            element.ForceSkillLevelUp(RandomUtil.GetInt(3, 6));
                    }
                }
                catch (Exception e)
                {
                    Common.Exception("AlienUtilsEx.AlienRefresh" + Common.NewLine + " - Failed to add Gardening skill", e);
                }

                try
                {
                    if (OccultTypeHelper.HasType(description, OccultTypes.Fairy))
                    {
                        msg += " -- Adding Fairy Magic skill" + Common.NewLine;

                        element = description.SkillManager.AddElement(SkillNames.FairyMagic);

                        if (element != null)
                            element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mFairyMagicSkill[0], Aliens.Settings.mFairyMagicSkill[1]));
                    }
                }
                catch (Exception e)
                {
                    Common.Exception("AlienUtilsEx.AlienRefresh" + Common.NewLine + " - Failed to add Fairy Magic skill", e);
                }

                try
                {
                    if (OccultTypeHelper.HasType(description, OccultTypes.Werewolf))
                    {
                        msg += " -- Adding Lycanthropy skill" + Common.NewLine;

                        element = description.SkillManager.AddElement(SkillNames.Lycanthropy);

                        if (element != null)
                            element.ForceSkillLevelUp(RandomUtil.GetInt(Aliens.Settings.mLycanthropySkill[0], Aliens.Settings.mLycanthropySkill[1]));
                    }
                }
                catch (Exception e)
                {
                    Common.Exception("AlienUtilsEx.AlienRefresh" + Common.NewLine + " - Failed to add Lycanthropy skill", e);
                }

                try
                {
                    if (OccultTypeHelper.HasType(description, OccultTypes.Witch))
                    {
                        msg += " -- Adding witch skills" + Common.NewLine;

                        element = description.SkillManager.AddElement(SkillNames.Spellcasting);

                        if (element != null)
                            element.ForceSkillLevelUp(RandomUtil.GetInt(3, 6));

                        element = description.SkillManager.AddElement(SkillNames.Spellcraft);

                        if (element != null)
                            element.ForceSkillLevelUp(RandomUtil.GetInt(3, 6));
                    }
                }
                catch (Exception e)
                {
                    Common.Exception("AlienUtilsEx.AlienRefresh" + Common.NewLine + " - Failed to add witch skills", e);
                }

                msg += " -- Adding alien to household";

                Household.AlienHousehold.AddSilent(description);
                description.OnHouseholdChanged(Household.AlienHousehold, false);

                Common.DebugNotify(msg);
            }
        }

        public static void ApplyAlienFaceBlend(CASAgeGenderFlags gender, ref SimBuilder sb)
        {
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
        }

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

        private static float GetAbductionChance(bool isActive = false)
        {
            float result = Aliens.Settings.mBaseAbductionChance;

            if (result <= 0)
            {
                Common.DebugNotify("Alien Activity: Abductions disabled");
                return 0;
            }

            if (isActive)
            {
                if (AlienUtils.sAlienAbductionHelper.TelescopeUsed)
                    result += Aliens.Settings.mTelescopeBonus;

                if (AlienUtils.sAlienAbductionHelper.SpaceRocksFound > 0)
                    result += Math.Min(AlienUtils.sAlienAbductionHelper.SpaceRocksFound * Aliens.Settings.mSpaceRockFoundBonus,
                        Aliens.Settings.mMaxSpaceRockBonus);
            }

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

        private static SimDescription.DeathType GetGhostBabyType(SimDescription abductee, SimDescription alien)
        {
            if (abductee.DeathStyle != SimDescription.DeathType.None && alien.DeathStyle != SimDescription.DeathType.None)
            {
                if (RandomUtil.CoinFlip())
                    return abductee.DeathStyle;
                else
                    return alien.DeathStyle;
            }
            else if (abductee.DeathStyle != SimDescription.DeathType.None)
                return abductee.DeathStyle;
            else if (alien.DeathStyle != SimDescription.DeathType.None)
                return alien.DeathStyle;
            else
                return SimDescription.DeathType.None;
        }

        private static List<OccultTypes> GetSharedOccults(ref List<OccultTypes> abductee, ref List<OccultTypes> alien)
        {
            List<OccultTypes> types = new List<OccultTypes>();
            List<OccultTypes> valid = OccultTypeHelper.CreateListOfMissingOccults(new List<OccultTypes>()
                {
                    OccultTypes.Frankenstein,
                    OccultTypes.Mummy,
                    OccultTypes.Robot,
                    OccultTypes.Unicorn
                }, false);

            foreach(OccultTypes current in valid)
            {
                OccultTypes type = current;

                if (abductee.Contains(type) && alien.Contains(type))
                {
                    types.Add(type);
                    abductee.Remove(type);
                    alien.Remove(type);
                }
            }

            return types;
        }

        private static List<Sim> GetValidAbductees(Household household)
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

        public static List<Sim> GetValidAbductees(Lot lot)
        {
            List<Sim> list = new List<Sim>();

            foreach (Sim current in lot.GetSims())
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

        private static List<Lot> GetValidLots(bool forAbduction = false)
        {
            List<Lot> list = new List<Lot>();

            foreach (Lot lot in LotManager.AllLots)
            {
                if (!lot.IsWorldLot && lot.LotType != LotType.Tutorial && !lot.IsCommunityLotOfType(CommercialLotSubType.kMisc_NoVisitors)
                    && !lot.IsCommunityLotOfType(CommercialLotSubType.kEP10_Diving) && !UnchartedIslandMarker.IsUnchartedIsland(lot))
                {
                    if (lot.IsPlayerHomeLot)
                        continue;

                    if (lot.IsResidentialLot && lot.Household == null)
                        continue;

                    if (AlienUtils.IsHouseboatAndNotDocked(lot))
                        continue;

                    if (forAbduction && lot.GetAllActorsCount() <= 0)
                        continue;

                    list.Add(lot);
                }
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

        private static Color HSLToRGB(float h, float s, float l)
        {
            int r, g, b;
            float var1, var2;

            if (s == 0f)
            {
                r = (int)(l * 255f);
                g = (int)(l * 255f);
                b = (int)(l * 255f);
            }
            else
            {
                if (l < 0.5f)
                    var2 = l * (1f + s);
                else
                    var2 = (l + s) - (s * l);

                var1 = 2f * l - var2;

                r = (int)(255f * HueToRGB(var1, var2, h + (1f / 3f)));
                g = (int)(255f * HueToRGB(var1, var2, h));
                b = (int)(255f * HueToRGB(var1, var2, h - (1f / 3f)));
            }

            return new Color(r, g, b);
        }

        private static float HueToRGB(float var1, float var2, float h)
        {
            h = h % 1f;

            if ((6f * h) < 1f)
                return (var1 + (var2 - var1) * 6f * h);

            if ((2f * h) < 1f)
                return var2;

            if ((3f * h) < 2f)
                return (var1 + (var2 - var1) * ((2f / 3f) - h) * 6f);

            return var1;
        }

        private static SimDescription MakeAlien(CASAgeGenderFlags age, CASAgeGenderFlags gender, WorldName homeworld, float alienDNAPercentage, bool assignRandomTraits)
        {
            ResourceKey skinTone = new ResourceKey(0xb93c88cd44494517, 55867754u, 0u);
            float skinToneIndex = RandomUtil.GetFloat(1f);
            
            SimBuilder sb = new SimBuilder();
            sb.Age = age;
            sb.Gender = gender;
            sb.Species = CASAgeGenderFlags.Human;
            sb.SkinTone = skinTone;
            sb.SkinToneIndex = skinToneIndex;
            sb.TextureSize = 1024u;
            sb.UseCompression = true;
            ApplyAlienFaceBlend(gender, ref sb);
            float hue = (skinToneIndex + 0.5f) % 1f;
            float saturation = age == CASAgeGenderFlags.Elder ? 0.25f : 0.75f;
            Color[] colors = new Color[] { HSLToRGB(hue, saturation, 0.5f) };
            SimDescription alienDescription = Genetics.MakeSim(sb, age, gender, skinTone, skinToneIndex, colors, homeworld, 4294967295u, true);

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
            SimBuilder sb = new SimBuilder();
            sb.Age = CASAgeGenderFlags.Baby;
            sb.Gender = gender;
            sb.Species = CASAgeGenderFlags.Human;
            sb.SkinTone = alien.SkinToneKey;
            sb.SkinToneIndex = alien.SkinToneIndex;
            sb.TextureSize = 1024u;
            sb.UseCompression = true;
            ApplyAlienFaceBlend(gender, ref sb);
            float hue = (sb.SkinToneIndex + 0.5f) % 1f;
            Color[] colors = new Color[] { HSLToRGB(hue, 0.75f, 0.5f) };
            SimDescription baby = Genetics.MakeSim(sb, CASAgeGenderFlags.Baby, gender, alien.SkinToneKey, alien.SkinToneIndex, colors, 
                GameUtils.GetCurrentWorld(), 4294967295u, true);

            if (baby != null)
            {
                if (interactive)
                    baby.FirstName = string.Empty;
                else
                    baby.FirstName = SimUtils.GetRandomAlienGivenName(baby.IsMale);

                baby.LastName = abductee.LastName;
                Genetics.AssignTraits(baby, null, abductee, interactive, averageMood, pregoRandom);
                
                if (Aliens.Settings.mFutureSim)
                    baby.TraitManager.AddHiddenElement(TraitNames.FutureSim);

                if (Aliens.Settings.mAllowOccultBabies)
                {
                    List<OccultTypes> toInherit = OccultsToInherit(OccultTypeHelper.CreateList(abductee), OccultTypeHelper.CreateList(alien));

                    if (toInherit != null && toInherit.Count > 0)
                    {
                        for (int i = 0; i < toInherit.Count; i++)
                        {
                            if (toInherit[i] != OccultTypes.Ghost)
                                OccultTypeHelper.Add(baby, toInherit[i], false, false);
                            else
                            {
                                SimDescription.DeathType deathType = GetGhostBabyType(abductee, alien);
                                Urnstones.SimToPlayableGhost(baby, deathType);
                            }
                        }

                        if (OccultTypeHelper.HasType(baby, OccultTypes.Fairy))
                        {
                            CASFairyData casFairyData = baby.SupernaturalData as CASFairyData;

                            if (casFairyData != null)
                            {
                                Vector3 wingColor;
                                WingTypes wingType;
                                Genetics.InheritWings(baby, abductee, alien, pregoRandom, out wingColor, out wingType);
                                casFairyData.WingType = wingType;
                                casFairyData.WingColor = wingColor;
                            }
                        }
                    }
                    else if (RandomUtil.RandomChance01(abductee.Pregnancy.mChanceOfRandomOccultMutation))
                    {
                        OccultTypeHelper.Add(baby, Pregnancy.ChooseARandomOccultMutation(), false, false);
                    }
                }

                baby.CelebrityManager.SetBabyLevel(Genetics.AssignBabyCelebrityLevel(null, abductee));
                abductee.Genealogy.AddChild(baby.Genealogy);

                if (alien != null)
                    alien.Genealogy.AddChild(baby.Genealogy);
            }

            return baby;
        }

        // <NOTE> For use with NRaasWoohooer's Pregnancy proxy and NRaasTraveler descendant generation so that new skin tones 
        //  are passed down instead of the standard grey-green.
        public static SimDescription MakeAlienDescendant()
        {
            return null;
        }

        private static List<OccultTypes> OccultsToInherit(List<OccultTypes> abductee, List<OccultTypes> alien)
        {
            if (abductee.Count == 0 && alien.Count == 0)
                return null;

            List<OccultTypes> list = new List<OccultTypes>();
            int numOccults = Aliens.Settings.mMaxBabyOccults;
            List<OccultTypes> shared = GetSharedOccults(ref abductee, ref alien);

            if (shared.Count == numOccults)
                return shared;

            else if (shared.Count > numOccults)
            {
                for (int i = 0; i < numOccults; i++)
                {
                    OccultTypes type = RandomUtil.GetRandomObjectFromList(shared);
                    list.Add(type);
                    shared.Remove(type);
                }

                return list;
            }

            for (int i = 0; i < shared.Count; i++)
                list.Add(shared[i]);

            if (abductee.Count > 0 || alien.Count > 0)
            {
                List<OccultTypes> pool = new List<OccultTypes>();

                foreach (OccultTypes current in abductee)
                    pool.Add(current);

                foreach (OccultTypes current2 in alien)
                    pool.Add(current2);

                int toGo = numOccults - shared.Count;

                for (int i = 0; i < toGo; i++)
                {
                    if (pool.Count == 0)
                        break;

                    OccultTypes type = RandomUtil.GetRandomObjectFromList(pool);
                    list.Add(type);
                    pool.Remove(type);
                }
            }

            return list;
        }

        public void OnWorldLoadFinished()
        {
            Common.DebugNotify("AlienUtilsEx.OnWorldLoadFinished" + Common.NewLine +
                " - Creating Replacement Alarm");

            if (GameUtils.GetCurrentWorldType() != WorldType.Vacation)
            {
                if (AlienUtils.sAlienHouseholdRefreshAlarm != AlarmHandle.kInvalidHandle)
                {
                    AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienHouseholdRefreshAlarm);
                    AlienUtils.sAlienHouseholdRefreshAlarm = AlarmHandle.kInvalidHandle;
                }

                AlienUtils.sAlienHouseholdRefreshAlarm = AlarmManager.Global.AddAlarmDay(15f, DaysOfTheWeek.All,
                    new AlarmTimerCallback(AlienRefreshCallback), "Alien Household Refresh Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);

                for (int i = 0; i < 24; i++)
                {
                    if (sAlienActivityAlarm[i] != AlarmHandle.kInvalidHandle)
                    {
                        AlarmManager.Global.RemoveAlarm(sAlienActivityAlarm[i]);
                        sAlienActivityAlarm[i] = AlarmHandle.kInvalidHandle;
                    }

                    sAlienActivityAlarm[i] = AlarmManager.Global.AddAlarmDay(i, DaysOfTheWeek.All, new AlarmTimerCallback(AlienActivityCallback),
                        "Alien Actiivty Ex Alarm", AlarmType.NeverPersisted, Household.AlienHousehold);
                }
            }
        }

        public void OnWorldQuit()
        {
            AlarmManager.Global.RemoveAlarm(AlienUtils.sAlienHouseholdRefreshAlarm);
            AlienUtils.sAlienHouseholdRefreshAlarm = AlarmHandle.kInvalidHandle;

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
    }
}
