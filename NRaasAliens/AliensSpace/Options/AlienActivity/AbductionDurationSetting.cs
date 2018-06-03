using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienActivity
{
    public class AbductionDurationSetting : IntegerSettingOption<GameObject>, IAlienActivityOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mAbductionLength;
            set => Aliens.Settings.mAbductionLength = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mBaseActivityChance > 0 && Aliens.Settings.mBaseAbductionChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "AbductionDuration";
        }

        protected override int Validate(int value)
        {
            if (value < 1)
                return 1;

            if (value > 120)
                return 120;

            return value;
        }
    }
}
