using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienActivity.AlienActivityBonuses
{
    public class OffspringBonusSetting : IntegerSettingOption<GameObject>, IAlienActivityBonusesOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mOffspringBonus;
            set => Aliens.Settings.mOffspringBonus = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mBaseVisitChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "OffspringBonus";
        }

        protected override int Validate(int value)
        {
            if (value < 0)
                return 0;

            if (value > 100)
                return 100;

            return value;
        }
    }
}
