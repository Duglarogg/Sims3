﻿using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienActivity.AlienActivityBonuses
{
    public class SpaceRockOnLotThresholdSetting : IntegerSettingOption<GameObject>, IAlienActivityBonusesOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mSpaceRockThreshold;
            set => Aliens.Settings.mSpaceRockThreshold = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mBaseVisitChance > 0 && Aliens.Settings.mSpaceRockBonus > 0;
        }

        public override string GetTitlePrefix()
        {
            return "SpaceRockOnLotThreshold";
        }

        protected override int Validate(int value)
        {
            if (value < 1)
                return 1;

            return value;
        }
    }
}
