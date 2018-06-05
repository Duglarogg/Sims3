using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class UseFertilitySetting : BooleanSettingOption<GameObject>, IAlienPregnancyOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override bool Value
        {
            get => Aliens.Settings.mUseFertility;
            set => Aliens.Settings.mUseFertility = value;
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mPregnancyChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "UseFertility";
        }
    }
}
