﻿using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class HandinessSetting : IntegerRangeSettingOption<GameObject>, IAliensOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override Pair<int, int> Value
        {
            get => Aliens.Settings.mHandinessSkill;
            set => Aliens.Settings.mHandinessSkill = Validate(value.First, value.Second);
        }

        public override string GetTitlePrefix()
        {
            return "Handiness";
        }

        protected override Pair<int, int> Validate(int value1, int value2)
        {
            Pair<int, int> result = base.Validate(value1, value2);

            if (result.First < 0)
                result.First = 0;

            if (result.Second > 10)
                result.Second = 10;

            return result;
        }
    }
}
