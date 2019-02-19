using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.ScoringMethods;
using NRaas.WoohooerSpace.Interactions;
using NRaas.WoohooerSpace.Proxies;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.CelebritySystem;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.StoryProgression;
using Sims3.Gameplay.ThoughtBalloons;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.CAS;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace NRaas.WoohooerSpace.Helpers
{
    public class CommonSurrogatePregnancy
    {
        static Common.MethodStore sStoryProgressionAllowImpregnation = new Common.MethodStore("NRaasStoryProgression", "NRaas.StoryProgression", "AllowImpregnation", new Type[] { typeof(SimDescription), typeof(bool) });
        static Common.MethodStore sStoryProgressionAllowPregnancy = new Common.MethodStore("NRaasStoryProgression", "NRaas.StoryProgression", "AllowPregnancy", new Type[] { typeof(SimDescription), typeof(SimDescription), typeof(bool) });

        public delegate float GetChanceOfSuccess(Sim a, Sim b, bool isAutonomous, CommonWoohoo.WoohooStyle style);
        public delegate bool SelectionDelegate(IGameObject toTest);

        public static GetChanceOfSuccess sGetChanceOfSuccess = OnGetChanceOfSuccess;

        private static bool CanGetPreggers(Sim sim, bool isAutonomous, ref GreyedOutTooltipCallback callback, out string reason)
        {
            using (Common.TestSpan span = new Common.TestSpan(ScoringLookup.Stats, "Duration CanGetPreggers", Common.DebugLevel.Stats))
            {
                if (SimTypes.IsPassporter(sim.SimDescription))
                {
                    reason = "Passport";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }

                if (isAutonomous)
                {
                    if (!sim.IsRobot && sim.SimDescription.Elder)
                    {
                        reason = "Elder";
                        callback = Common.DebugTooltip(reason);
                        return false;
                    }
                    else if (Households.IsFull(sim.Household, sim.IsPet, Woohooer.Settings.mMaximumHouseholdSizeForAutonomousV2[PersistedSettings.GetSpeciesIndex(sim)]))
                    {
                        reason = "House Full";
                        callback = Common.DebugTooltip(reason);
                        return false;
                    }
                }
                else
                {
                    if (!CommonPregnancy.SatisfiesMaximumOccupants(sim, isAutonomous, ref callback))
                    {
                        reason = "Maximum Occupants";
                        return false;
                    }
                }

                if (!sim.IsRobot && SimTypes.IsSkinJob(sim.SimDescription))
                {
                    reason = "Non-Robot Skin Job Fail";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }
                else if (sim.BuffManager.HasTransformBuff())
                {
                    reason = "Transform Buff";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }
                else if (!sim.IsRobot && sim.SimDescription.IsVisuallyPregnant)
                {
                    reason = "Already Pregnant";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }
                else if (sim.IsRobot && sim.SimDescription.IsPregnant)
                {
                    reason = "Already Pregnant";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }

                if (sim.Household != null && sim.Household.IsTouristHousehold)
                {
                    MiniSimDescription description = MiniSimDescription.Find(sim.SimDescription.SimDescriptionId);

                    if (description == null)
                    {
                        reason = "Too Many Sims";
                        callback = delegate
                            {
                                return Common.LocalizeEAString(sim.IsFemale, "Gameplay/Actors/Sim/TryForBaby:TooManySims", new object[] { sim });
                            };
                        return false;
                    }
                }
                else if (sim.LotHome == null)
                {
                    reason = "Too Many Sims";
                    callback = delegate
                        {
                            if (sim.Household.IsAlienHousehold)
                                return Common.LocalizeEAString(sim.IsFemale, "Gameplay/Actors/Sim/TryForBaby:AlienNPCs", new object[] { sim });
                            else
                                return Common.LocalizeEAString(sim.IsFemale, "Gameplay/Actors/Sim/TryForBaby:TooManySims", new object[] { sim });
                        };
                    return false;
                }
                else if (sim.SimDescription.IsDueToAgeUp() || (sim.SimDescription.AgingState != null && sim.SimDescription.AgingState.IsAgingInProgress()))
                {
                    reason = "Aging Up Fail";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }
                else if (SimTypes.IsLampGenie(sim.SimDescription))
                {
                    reason = "Lamp Genie";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }

                if (sStoryProgressionAllowImpregnation.Valid && Woohooer.Settings.TestStoryProgression(isAutonomous))
                {
                    reason = sStoryProgressionAllowImpregnation.Invoke<string>(new object[] { sim.SimDescription, isAutonomous });

                    if (reason != null)
                    {
                        callback = Woohooer.StoryProgressionTooltip(reason, false);
                        return false;
                    }
                }

                reason = null;

                return true;
            }
        }

        public static bool CanImpregnate(Sim simA, Sim simB, ref GreyedOutTooltipCallback callback)
        {
            if (simA.SimDescription.Gender == simB.SimDescription.Gender)
            {
                callback = Common.DebugTooltip("Surrogate: Same Sex Impregnation Not Allowed (Yet)");
                return false;
            }

            if (!simA.IsRobot && !simB.IsRobot)
            {
                callback = Common.DebugTooltip("Don't you meatbags have your own way to reproduce?");
                return false;
            }

            if (simA.IsRobot && !HasDNASample(simA, simA.SimDescription.Gender))
            {
                callback = Common.DebugTooltip("Robot A Has No DNA");
                return false;
            }

            if (simB.IsRobot && !HasDNASample(simB, simB.SimDescription.Gender))
            {
                callback = Common.DebugTooltip("Robot B Has No DNA");
                return false;
            }

            return true;
        }

        public static bool CanTryForBaby(Sim actor, Sim target, bool autonomous, CommonWoohoo.WoohooStyle style, ref GreyedOutTooltipCallback callback, out string reason)
        {
            using (Common.TestSpan span = new Common.TestSpan(ScoringLookup.Stats, "Duration CanTryForBaby", Common.DebugLevel.Stats))
            {
                int chance = 0;
                bool teenCanTry = false;
                int speciesIndex = PersistedSettings.GetSpeciesIndex(actor);

                switch (style)
                {
                    case CommonWoohoo.WoohooStyle.Risky:
                    case CommonWoohoo.WoohooStyle.TryForBaby:
                        if (actor.SimDescription.Teen || target.SimDescription.Teen)
                            chance = Woohooer.Settings.mTryForBabyTeenBabyMadeChance;
                        else
                            chance = Woohooer.Settings.mTryForBabyMadeChanceV2[speciesIndex];

                        teenCanTry = Woohooer.Settings.mTeenTryForBabyAutonomous;
                        break;
                }

                if (chance <= 0)
                {
                    reason = "Chance Fail";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }

                if (!CommonSocials.CanGetRomantic(actor, target, autonomous, true, true, ref callback, out reason))
                    return false;

                if (autonomous || Woohooer.Settings.TestStoryProgression(autonomous))
                {
                    if (sStoryProgressionAllowPregnancy.Valid && Woohooer.Settings.TestStoryProgression(autonomous))
                    {
                        if (actor.SimDescription.Gender == target.SimDescription.Gender)
                        {
                            reason = "Surrogate: Same Sex Fail";
                            callback = Common.DebugTooltip(reason);
                            return false;
                        }
                        else
                        {
                            SimDescription male, female;

                            if (actor.IsFemale)
                            {
                                female = actor.SimDescription;
                                male = target.SimDescription;
                            }
                            else
                            {
                                male = actor.SimDescription;
                                female = target.SimDescription;
                            }

                            reason = sStoryProgressionAllowPregnancy.Invoke<string>(new object[] { female, male, autonomous });

                            if (reason != null)
                            {
                                callback = Woohooer.StoryProgressionTooltip(reason, false);
                                return false;
                            }


                        }
                    }
                }

                if (actor.SimDescription.Gender == target.SimDescription.Gender)
                {
                    reason = "Surrogate: Same Sex Fail";
                    callback = Common.DebugTooltip(reason);
                    return false;
                }
                else
                {
                    if (actor.IsFemale && !CanGetPreggers(actor, autonomous, ref callback, out reason))
                        return false;
                    else if (!CanGetPreggers(target, autonomous, ref callback, out reason))
                        return false;
                }

                if (autonomous || Woohooer.Settings.mTestAllConditionsForUserDirected[speciesIndex])
                {
                    if (HasBlockingBuff(actor))
                    {
                        reason = "Actor Buff Block";
                        callback = delegate { return Common.Localize("TryForBaby:BuffBlock"); };
                        return false;
                    }

                    if (HasBlockingBuff(target))
                    {
                        reason = "Target Buff Block";
                        callback = delegate { return Common.Localize("TryForBaby:BuffBlock"); };
                        return false;
                    }

                    if (autonomous)
                    {
                        if (actor.SimDescription.IsPregnant || target.SimDescription.IsPregnant)
                        {
                            reason = "Pregnant";
                            callback = delegate { return Common.Localize("TryForBaby:Pregnant"); };
                            return false;
                        }
                    }
                    else
                    {
                        if ((actor.IsRobot && actor.SimDescription.IsPregnant) || actor.SimDescription.IsVisuallyPregnant)
                        {
                            reason = "Pregnant";
                            callback = delegate { return Common.Localize("TryForBaby:Pregnant"); };
                            return false;
                        }

                        if ((target.IsRobot && target.SimDescription.IsPregnant) || target.SimDescription.IsVisuallyPregnant)

                        if (actor.SimDescription.IsVisuallyPregnant || target.SimDescription.IsVisuallyPregnant)
                        {
                            reason = "Pregnant";
                            callback = delegate { return Common.Localize("TryForBaby:Pregnant"); };
                            return false;
                        }
                    }
                }

                if (actor.IsFemale && !actor.IsRobot && actor.SimDescription.Elder)
                {
                    reason = "Elder";
                    callback = delegate { return Common.Localize("TryForBaby:Elder"); };
                    return false;
                }

                if (target.IsFemale && !target.IsRobot && target.SimDescription.Elder)
                {
                    reason = "Elder";
                    callback = delegate { return Common.Localize("TryForBaby:Elder"); };
                    return false;
                }

                if (actor.SimDescription.Teen || target.SimDescription.Teen)
                {
                    if (!teenCanTry && autonomous)
                    {
                        reason = "Teenagers";
                        callback = delegate { return Common.LocalizeEAString("NRaas.Woohooer:Teenagers"); };
                        return false;
                    }
                }

                if ((!actor.IsRobot && SimTypes.IsSkinJob(actor.SimDescription)) || (!target.IsRobot && SimTypes.IsSkinJob(target.SimDescription)))
                {
                    reason = "Skin Job";
                    callback = delegate { return Common.Localize("TryForBaby:SkinJob"); };
                    return false;
                }

                return true;
            }
        }

        private static void ChooseDNASamples(Sim actor, Sim target, ref ScientificSample maleDNA, ref ScientificSample femaleDNA)
        {
            if (actor.IsRobot)
            {
                if (actor.IsFemale)
                    femaleDNA = SelectDNASample(actor, CASAgeGenderFlags.Female);
                else
                    maleDNA = SelectDNASample(actor, CASAgeGenderFlags.Male);
            }

            if (target.IsRobot)
            {
                if (target.IsFemale)
                    femaleDNA = SelectDNASample(target, CASAgeGenderFlags.Female);
                else
                    maleDNA = SelectDNASample(target, CASAgeGenderFlags.Male);
            }
        }

        private static List<ScientificSample> GetDNASamples(Sim robot, CASAgeGenderFlags gender)
        {
            if (robot.Inventory == null)
                return null;

            List<InventoryStack> stacks = new List<InventoryStack>();

            foreach (InventoryStack current in robot.Inventory.InventoryItems.Values)
            {
                if (current != null && new SelectionDelegate(IsObjectSample)(current.List[0].Object))
                {
                    ScientificSample scientificSample = current.List[0].Object as ScientificSample;

                    if (scientificSample.ScientificSampleType == ScientificSample.SampleType.Dna)
                        stacks.Add(current);
                }
            }

            List<ScientificSample> results = new List<ScientificSample>();

            foreach (InventoryStack current2 in stacks)
            {
                foreach (InventoryItem current3 in current2.List)
                {
                    ScientificSample scientificSample2 = current3.Object as ScientificSample;

                    if (scientificSample2 != null)
                    {
                        ScientificSample.DnaSampleSubject subject = scientificSample2.Subject as ScientificSample.DnaSampleSubject;

                        if (subject != null && subject.Subject.IsHuman && subject.Subject.Gender == gender)
                            results.Add(scientificSample2);
                    }
                }
            }

            return results;
        }

        public static void GetDNASamples(Sim actor, Sim target, ref ScientificSample maleDNA, ref ScientificSample femaleDNA)
        {
            if (actor.IsSelectable || target.IsSelectable)
                ChooseDNASamples(actor, target, ref maleDNA, ref femaleDNA);
            else
                RandomDNASmaples(actor, target, ref maleDNA, ref femaleDNA);
        }

        private static bool HasBlockingBuff(Sim sim)
        {
            if (sim == null)
                return false;

            if (sim.BuffManager == null)
                return false;

            return sim.BuffManager.HasAnyElement(new BuffNames[] { BuffNames.ItsABoy, BuffNames.ItsABoy, BuffNames.ItsTwins,
                BuffNames.ItsTriplets, CommonPregnancy.sItsQuadruplets });
        }

        public static bool HasDNASample(Sim robot, CASAgeGenderFlags gender)
        {
            if (robot.Inventory == null)
                return false;

            List<ScientificSample> samples = GetDNASamples(robot, gender);

            return samples != null && samples.Count > 0;
        }

        public static Pregnancy Impregnate(Sim simA, Sim simB, ScientificSample dnaF, ScientificSample dnaM, bool isAutonomous, CommonWoohoo.WoohooStyle style)
        {
            if (simA == null || simB == null)
            {
                Common.DebugNotify("CommonSurrogatePregnancy.Impregnate" + Common.NewLine + " - Impregnate Fail: Parent is Null");
                return null;
            }

            if (simA.SimDescription.Gender == simB.SimDescription.Gender)
            {
                Common.DebugNotify("CommonSurrogatePregnancy.Impregnate" + Common.NewLine + " - Impregante Fail: Same Sex");
                return null;
            }

            if (dnaF == null && dnaM == null)
            {
                Common.DebugNotify("CommonSurrogatePregnancy.Impregnate" + Common.NewLine + " - Impregnate Fail: DNA are Null");
                return null;
            }

            Sim mother, father;

            if (simA.IsFemale)
            {
                mother = simA;
                father = simB;
            }
            else
            {
                mother = simB;
                father = simA;
            }

            if (mother.IsRobot)
                return ImpregnateRobot(mother, father, dnaF, dnaM, isAutonomous, style);
            else
                return ImpregnateHuman(mother, father, dnaM, isAutonomous, style);
        }

        // For 1 robot and 1 human
        private static Pregnancy ImpregnateHuman(Sim mother, Sim father, ScientificSample dna, bool isAutonomous, CommonWoohoo.WoohooStyle style)
        {
            ScientificSample.DnaSampleSubject dnaSubject = dna.Subject as ScientificSample.DnaSampleSubject;

            if (dnaSubject != null)
                return StartPregnancy(mother, father, dnaSubject.Subject, isAutonomous, true);
            else
            {
                Common.DebugNotify("Surrogate Pregnancy: Male DNA Null", father);
                return null;
            }
        }

        // For 2 robots
        private static Pregnancy ImpregnateRobot(Sim mother, Sim father, ScientificSample dnaF, ScientificSample dnaM, bool isAutonomous, CommonWoohoo.WoohooStyle style)
        {
            ScientificSample.DnaSampleSubject femaleSubject = dnaF.Subject as ScientificSample.DnaSampleSubject;
            ScientificSample.DnaSampleSubject maleSubject = dnaM.Subject as ScientificSample.DnaSampleSubject;

            if (femaleSubject != null)
            {
                if (maleSubject != null)
                    return StartPregnancy(mother, father, femaleSubject.Subject, maleSubject.Subject, isAutonomous, true);
                else
                {
                    if (father.IsRobot)
                    {
                        Common.DebugNotify("Surrogate Pregnancy: Male DNA Null", father);
                        return null;
                    }
                    else
                        return StartPregnancy(mother, father, femaleSubject.Subject, null, isAutonomous, true);
                }
            }
            else
            {
                Common.DebugNotify("Surrogate Pregnancy: Female DNA Null", mother);
                return null;
            }
        }

        public static bool IsObjectSample(IGameObject toTest)
        {
            ScientificSample scientificSample = toTest as ScientificSample;
            return scientificSample != null;
        }

        public static bool IsSuccess(Sim simA, Sim simB, bool isAutonomous, CommonWoohoo.WoohooStyle style)
        {
            string reason;
            GreyedOutTooltipCallback callback = null;

            if (!CanTryForBaby(simA, simB, isAutonomous, style, ref callback, out reason))
            {
                if (callback != null)
                    Common.DebugNotify("Surrogate: " + callback(), simA, simB);

                return false;
            }

            if (simA.IsFemale && !simA.IsRobot && simA.BuffManager != null && simA.BuffManager.HasElement(PregnancyTestBuffs.sContraceptive))
            {
                Common.DebugNotify("Surrogate: Contraception Fail", simA, simB);
                return false;
            }

            if (simB.IsFemale && !simB.IsRobot && simB.BuffManager != null && simB.BuffManager.HasElement(PregnancyTestBuffs.sContraceptive))
            {
                Common.DebugNotify("Surrogate: Contraception Fail", simA, simB);
                return false;
            }

            if (simA.IsFemale && !simA.IsRobot)
            {
                // Add pregnancy test enabling moodlet here
            }

            if (simB.IsFemale && !simB.IsRobot)
            {
                // Add pregnancy test enabling moodlet here
            }

            float chance = sGetChanceOfSuccess(simA, simB, isAutonomous, style);

            if (!RandomUtil.RandomChance(chance))
            {
                Common.DebugNotify("Surrogate: Chance Fail " + chance, simA, simB);
                return false;
            }

            Common.DebugNotify("Surrogate: Chance Success " + chance, simA, simB);
            return true;
        }

        public static SimDescription MakeDescendant(SimDescription dad, SimDescription mom, SimDescription robotDad, SimDescription robotMom,
            CASAgeGenderFlags age, CASAgeGenderFlags gender, float averageMood, Random pregoRandom, bool interactive, bool updateGenealogy,
            bool setName, WorldName homeWorld, bool plantSimBaby)
        {
            return MakeDescendant(dad, mom, robotDad, robotMom, age, gender, averageMood, pregoRandom, interactive, updateGenealogy, setName,
                homeWorld, plantSimBaby, new Pregnancy.BabyCustomizeData { IsBabyCustomized = false });
        }

        public static SimDescription MakeDescendant(SimDescription dad, SimDescription mom, SimDescription robotDad, SimDescription robotMom, 
            CASAgeGenderFlags age, CASAgeGenderFlags gender, float averageMood, Random pregoRandom, bool interactive, bool updateGenealogy,
            bool setName, WorldName homeWorld, bool plantSimBaby, Pregnancy.BabyCustomizeData customizeData)
        {
            if (robotDad == null && robotMom == null)
            {
                Common.DebugNotify("CommonSurrogatePregnancy.MakeDescendant" + Common.NewLine + " - Fail: Robot Parnets Null");
                return null;
            }

            SimBuilder simBuilder = new SimBuilder();
            simBuilder.Age = age;
            simBuilder.Gender = gender;
            simBuilder.Species = CASAgeGenderFlags.Human;
            simBuilder.TextureSize = 1024u;
            simBuilder.UseCompression = true;
            List<SimDescription> list = new List<SimDescription>();

            if (mom != null)
                list.Add(mom);

            if (dad != null)
                list.Add(dad);

            float alienDNAPercentage = SimDescription.GetAlienDNAPercentage(dad, mom, true);
            SimDescription[] array = list.ToArray();
            bool flag = alienDNAPercentage >= SimDescription.kMinAlienDNAPercentToBeAlien;

            if (plantSimBaby)
            {
                float skinToneIndex = OccultPlantSim.kBaseGreenSkinIndex + RandomUtil.GetFloat(1f - OccultPlantSim.kBaseGreenSkinIndex);
                simBuilder.SkinTone = new ResourceKey(2751605866008866797uL, 55867754u, 0u);
                simBuilder.SkinToneIndex = skinToneIndex;
            }
            else
                Genetics.InheritSkinColor(simBuilder, array, pregoRandom, homeWorld);

            if (customizeData.IsBabyCustomized)
            {
                CASPart mPart = customizeData.EyeColorPreset.mPart;

                if (simBuilder.AddPart(mPart))
                {
                    string mPresetString = customizeData.EyeColorPreset.mPresetString;

                    if (!string.IsNullOrEmpty(mPresetString))
                        OutfitUtils.ApplyPresetStringToPart(simBuilder, mPart, mPresetString);

                }
            }
            else
                Genetics.InheritEyeColor(simBuilder, array, pregoRandom);

            Genetics.InheritFacialBlends(simBuilder, array, pregoRandom);
            ResourceKey geneticHairstyleKey = Genetics.InheritHairStyle(gender, dad, mom, pregoRandom, flag);
            Genetics.InheritBodyShape(simBuilder, array, pregoRandom);
            bool flag2 = pregoRandom.Next(0, 2) == 0;
            float num = (float)pregoRandom.NextDouble() * 100f;
            Color[] array2;

            if (customizeData.IsBabyCustomized)
            {
                array2 = new Color[10];

                for (int i = 0; i < 4; i++)
                {
                    array2[i] = customizeData.HairColor[i];
                    array2[i + 5] = customizeData.HairColor[i];
                }

                array2[4] = customizeData.HairColor[0];
                array2[9] = customizeData.HairColor[0];
            }
            else if (age == CASAgeGenderFlags.Elder)
                array2 = Genetics.GetRandomElderHairColor();
            else
                array2 = Genetics.InheritHairColor(simBuilder, array, pregoRandom);

            SimDescription baby = Genetics.MakeSim(simBuilder, CASAgeGenderFlags.Baby, gender, simBuilder.SkinTone, simBuilder.SkinToneIndex, array2, homeWorld, 4294967295u, false);

            if (baby == null)
                return null;

            baby.SetAlienDNAPercentage(alienDNAPercentage);
            baby.GeneticHairstyleKey = geneticHairstyleKey;
            bool flag3 = false;

            if (num < Genetics.kInheritMomHiddenBodyHairStyleChance)
                flag3 |= Genetics.InheritBodyHairstyle(baby, mom);

            if (!flag3 || num < Genetics.kInheritDadBodyHairStyleChance + Genetics.kInheritMomHiddenBodyHairStyleChance)
                Genetics.InheritBodyHairstyle(baby, dad);

            if (customizeData.IsBabyCustomized)
            {
                Genetics.TraitOutcome traitOutcome = Genetics.AssignTraits(baby, dad, mom, false, averageMood, pregoRandom);
                List<TraitNames> list2 = new List<TraitNames>();
                List<Trait> list3 = new List<Trait>();

                foreach (Trait current in baby.TraitManager.List)
                {
                    if (!current.IsVisible)
                        list3.Add(current);
                }

                baby.TraitManager.RemoveAllElements();

                if (customizeData.CurrentTraits != null)
                {
                    foreach (ITraitEntryInfo current2 in customizeData.CurrentTraits)
                        baby.TraitManager.AddElement((TraitNames)current2.TraitGuid);

                    if (customizeData.CurrentTraits.Count != 2)
                        baby.TraitManager.AddRandomTrait(2 - customizeData.CurrentTraits.Count);

                    foreach (Trait current3 in list3)
                        baby.TraitManager.AddHiddenElement((TraitNames)current3.TraitGuid);
                }

                foreach (Trait current4 in baby.TraitManager.List)
                {
                    if (current4.IsVisible)
                        list2.Add((TraitNames)current4.TraitGuid);
                }

                if (interactive)
                {
                    bool isFemale = baby.IsFemale;
                    string titleText = Localization.LocalizeString(baby.IsFemale, "Gameplay/CAS/Genetics:MakeBabyTitle", new object[0]);
                    string promptText = null;

                    switch (traitOutcome)
                    {
                        case Genetics.TraitOutcome.Horrible:
                            {
                                string entryKey = "Gameplay/CAS/Genetics:MakeBabyDescTwoTraitsHorrible";
                                promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                    {
                                        robotMom == null ? mom : robotMom,
                                        baby.TraitManager.GetElement((ulong)list2[0]).TraitName(baby.IsFemale),
                                        baby.TraitManager.GetElement((ulong)list2[1]).TraitName(baby.IsFemale)
                                    });
                                break;
                            }

                        case Genetics.TraitOutcome.Bad:
                            {
                                string entryKey = "Gameplay/CAS/Genetics:MakeBabyDescTwoTraitsBad";
                                promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                    {
                                        robotMom == null ? mom : robotMom,
                                        baby.TraitManager.GetElement((ulong)list2[0]).TraitName(baby.IsFemale),
                                        baby.TraitManager.GetElement((ulong)list2[1]).TraitName(baby.IsFemale)
                                    });
                                break;
                            }

                        case Genetics.TraitOutcome.Average:
                            {
                                string entryKey = "Gameplay/CAS/Genetics:MakeBabyDescTwoTraits";
                                promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                    {
                                        robotMom == null ? mom : robotMom,
                                        baby.TraitManager.GetElement((ulong)list2[0]).TraitName(baby.IsFemale),
                                        baby.TraitManager.GetElement((ulong)list2[1]).TraitName(baby.IsFemale)
                                    });
                                break;
                            }

                        case Genetics.TraitOutcome.Good:
                            {
                                string entryKey = "Gameplay/CAS/Genetics:MakeBabyDescOneTraits";
                                promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                    {
                                        robotMom == null ? mom : robotMom,
                                        baby.TraitManager.GetElement((ulong)list2[0]).TraitName(baby.IsFemale)
                                    });
                                break;
                            }

                        case Genetics.TraitOutcome.Excellent:
                            {
                                string entryKey = "Gameplay/CAS/Genetics:MakeBabyDesc";
                                promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                    {
                                        robotMom == null ? mom : robotMom,
                                    });
                                break;
                            }
                    }

                    while (string.IsNullOrEmpty(baby.FirstName))
                        baby.FirstName = StringInputDialog.Show(titleText, promptText, "", CASBasics.GetMaxNameLength(), StringInputDialog.Validation.SimNameText);
                }
            }
            else
                Genetics.AssignTraits(baby, dad, mom, interactive, averageMood, pregoRandom);

            if (setName)
            {
                baby.LastName = array[0].LastName;

                if (!interactive)
                    baby.FirstName = SimUtils.GetRandomGivenName(baby.IsMale, homeWorld);
            }

            baby.CelebrityManager.SetBabyLevel(Genetics.AssignBabyCelebrityLevel(dad, mom));

            if (updateGenealogy)
            {
                if (robotDad != null && robotMom == null)
                {
                    if (robotDad.Genealogy != null)
                        robotDad.Genealogy.AddChild(baby.Genealogy);

                    if (mom != null && mom.Genealogy != null)
                        mom.Genealogy.AddChild(baby.Genealogy);
                }
                else if (robotMom != null && robotDad == null)
                {
                    if (robotMom.Genealogy != null)
                        robotMom.Genealogy.AddChild(baby.Genealogy);

                    if (dad != null && dad.Genealogy != null)
                        dad.Genealogy.AddChild(baby.Genealogy);
                }
                else
                {
                    if (robotDad.Genealogy != null)
                        robotDad.Genealogy.AddChild(baby.Genealogy);

                    if (robotMom.Genealogy != null)
                        robotMom.Genealogy.AddChild(baby.Genealogy);
                }
            }

            OccultTypes occultTypes = OccultTypes.None;

            if (flag2)
            {
                if (mom != null)
                {
                    CASSupernaturalData supernaturalData = mom.SupernaturalData;

                    if (supernaturalData != null)
                        occultTypes = supernaturalData.OccultType;
                }
            }
            else if (dad != null)
            {
                CASSupernaturalData supernaturalData2 = dad.SupernaturalData;

                if (supernaturalData2 != null)
                    occultTypes = supernaturalData2.OccultType;
            }

            if (plantSimBaby)
                occultTypes = OccultTypes.PlantSim;

            if (!OccultManager.DoesOccultTransferToOffspring(occultTypes))
                occultTypes = OccultTypes.None;

            if (RandomUtil.CoinFlip())
            {
                if (occultTypes == OccultTypes.Fairy)
                {
                    baby.AddSupernaturalData(OccultTypes.Fairy);
                    CASFairyData fairyData = baby.SupernaturalData as CASFairyData;

                    if (fairyData != null)
                    {
                        Vector3 wingColor;
                        WingTypes wingType;
                        Genetics.InheritWings(baby, mom, dad, pregoRandom, out wingColor, out wingType);
                        fairyData.WingType = wingType;
                        fairyData.WingColor = wingColor;
                    }
                }

                if (occultTypes != OccultTypes.None)
                    baby.OccultManager.AddOccultType(occultTypes, false, false, false);
            }

            return baby;
        }

        public static float OnGetChanceOfSuccess(Sim simA, Sim simB, bool isAutonomous, CommonWoohoo.WoohooStyle style)
        {
            float chance = 0;
            bool useFertility = true;
            int speciesIndex = PersistedSettings.GetSpeciesIndex(simA);

            switch (style)
            {
                case CommonWoohoo.WoohooStyle.Risky:
                case CommonWoohoo.WoohooStyle.TryForBaby:
                    if (simA.SimDescription.Teen || simB.SimDescription.Teen)
                        chance = Woohooer.Settings.mTryForBabyTeenBabyMadeChance;
                    else
                        chance = Woohooer.Settings.mTryForBabyMadeChanceV2[speciesIndex];

                    useFertility = Woohooer.Settings.mTryForBabyFertility[speciesIndex];
                    break;
            }

            if (chance <= 0)
            {
                Common.DebugNotify("Surrogate: No Chance");
                return 0;
            }

            if (useFertility)
            {
                if (simA.IsHuman)
                {
                    if (simA.BuffManager != null && simA.BuffManager.HasTransformBuff())
                        return 0;

                    if (simB.BuffManager != null && simB.BuffManager.HasTransformBuff())
                        return 0;

                    if (simA.TraitManager.HasElement(TraitNames.FertilityTreatment)
                        || (simA.BuffManager != null && simA.BuffManager.HasElement(BuffNames.ATwinkleInTheEye)))
                        chance += TraitTuning.kFertilityBabyMakingChanceIncrease;

                    if (simB.TraitManager.HasElement(TraitNames.FertilityTreatment)
                        || (simB.BuffManager != null && simB.BuffManager.HasElement(BuffNames.ATwinkleInTheEye)))
                        chance += TraitTuning.kFertilityBabyMakingChanceIncrease;
                }
                else
                {
                    Common.DebugNotify("Surrogate: No Pets - How did you even get here?");
                }

                if (simA.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    chance += 100f;
                    simA.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                }

                if (simB.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    chance += 100f;
                    simB.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                }

                if (simA.BuffManager != null && simA.BuffManager.HasElement(BuffNames.MagicInTheAir))
                    chance += BuffMagicInTheAir.kBabyMakingChanceIncrease * 100f;

                if (simB.BuffManager != null && simB.BuffManager.HasElement(BuffNames.MagicInTheAir))
                    chance += BuffMagicInTheAir.kBabyMakingChanceIncrease * 100f;
            }

            return chance;
        }

        private static void RandomDNASmaples(Sim actor, Sim target, ref ScientificSample maleDNA, ref ScientificSample femaleDNA)
        {
            if (actor.IsRobot)
            {
                if (actor.IsFemale)
                    femaleDNA = RandomUtil.GetRandomObjectFromList(GetDNASamples(actor, CASAgeGenderFlags.Female));
                else
                    maleDNA = RandomUtil.GetRandomObjectFromList(GetDNASamples(actor, CASAgeGenderFlags.Male));
            }

            if (target.IsRobot)
            {
                if (target.IsFemale)
                    femaleDNA = RandomUtil.GetRandomObjectFromList(GetDNASamples(target, CASAgeGenderFlags.Female));
                else
                    maleDNA = RandomUtil.GetRandomObjectFromList(GetDNASamples(target, CASAgeGenderFlags.Male));
            }
        }

        public static bool SatisfiesImpregnate(Sim actor, Sim target, string logName, bool isAutonomous, bool scoreTarget, ref GreyedOutTooltipCallback callback)
        {
            if (!CanImpregnate(actor, target, ref callback))
                return false;

            // more requirements to come...

            return true;
        }

        private static ScientificSample SelectDNASample(Sim robot, CASAgeGenderFlags gender)
        {
            List<ObjectPicker.HeaderInfo> list = new List<ObjectPicker.HeaderInfo>();
            List<ObjectPicker.TabInfo> list2 = new List<ObjectPicker.TabInfo>();
            List<ScientificSample> dnaSamples = GetDNASamples(robot, gender);

            int numSelectableRows = 1;
            ObjectPicker.TabInfo tabInfo = new ObjectPicker.TabInfo(string.Empty, "TabText", new List<ObjectPicker.RowInfo>());

            foreach (ScientificSample current in dnaSamples)
            {
                ObjectPicker.RowInfo rowInfo = new ObjectPicker.RowInfo(current, new List<ObjectPicker.ColumnInfo>());
                rowInfo.ColumnInfo.Add(new ObjectPicker.ThumbAndTextColumn(current.GetThumbnailKey(), current.GetLocalizedName()));
                rowInfo.ColumnInfo.Add(new ObjectPicker.TextColumn(current.Subject.GetSubjectString()));
                tabInfo.RowInfo.Add(rowInfo);
            }

            list2.Add(tabInfo);
            list.Add(new ObjectPicker.HeaderInfo("HeaderString1", "HeaderTooltip1", 250));
            list.Add(new ObjectPicker.HeaderInfo("HeaderString2", "HeaderTooltip2", 250));
            List<ObjectPicker.RowInfo> list4 = ObjectPickerDialog.Show(true, ModalDialog.PauseMode.PauseSimulator, "DialogTitle",
                "Okay", "Cancel", list2, list, numSelectableRows);
            ScientificSample result = null;

            if (list4 != null && list4.Count > 0)
                result = (ScientificSample)list4[0].Item;

            return result;
        }

        // For meat mom
        private static Pregnancy StartPregnancy(Sim woman, Sim robot, SimDescription dna, bool isAutonomous, bool playChimes)
        {
            string reason = string.Empty;
            GreyedOutTooltipCallback callback = null;

            if (!CanGetPreggers(woman, isAutonomous, ref callback, out reason))
            {
                if (callback != null)
                    Common.DebugNotify("Surrogate Pregnancy: " + callback(), woman);

                return null;
            }
            else if (woman.SimDescription.IsPregnant)
            {
                Common.DebugNotify("Surrogate Pregnancy: Already Pregnant", woman);
                return woman.SimDescription.Pregnancy;
            }

            Pregnancy pregnancy = HumanPregnancyProxy.Start(woman, robot, dna);

            if (pregnancy != null)
            {
                if (playChimes && (woman.IsSelectable || robot.IsSelectable))
                    Audio.StartSound("sting_baby_conception");

                Common.DebugNotify("Surrogate Pregnancy: Success", woman);
            }

            return pregnancy;
        }

        // For robo mom
        private static Pregnancy StartPregnancy(Sim robot, Sim father, SimDescription dnaF, SimDescription dnaM, bool isAutonomous, bool playChimes)
        {
            string reason = string.Empty;
            GreyedOutTooltipCallback callback = null;

            if (!CanGetPreggers(robot, isAutonomous, ref callback, out reason))
            {
                if (callback != null)
                    Common.DebugNotify("Surrogate Pregnancy: " + callback(), robot);

                return null;
            }
            else if (robot.SimDescription.IsPregnant)
            {
                Common.DebugNotify("Surrogate Pregnancy: Already Pregnant", robot);

                return robot.SimDescription.Pregnancy;
            }

            Pregnancy pregnancy = RobotPregnancyProxy.Start(robot, father, dnaF, dnaM);

            if (pregnancy != null)
            {
                if (playChimes && (robot.IsSelectable || father.IsSelectable))
                    Audio.StartSound("sting_baby_conception");

                Common.DebugNotify("Surrogate Pregnancy: Success", robot);
            }

            return pregnancy;
        }

        public static void UpdateRobotPregnancyTuning()
        {
            int pregnancyLength = Woohooer.Settings.mRobotPregnancyLength * 24;

            // No testing phase or pre-show phase for robot pregnancies
            //Woohooer.Settings.mRobotHourToStartContractions = pregnancyLength;
            //Woohooer.Settings.mRobotHoursOfPregnancy = pregnancyLength + Woohooer.Settings.mRobotLaborLength;
            Woohooer.Settings.mRobotHoursOfPregnancy = pregnancyLength;

            Woohooer.Settings.mForeignRobotDisplayTNS = (int)Math.Round((1f / 6f) * pregnancyLength);
            Woohooer.Settings.mForeignRobotLeavesWorld = (int)(Math.Round(1f / 4f) * pregnancyLength);
            Woohooer.Settings.mRobotHourToStartWalkingPregnant = (int)Math.Round((5f / 9f) * pregnancyLength);
            // Robots will not morph over the course of their pregnancy
        }
    }
}
