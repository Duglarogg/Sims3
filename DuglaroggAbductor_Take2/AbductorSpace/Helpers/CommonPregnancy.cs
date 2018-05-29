using Duglarogg.CommonSpace.Helpers;
using NRaas;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Helpers
{
    public class CommonPregnancy
    {
        static Common.MethodStore sStoryProgressionAllowImpregnation = new Common.MethodStore("NRaasStoryProgression", "NRaas.StoryProgression", "AllowImpregnation", new Type[] { typeof(SimDescription), typeof(bool) });
        static Common.MethodStore sWoohooerAllowPlantSimPregnancy = new Common.MethodStore("NRaasWoohooer", "NRaas.Woohooer", "AllowPlantSimPregnancy", new Type[0]);

        public static BuffNames sItsQuadruplets = unchecked((BuffNames)ResourceUtils.HashString64("NRaasItsQuadruplets"));

        public static bool AllowPlantSimPregnancy()
        {
            if (sWoohooerAllowPlantSimPregnancy.Valid)
            {
                return sWoohooerAllowPlantSimPregnancy.Invoke<bool>(new Type[0]);
            }

            return true;
        }

        static bool CanGetPreggers(Sim sim)
        {
            if (SimTypes.IsPassporter(sim.SimDescription))
            {
                return false;
            }

            int numHumans, numPets;
            sim.SimDescription.Household.GetNumberOfSimsAndPets(true, out numHumans, out numPets);

            if (!Household.CanSpeciesGetAddedToHousehold(sim.SimDescription.Species, numHumans, numPets))
            {
                return false;
            }

            if (SimTypes.IsSkinJob(sim.SimDescription))
            {
                return false;
            }
            else if (sim.BuffManager.HasTransformBuff())
            {
                return false;
            }
            else if (sim.SimDescription.IsPregnant || sim.SimDescription.IsVisuallyPregnant)
            {
                return false;
            }

            if (sim.Household != null && sim.Household.IsTouristHousehold)
            {
                MiniSimDescription description = MiniSimDescription.Find(sim.SimDescription.SimDescriptionId);

                if (description == null)
                {
                    return false;
                }
            }
            else if (sim.LotHome == null)
            {
                return false;
            }
            else if (sim.SimDescription.IsDueToAgeUp() || (sim.SimDescription.AgingState != null && sim.SimDescription.AgingState.IsAgingInProgress()))
            {
                return false;
            }
            else if (SimTypes.IsLampGenie(sim.SimDescription))
            {
                return false;
            }

            return true;
        }

        public static Pregnancy CreatePregnancy(Sim mother, SimDescription father, bool handlePlantSim)
        {
            if ((mother.SimDescription.IsPlantSim || father.IsPlantSim) && handlePlantSim && (SimTypes.IsSelectable(mother) || SimTypes.IsSelectable(father)))
            {
                IGameObject obj = GlobalFunctions.CreateObjectOutOfWorld("forbiddenFruit", ProductVersion.EP9, "Sims3.Gameplay.Objects.Gardening.ForbiddenFruit", null);

                if (obj != null)
                {
                    Inventories.TryToMove(obj, mother);
                }

                return null;
            }

            return new Pregnancy(mother, father);
        }

        static float GetImpregnationChance(Sim a, SimDescription b)
        {
            float result = Abductor.Settings.mImpregnationChance;

            if (result <= 0)
            {
                Logger.Append("Alien Pregnancy: Disabled");
                return 0;
            }

            if (a.BuffManager != null && a.BuffManager.HasTransformBuff())
            {
                return 0;
            }

            if (b.CreatedSim != null && b.CreatedSim.BuffManager != null && b.CreatedSim.BuffManager.HasTransformBuff())
            {
                return 0;
            }

            if (Abductor.Settings.mUseFertility)
            {
                if ((a.BuffManager != null && a.BuffManager.HasElement(BuffNames.ATwinkleInTheEye)) || a.TraitManager.HasElement(TraitNames.FertilityTreatment))
                {
                    result += TraitTuning.kFertilityBabyMakingChanceIncrease;
                }

                if (a.BuffManager != null && a.BuffManager.HasElement(BuffNames.MagicInTheAir))
                {
                    result += BuffMagicInTheAir.kBabyMakingChanceIncrease * 100f;
                }

                if (a.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    result += 100f;
                    a.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                }

                if ((b.CreatedSim != null && b.CreatedSim.BuffManager != null && b.CreatedSim.BuffManager.HasElement(BuffNames.ATwinkleInTheEye)) || b.TraitManager.HasElement(TraitNames.FertilityTreatment))
                {
                    result += TraitTuning.kFertilityBabyMakingChanceIncrease;
                }

                if (b.CreatedSim != null && b.CreatedSim.BuffManager != null && b.CreatedSim.BuffManager.HasElement(BuffNames.MagicInTheAir))
                {
                    result += BuffMagicInTheAir.kBabyMakingChanceIncrease * 100f;
                }

                if (b.TraitManager.HasElement(TraitNames.WishedForLargeFamily))
                {
                    result += 100f;

                    if (b.CreatedSim != null)
                    {
                        b.CreatedSim.BuffManager.RemoveElement(BuffNames.WishForLargeFamily);
                    }
                }

                if (GameUtils.IsInstalled(ProductVersion.EP7) && SimClock.IsNightTime() && SimClock.IsFullMoon())
                {
                    result += Pregnancy.kFullMoonImprovedBabyChance * 100f;
                }
            }

            return result;
        }

        static bool HasBlockingBuff(Sim sim)
        {
            if (sim == null)
            {
                return false;
            }

            if (sim.BuffManager == null)
            {
                return false;
            }

            return sim.BuffManager.HasAnyElement(new BuffNames[] { BuffNames.ItsABoy, BuffNames.ItsAGirl, BuffNames.ItsTwins, BuffNames.ItsTriplets, sItsQuadruplets });
        }

        public static bool IsSuccess(Sim abductee, SimDescription alien)
        {
            if (!CanGetPreggers(abductee))
            {
                return false;
            }

            float chance = GetImpregnationChance(abductee, alien);

            if (!RandomUtil.RandomChance(chance))
            {
                Logger.Append("Alien Pregnancy: Chance Fail " + chance);
                return false;
            }

            return true;
        }
    }
}
