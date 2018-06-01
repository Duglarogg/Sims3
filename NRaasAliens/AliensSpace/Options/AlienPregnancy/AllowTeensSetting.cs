using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class AllowTeensSetting : BooleanSettingOption<GameObject>, IAlienPregnancyOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override bool Value
        {
            get => Aliens.Settings.mAllowTeens;
            set => Aliens.Settings.mAllowTeens = value;
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mPregnancyChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "AllowTeens";
        }
    }
}
