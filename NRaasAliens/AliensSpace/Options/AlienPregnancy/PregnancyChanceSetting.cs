using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class PregnancyChanceSetting : IntegerSettingOption<GameObject>, IAlienPregnancyOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mPregnancyChance;
            set => Aliens.Settings.mPregnancyChance = Validate(value);
        }

        public override string GetTitlePrefix()
        {
            return "PregnancyChance";
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
