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
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Helpers
{
    public class CommonPregnancy : Common.IPreLoad
    {
        static Common.MethodStore sStoryProgressionAllowImpregnation = new Common.MethodStore("NRaasStoryProgression", "NRaas.StoryProgression", "AllowImpregnation", new Type[] { typeof(SimDescription), typeof(bool) });
        static Common.MethodStore sWoohooerAllowPlantSimPregnancy = new Common.MethodStore("NRaasWoohooer", "NRaas.Woohooer", "AllowPlantSimPregnancy", new Type[] { });

        public static BuffNames sItsQuadruplets = unchecked((BuffNames)ResourceUtils.HashString64("NRaasItsQuadruplets"));

        public static GetChanceOfSuccess sGetChanceOfSuccess = OnGetChanceOfSuccess;

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

        public static bool AllowPlantSimPregnancy()
        {
            if (sWoohooerAllowPlantSimPregnancy.Valid)
            {
                return sWoohooerAllowPlantSimPregnancy.Invoke<bool>(new Type[] { });
            }
            else
            {
                return true;
            }
        }

        private static bool CanGetPreggers(Sim sim, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback, out string reason)
        {
            using (Common.TestSpan span = new Common.TestSpan(ScoringLookup.Stats, "Duration CanGetPreggers", Common.DebugLevel.Stats))
            {
                if (SimTypes.IsPassporter(sim.SimDescription))
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Passporter");
                    reason = "Passporter";
                    return false;
                }

                CASAgeGenderFlags species = sim.SimDescription.Species;
                int numHumans, numPets;
                sim.SimDescription.Household.GetNumberOfSimsAndPets(true, out numHumans, out numPets);

                if (!Household.CanSpeciesGetAddedToHousehold(species, numHumans, numPets))
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("House Full");
                    reason = "House Full";
                    return false;
                }

                if (SimTypes.IsSkinJob(sim.SimDescription))
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Skin Job Fail");
                    reason = "Skin Job Fail";
                    return false;
                }
                else if (sim.BuffManager.HasTransformBuff())
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Transform Buff");
                    reason = "Transform Buff";
                    return false;
                }
                else if (sim.SimDescription.IsPregnant || sim.SimDescription.IsVisuallyPregnant)
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Already Pregnant");
                    reason = "Already Pregnant";
                    return false;
                }

                if ((sim.Household != null) && sim.Household.IsTouristHousehold)
                {
                    MiniSimDescription description = MiniSimDescription.Find(sim.SimDescription.SimDescriptionId);

                    if (description == null)
                    {
                        greyedOutTooltipCallback = delegate
                        {
                            return Common.LocalizeEAString(sim.IsFemale, "Gameplay/Actors/Sim/TryForBaby:TooManySims", new object[] { sim });
                        };
                        reason = "TooManySims";
                        return false;
                    }
                }
                else if (sim.LotHome == null)
                {
                    greyedOutTooltipCallback = delegate
                    {
                        if (sim.Household.IsAlienHousehold)
                        {
                            return Common.LocalizeEAString(sim.IsFemale, "Gameplay/Actors/Sim/TryForBaby:AlienNPCs", new object[] { sim });
                        }
                        else
                        {
                            return Common.LocalizeEAString(sim.IsFemale, "Gameplay/Actors/Sim/TryForBaby:TooManySims", new object[] { sim });
                        }
                    };
                    reason = "TooManySims";
                    return false;
                }
                else if (sim.SimDescription.IsDueToAgeUp() || (sim.SimDescription.AgingState != null && sim.SimDescription.AgingState.IsAgingInProgress()))
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Aging Up Fail");
                    reason = "Aging Up Fail";
                    return false;
                }
                else if (SimTypes.IsLampGenie(sim.SimDescription))
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Lamp Genie");
                    reason = "Lamp Genie";
                    return false;
                }

                if (sStoryProgressionAllowImpregnation.Valid && Abductor.Settings.LinkToStoryProgression(isAutonomous))
                {
                    reason = sStoryProgressionAllowImpregnation.Invoke<string>(new object[] { sim.SimDescription, isAutonomous });
                    if (reason != null)
                    {
                        greyedOutTooltipCallback = Abductor.StoryProgessionTooltip(reason, false);
                        return false;
                    }
                }

                reason = null;
                return true;
            }
        }

        public static Pregnancy CreatePregnancy(Sim mother, SimDescription father, bool handlePlantSim)
        {
            if (handlePlantSim)
            {
                if (SimTypes.IsSelectable(mother) || SimTypes.IsSelectable(father))
                {
                    if (mother.SimDescription.IsPlantSim || father.IsPlantSim)
                    {
                        IGameObject obj = GlobalFunctions.CreateObjectOutOfWorld("forbiddenFruit", ProductVersion.EP9, "Sims3.Gameplay.Objects.Gardening.ForbiddenFruit", null);

                        if (obj != null)
                        {
                            Inventories.TryToMove(obj, mother);
                        }
                    }

                    return null;
                }
            }

            return new Pregnancy(mother, father);
        }

        private static bool HasBlockingBuff(Sim sim)
        {
            if (sim == null) return false;

            if (sim.BuffManager == null) return false;

            return sim.BuffManager.HasAnyElement(new BuffNames[] { BuffNames.ItsABoy, BuffNames.ItsAGirl, BuffNames.ItsTwins, BuffNames.ItsTriplets, sItsQuadruplets });
        }

        public static bool IsSuccess(Sim abductee, Sim abductor, bool isAutonomous)
        {
            string reason;
            GreyedOutTooltipCallback callBack = null;

            if (!CanGetPreggers(abductee, isAutonomous, ref callBack, out reason))
            {
                if (callBack != null)
                {
                    Common.DebugNotify("Pregnancy: " + callBack(), abductee);
                }

                return false;
            }

            float chance = sGetChanceOfSuccess(abductee, abductor);

            if (!RandomUtil.RandomChance(chance))
            {
                Common.DebugNotify("Pregnancy: Chance Fail " + chance, abductee, abductor);
                return false;
            }

            Common.DebugNotify("Pregnancy: Chance Success " + chance, abductee, abductor);
            return true;
        }

        public static float OnGetChanceOfSuccess(Sim a, Sim b)
        {
            float chance = Abductor.Settings.mImpregnationChance;

            if (chance <= 0)
            {
                Common.DebugNotify("Alien Pregnancy: No Chance");
                return 0;
            }

            bool useFertility = Abductor.Settings.mUseFertility;

            if (useFertility)
            {
                if (a.BuffManager != null && a.BuffManager.HasTransformBuff()) return 0;

                if (b.BuffManager != null && b.BuffManager.HasTransformBuff()) return 0;

                if ((a.BuffManager != null && a.BuffManager.HasElement(BuffNames.ATwinkleInTheEye)) || a.TraitManager.HasElement(TraitNames.FertilityTreatment))
                {
                    chance += TraitTuning.kFertilityBabyMakingChanceIncrease;
                }

                if (a.BuffManager != null && a.BuffManager.HasElement(BuffNames.MagicInTheAir))
                {
                    chance += BuffMagicInTheAir.kBabyMakingChanceIncrease * 100f;
                }

                if (a.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    chance += 100f;
                    a.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                }

                if ((b.BuffManager != null && b.BuffManager.HasElement(BuffNames.ATwinkleInTheEye)) || b.TraitManager.HasElement(TraitNames.FertilityTreatment))
                {
                    chance += TraitTuning.kFertilityBabyMakingChanceIncrease;
                }

                if (b.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    chance += 100f;
                    b.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                }

                if (b.BuffManager != null && b.BuffManager.HasElement(BuffNames.MagicInTheAir))
                {
                    chance += BuffMagicInTheAir.kBabyMakingChanceIncrease * 100f;
                }

                if (GameUtils.IsInstalled(ProductVersion.EP7) && SimClock.IsNightTime() && SimClock.IsFullMoon())
                {
                    chance += Pregnancy.kFullMoonImprovedBabyChance * 100f;
                }
            }

            return chance;
        }

        public delegate float GetChanceOfSuccess(Sim a, Sim b);
    }
}
