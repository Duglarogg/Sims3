using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class AllowOccultBabiesSetting : BooleanSettingOption<GameObject>, IAlienPregnancyOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override bool Value
        {
            get => Aliens.Settings.mAllowOccultBabies;
            set => Aliens.Settings.mAllowOccultBabies = value;
        }

        public override string GetTitlePrefix()
        {
            return "AllowOccultBabies";
        }
    }
}
