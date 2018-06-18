using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.ScoringMethods;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Helpers
{
    public class CommonPregnancy : Common.IPreLoad
    {
        static Common.MethodStore sStoryProgressionAllowImpregnation = new Common.MethodStore("NRaasStoryProgression", "NRaas.StoryProgression",
            "AllowImpregnation", new Type[] { typeof(SimDescription), typeof(bool) });

        /*
        static Common.MethodStore sWoohooerAllowPlantSimPregnancy = new Common.MethodStore("NRaasWoohooer", "NRaas.Woohooer", "AllowPlantSimPregnancy",
            new Type[] { });
            */

        public delegate float GetChanceOfSuccess(Sim a, SimDescription b);
        public static GetChanceOfSuccess sGetChanceOfSuccess = OnGetChanceOfSuccess;
        public static BuffNames sItsQuadruplets = unchecked((BuffNames)ResourceUtils.HashString64("NRaasItsQuadruplets"));

        public static bool AllowPlantSimPregnancy()
        {
            return true;
        }

        
        public static Genetics.TraitOutcome AssignTraits(SimDescription baby, SimDescription abductee, bool interactive, float averageMood, Random pregoRandom)
        {
            List<Trait> horribleTraits = new List<Trait>();
            List<Trait> negativeTratis = new List<Trait>();
            List<Trait> remainingTraits = new List<Trait>();
            CASAGSAvailabilityFlags cagsFlags = CASUtils.CASAGSAvailabilityFlagsFromCASAgeGenderFlags(baby.Age | baby.Species);

            foreach (Trait trait in TraitManager.GetDictionaryTraits)
            {
                if (trait.CanBeLearnedRandomly && trait.TraitValidForAgeSpecies(cagsFlags) && GameUtils.IsInstalled(trait.ProductVersion))
                {
                    if (trait.Score <= TraitManager.HorribleScore)
                        horribleTraits.Add(trait);
                    else if (trait.Score < 0)
                        negativeTratis.Add(trait);
                    else
                        remainingTraits.Add(trait);
                }
            }

            List<TraitNames> babyTraitNames = new List<TraitNames>();
            List<Genetics.InheritedTraitSource> traitSources = new List<Genetics.InheritedTraitSource>();
            Genetics.TraitOutcome result = Genetics.InheritTraits(remainingTraits, negativeTratis, horribleTraits, averageMood, pregoRandom, abductee, null, baby, babyTraitNames, traitSources, baby.Age);

            if (abductee != null)
            {
                foreach (Trait trait in abductee.TraitManager.List)
                {
                    bool flag = trait.AgeSpeciesAvailabiltiyFlag == CASAGSAvailabilityFlags.None;

                    if (flag && (!Genetics.IsCultureSpecificTrait(trait.Guid) || RandomUtil.RandomChance01(TraitTuning.CultureTypeTraitChanceForInheritance)))
                    {
                        if (trait.Guid == TraitNames.ImmuneToFire && Genetics.AssignPyroTrait(abductee))
                            baby.TraitManager.AddElement(TraitManager.GetTraitFromDictionary(TraitNames.Pyromaniac).Guid, flag);
                        else if (!Array.Exists<TraitNames>(Genetics.kHiddenTraitsToNotInherit, (TraitNames tn) => trait.Guid == tn))
                            baby.TraitManager.AddElement(trait.Guid, flag);
                    }

                    traitSources.Add(Genetics.InheritedTraitSource.Parent);
                }
            }

            if (interactive)
            {
                string titleText = Localization.LocalizeString(baby.IsFemale, "Gameplay/CAS/Genetics:MakeBabyTitle", new object[0]);
                string promptText = null;

                switch (result)
                {
                    case Genetics.TraitOutcome.Horrible:
                        {
                            string entryKey = abductee.IsMale ? "Gameplay/CAS/Genetics/MakeBabyDescTwoTraitsHorribleMale" : "Gameplay/CAS/Genetics/MakeBabyDescTwoTraitsHorrible";
                            promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                {
                                    abductee,
                                    baby.TraitManager.GetElement((ulong)babyTraitNames[0]).TraitName(baby.IsFemale),
                                    baby.TraitManager.GetElement((ulong)babyTraitNames[1]).TraitName(baby.IsFemale)
                                });
                        }
                        break;

                    case Genetics.TraitOutcome.Bad:
                        {
                            string entryKey = abductee.IsMale ? "Gameplay/CAS/Genetics/MakeBabyDescTwoTraitsBadMale" : "Gameplay/CAS/Genetics/MakeBabyDescTwoTraitsBad";
                            promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                {
                                    abductee,
                                    baby.TraitManager.GetElement((ulong)babyTraitNames[0]).TraitName(baby.IsFemale),
                                    baby.TraitManager.GetElement((ulong)babyTraitNames[1]).TraitName(baby.IsFemale)
                                });
                        }
                        break;

                    case Genetics.TraitOutcome.Average:
                        {
                            string entryKey = abductee.IsMale ? "Gameplay/CAS/Genetics/MakeBabyDescTwoTraitsMale" : "Gameplay/CAS/Genetics/MakeBabyDescTwoTraits";
                            promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                {
                                    abductee,
                                    baby.TraitManager.GetElement((ulong)babyTraitNames[0]).TraitName(baby.IsFemale),
                                    baby.TraitManager.GetElement((ulong)babyTraitNames[1]).TraitName(baby.IsFemale)
                                });
                        }
                        break;

                    case Genetics.TraitOutcome.Good:
                        {
                            string entryKey = abductee.IsMale ? "Gameplay/CAS/Genetics/MakeBabyDescOneTraitsMale" : "Gameplay/CAS/Genetics/MakeBabyDescOneTraits";
                            promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                {
                                    abductee,
                                    baby.TraitManager.GetElement((ulong)babyTraitNames[0]).TraitName(baby.IsFemale)
                                });
                        }
                        break;

                    case Genetics.TraitOutcome.Excellent:
                        {
                            string entryKey = abductee.IsMale ? "Gameplay/CAS/Genetics:MakeBabyDescMale" : "Gameplay/CAS/Genetics:MakeBabyDesc";
                            promptText = Localization.LocalizeString(baby.IsFemale, entryKey, new object[]
                                {
                                    abductee
                                });
                        }
                        break;
                }

                while (string.IsNullOrEmpty(baby.FirstName))
                    baby.FirstName = StringInputDialog.Show(titleText, promptText, "", CASBasics.GetMaxNameLength(), StringInputDialog.Validation.SimNameText);
            }

            int numVisibleTraits = baby.TraitManager.CountVisibleTraits();
            int numTraitsForAge = baby.TraitManager.NumTraitsForAge();
            int difference;

            if (numVisibleTraits < numTraitsForAge)
                difference = numTraitsForAge - numVisibleTraits;
            else
                difference = 0;

            if (difference > 0)
            {
                List<Trait> list6 = new List<Trait>();
                list6.AddRange(horribleTraits);
                list6.AddRange(negativeTratis);
                list6.AddRange(remainingTraits);

                if (interactive)
                {
                    List<ITraitEntryInfo> list7 = new List<ITraitEntryInfo>();

                    foreach (Trait trait in list6)
                        list7.Add(trait);

                    Dictionary<ulong, ITraitEntryInfo> dictionary = new Dictionary<ulong, ITraitEntryInfo>();

                    using (List<TraitNames>.Enumerator enumerator5 = babyTraitNames.GetEnumerator())
                    {
                        while (enumerator5.MoveNext())
                        {
                            ulong current3 = (ulong)enumerator5.Current;

                            if (TraitManager.GetTraitFromDictionary((TraitNames)current3).IsVisible)
                                dictionary.Add(current3, baby.TraitManager.GetElement(current3));
                        }
                    }

                    List<ulong> list8 = TraitsPickerDialog.Show(baby, numTraitsForAge, dictionary, list7, null, false, false, false, true);

                    if (list8 == null)
                        return result;

                    using (List<ulong>.Enumerator enumerator6 = list8.GetEnumerator())
                    {
                        while (enumerator6.MoveNext())
                        {
                            ulong current4 = enumerator6.Current;

                            foreach (Trait current5 in list6)
                            {
                                if (current5.TraitGuid == current4)
                                {
                                    baby.TraitManager.AddElement(current5.Guid, current5.AgeSpeciesAvailabiltiyFlag == CASAGSAvailabilityFlags.None);
                                    traitSources.Add(Genetics.InheritedTraitSource.PlayerSelected);
                                    break;
                                }
                            }
                        }
                    }

                    return result;
                }

                while (difference > 0)
                {
                    List<float> list9 = new List<float>();
                    List<Trait> list10 = new List<Trait>();

                    foreach (Trait current6 in list6)
                    {
                        Trait trait2;

                        if (!baby.TraitManager.IsConflictingTrait(current6.Guid, out trait2))
                        {
                            list10.Add(current6);
                            list9.Add(current6.RandomWeight);
                        }
                    }

                    if (list10.Count == 0)
                        return result;

                    list6 = list10;
                    float num4;
                    Trait weightedRandomObjectFromList = RandomUtil.GetWeightedRandomObjectFromList(list9, list6, out num4);
                    list6.Remove(weightedRandomObjectFromList);

                    if (baby.TraitManager.AddElement(weightedRandomObjectFromList.Guid, weightedRandomObjectFromList.AgeSpeciesAvailabiltiyFlag == CASAGSAvailabilityFlags.None))
                    {
                        traitSources.Add(Genetics.InheritedTraitSource.Random);
                        difference--;
                    }
                }
            }

            return result;
        }
        

        public static bool CanGetPregnant(Sim sim, bool isAutonomous, out string reason)
        {
            using (Common.TestSpan span = new Common.TestSpan(ScoringLookup.Stats, "Duration CanGetPregnant", Common.DebugLevel.Stats))
            {
                if (SimTypes.IsPassporter(sim.SimDescription))
                {
                    reason = "Passporter";
                    return false;
                }

                int numHumans, numPets;
                sim.SimDescription.Household.GetNumberOfSimsAndPets(true, out numHumans, out numPets);

                if (!Household.CanSpeciesGetAddedToHousehold(sim.SimDescription.Species, numHumans, numPets))
                {
                    reason = "House Full";
                    return false;
                }

                if (sim.SimDescription.Teen && !Aliens.Settings.mAllowTeens)
                {
                    reason = "Teens Not Allowed";
                    return false;
                }
                else if (SimTypes.IsSkinJob(sim.SimDescription))
                {
                    reason = "Skin Job Fail";
                    return false;
                }
                else if (sim.BuffManager.HasTransformBuff())
                {
                    reason = "Transform Buff";
                    return false;
                }
                else if (sim.SimDescription.IsPregnant || sim.SimDescription.IsVisuallyPregnant)
                {
                    reason = "Already Pregnant";
                    return false;
                }

                if (sim.Household != null && sim.Household.IsTouristHousehold)
                {
                    MiniSimDescription description = MiniSimDescription.Find(sim.SimDescription.SimDescriptionId);

                    if (description == null)
                    {
                        reason = "House Full";
                        return false;
                    }
                }
                else if (sim.LotHome == null)
                {
                    reason = "House Full";
                    return false;
                }
                else if (sim.SimDescription.IsDueToAgeUp() || (sim.SimDescription.AgingState != null && sim.SimDescription.AgingState.IsAgingInProgress()))
                {
                    reason = "Aging Up Fail";
                    return false;
                }
                else if (SimTypes.IsLampGenie(sim.SimDescription))
                {
                    reason = "Lamp Genie";
                    return false;
                }

                if (sStoryProgressionAllowImpregnation.Valid && Aliens.Settings.LinkToStoryProgression(isAutonomous))
                {
                    reason = sStoryProgressionAllowImpregnation.Invoke<string>(new object[] { sim.SimDescription, isAutonomous });

                    if (reason != null)
                        return false;
                }

                reason = null;
                return true;
            }
        }

        /*
        public static Genetics.TraitOutcome InheritTraits(List<Trait> remainingTraits, List<Trait> negativeTraits, List<Trait> horribleTraits, 
            float averageMood, Random rnd, SimDescription abductee, SimDescription baby, List<TraitNames> babyTraitNames, 
            List<Genetics.InheritedTraitSource> traitSources, CASAgeGenderFlags age)
        {

        }
        */

        public static float OnGetChanceOfSuccess(Sim abductee, SimDescription alien)
        {
            float chance = Aliens.Settings.mPregnancyChance;

            if (chance <= 0)
            {
                Common.DebugNotify("Abductions: Pregnancy Disabled");
                return 0;
            }

            if (Aliens.Settings.mUseFertility)
            {
                if ((abductee.BuffManager != null && abductee.BuffManager.HasElement(BuffNames.ATwinkleInTheEye))
                    || (abductee.TraitManager.HasElement(TraitNames.FertilityTreatment)))
                    chance += TraitTuning.kFertilityBabyMakingChanceIncrease;

                if (abductee.BuffManager != null && abductee.BuffManager.HasElement(BuffNames.MagicInTheAir))
                    chance += BuffMagicInTheAir.kBabyMakingChanceIncrease;

                if (abductee.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    chance += 100f;
                    abductee.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                }

                if (GameUtils.IsInstalled(ProductVersion.EP7) && SimClock.IsNightTime() && SimClock.IsFullMoon())
                    chance += Pregnancy.kFullMoonImprovedBabyChance;
            }

            return chance;
        }

        public void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.GetTuning<RabbitHole, Pregnancy.GoToHospital.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }

            tuning = Tunings.GetTuning<Lot, Pregnancy.HaveBabyHome.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }

            tuning = Tunings.GetTuning<RabbitHole, Pregnancy.HaveBabyHospital.Definition>();

            if (tuning != null)
            {
                tuning.Availability.Teens = true;
                tuning.Availability.Adults = true;
                tuning.Availability.Elders = true;
            }
        }
    }
}
