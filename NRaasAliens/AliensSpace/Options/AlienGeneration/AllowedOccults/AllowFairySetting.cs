using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedOccults
{
    public class AllowFairySetting : AllowedAlienOccultOption, IAllowedAlienOccultsOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override OccultTypes Type => OccultTypes.Fairy;
    }
}
