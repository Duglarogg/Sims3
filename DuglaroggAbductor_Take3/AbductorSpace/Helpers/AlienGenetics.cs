using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Helpers
{
    public class AlienGenetics
    {
        static ResourceKey[] kSkinTones = new ResourceKey[]
        {
                new ResourceKey(ResourceUtils.HashString64("AlienEP08 Skin"), 55867754u, 0u),
                new ResourceKey(ResourceUtils.HashString64("Aqua Skin Tone"), 55867754u, 0u),
                new ResourceKey(ResourceUtils.HashString64("Blue Skin"), 55867754u, 0u),
                new ResourceKey(ResourceUtils.HashString64("Gold Skin Tone"), 55867754u, 0u),
                new ResourceKey(ResourceUtils.HashString64("Purple Skine Tone"), 55867754u, 0u),
                new ResourceKey(ResourceUtils.HashString64("Alien Skin"), 55867754u, 0u)
        };

        static Color[] kHairColors = new Color[]
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

        static float GetSkinToneIndex(ResourceKey tone)
        {
            float result = 0;

            if (tone == kSkinTones[0])
            {
                result = 1f;
            }
            else if (tone == kSkinTones[2])
            {
                result = 0.5f + (0.5f * RandomUtil.GetFloat(1f));
            }
            else if (tone == kSkinTones[5])
            {
                result = 0.25f + (0.75f * RandomUtil.GetFloat(1f));
            }
            else
            {
                result = 0.25f + (0.5f * RandomUtil.GetFloat(1f));
            }

            return result;
        }

        public static SimDescription MakeAlien(CASAgeGenderFlags age, CASAgeGenderFlags gender, WorldName homeworld, float alienDNAPercentage, bool assignRandomTraits)
        {
            ResourceKey skinTone = kSkinTones[RandomUtil.GetInt(0, kSkinTones.Length - 1)];
            float skinToneIndex = GetSkinToneIndex(skinTone);
            SimBuilder sb = new SimBuilder();
            sb.Age = age;
            sb.Gender = gender;
            sb.Species = CASAgeGenderFlags.Human;
            sb.SkinTone = skinTone;
            sb.SkinToneIndex = skinToneIndex;
            sb.TextureSize = 1024u;
            sb.UseCompression = true;
            bool flag = (gender == CASAgeGenderFlags.Female);
            SetFacialBlends(sb);
            SimDescription alienDescription = Genetics.MakeSim(sb, age, gender, skinTone, skinToneIndex, kHairColors, homeworld, 4294967295u, true);

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
            WorldName homeworld = GameUtils.GetCurrentWorld();

            if (abductee.Household.IsTouristHousehold)
            {
                homeworld = abductee.HomeWorld;
            }

            SimDescription baby = MakeAlien(CASAgeGenderFlags.Baby, gender, homeworld, 1f, interactive);

            if (baby != null)
            {
                if (interactive)
                {
                    baby.FirstName = string.Empty;
                }

                baby.LastName = alien.LastName;
                Genetics.AssignTraits(baby, alien, abductee, interactive, averageMood, pregoRandom);
                baby.TraitManager.AddHiddenElement(AlienUtilsEx.sAlienChild);

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
                // Link to NRaasHybrid to allow alien children to inherit one or more occult types from abductee.
                /* ---------------------------------------------------------------------------------------------- */
            }

            return baby;
        }

        static void SetFacialBlends(SimBuilder sb)
        {
            bool flag = (sb.Gender == CASAgeGenderFlags.Female);
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
    }
}
