using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class AlienScienceSetting : BooleanSettingOption<GameObject>, IAliensOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override bool Value
        {
            get => Aliens.Settings.mAlienScience;
            set => Aliens.Settings.mAlienScience = value;
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP9);
        }

        public override string GetTitlePrefix()
        {
            return "AlienScience";
        }
    }
}
