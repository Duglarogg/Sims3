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

                if (alien.TraitManager.HasElement(TraitNames.FertilityTreatment))
                    chance += TraitTuning.kFertilityBabyMakingChanceIncrease;

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
