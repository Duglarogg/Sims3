using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class AllowOccultAliensSetting : BooleanSettingOption<GameObject>, IAliensOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override bool Value
        {
            get => Aliens.Settings.mAllowOccultAliens;
            set => Aliens.Settings.mAllowOccultAliens = value;
        }

        public override string GetTitlePrefix()
        {
            return "AllowOccultAliens";
        }
    }
}
