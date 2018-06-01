using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienActivity
{
    public class BaseActivityChanceSetting : IntegerSettingOption<GameObject>, IAlienActivityOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mBaseActivityChance;
            set => Aliens.Settings.mBaseActivityChance = Validate(value);
        }

        public override string GetTitlePrefix()
        {
            return "BaseActivityChance";
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
