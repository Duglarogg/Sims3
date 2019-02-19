using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.CelebritySystem;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Pools;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.WoohooerSpace.Interactions
{
    public class RaidDNABank : RabbitHole.RabbitHoleInteraction<Sim, RabbitHole>, Common.IAddInteraction
    {
        [DoesntRequireTuning] // Ignoring tuning... for now
        public class Definition : InteractionDefinition<Sim, RabbitHole, RaidDNABank>
        {
            public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop)
            {
                if (actor.IsMale)
                    return Common.Localize("RaidDNABankMale:MenuName");
                else
                    return Common.Localize("RaidDNABankFemale:MenuName");
            }

            public override bool Test(Sim actor, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.Guid != RabbitHoleType.Hospital)
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Target is not a hospital!");
                    return false;
                }

                if (!actor.IsRobot)
                {
                    greyedOutTooltipCallback = Common.DebugTooltip("Actor is not a robot!");
                    return false;
                }

                return true;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();
        public List<SimDescription> mDonors = null;
        public bool mSuccessful = false;
        public int mChance = 0;
        public int mMaxSamples = 0;

        [Tunable, TunableComment("Duration (in minutes) for a DNA raid")]
        public static int kSimMinutesForRaid = 60;

        [Tunable, TunableComment("Base max number of DNA samples that can be stolen")]
        public static int kSamplesToSteal = 5;

        [Tunable, TunableComment("Base chance of stealing one DNA sample")]
        public static int kChanceToSteal = 45;

        [Tunable, TunableComment("")]
        public static int kBonusToSteal = 15;

        [Tunable, TunableComment("")]
        public static int kPenaltyToSteal = 15;

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<RabbitHole>(Singleton);
        }

        public override bool BeforeEnteringRabbitHole()
        {
            mDonors = new List<SimDescription>();

            Dictionary<ulong, SimDescription>.Enumerator residentSims = SimListing.GetResidents(true).GetEnumerator();

            while (residentSims.MoveNext())
            {
                SimDescription sim = residentSims.Current.Value;

                if (sim.IsHuman && !(sim.IsRobot || sim.IsMummy)  && sim.Gender == Actor.SimDescription.Gender)
                    mDonors.Add(sim);
            }

            mChance = kChanceToSteal;
            mMaxSamples = kSamplesToSteal;

            if (Actor.IsSimBot)
            {
                if (Actor.TraitManager.HasElement(TraitNames.Burglar))
                {
                    mChance += kBonusToSteal;
                    mMaxSamples++;
                }

                if (Actor.TraitManager.HasElement(TraitNames.Clumsy))
                {
                    mChance -= kPenaltyToSteal;
                    mMaxSamples--;
                }

                if (Actor.TraitManager.HasElement(TraitNames.Evil))
                    mChance += kBonusToSteal;

                if (Actor.TraitManager.HasElement(TraitNames.Kleptomaniac))
                {
                    mChance += kBonusToSteal;
                    mMaxSamples++;
                }

                if (Actor.TraitManager.HasElement(TraitNames.Loser))
                {
                    mChance -= kPenaltyToSteal;
                    mMaxSamples--;
                }

                if (Actor.TraitManager.HasElement(TraitNames.Lucky))
                    mChance += kBonusToSteal;

                if (Actor.TraitManager.HasElement(TraitNames.Unlucky))
                    mChance -= kPenaltyToSteal;
            }

            if (Actor.IsEP11Bot)
            {
                if (Actor.TraitManager.HasElement(TraitNames.EfficientChip))
                    mMaxSamples++;

                if (Actor.TraitManager.HasElement(TraitNames.EvilChip))
                    mChance += kBonusToSteal;

                if (Actor.TraitManager.HasElement(TraitNames.FearOfHumansChip))
                {
                    mChance -= kPenaltyToSteal;
                    mMaxSamples--;
                }

                if (Actor.TraitManager.HasElement(TraitNames.SentienceChip))
                    mChance += kBonusToSteal;

            }

            return base.BeforeEnteringRabbitHole();
        }

        public override bool InRabbitHole()
        {
            BeginCommodityUpdates();
            bool flag = DoTimedLoop(kSimMinutesForRaid);
            EndCommodityUpdates(flag);

            int samplesStolen = 0;

            if (flag)
            {
                for (int i = 0; i < mMaxSamples; i++)
                {
                    if (RandomUtil.RandomChance(mChance))
                    {
                        ScientificSample.DnaSampleSubject subject = new ScientificSample.DnaSampleSubject(RandomUtil.GetRandomObjectFromList(mDonors));
                        ScientificSample.CreateAndAddToInventory(Actor, subject);
                        samplesStolen++;
                    }
                }
            }

            mSuccessful = samplesStolen > 0;

            return flag;
        }

        public override bool AfterExitingRabbitHole()
        {
            if (mSuccessful)
            {
                Actor.PlaySoloAnimation("a_pregnancy_idle_rubBelly_x", true);
                // Trigger notification stating how many DNA samples were stolen
            }
            else
            {
                // Trigger notification stating that the raid failed
            }

            return true;
        }
    }
}
