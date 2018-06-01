using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienActivity.AlienActivityBonuses
{
    public class HighLTRBonusSetting : IntegerSettingOption<GameObject>, IAlienActivityBonusesOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => throw new NotImplementedException();

        protected override int Value
        {
            get => Aliens.Settings.mHighLTRBonus;
            set => Aliens.Settings.mHighLTRBonus = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mBaseVisitChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "HighLTRBonus";
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
