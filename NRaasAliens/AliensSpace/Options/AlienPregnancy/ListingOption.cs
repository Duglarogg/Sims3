using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class ListingOption : OptionList<IAlienPregnancyOption>, IPrimaryOption<GameObject>, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        public override ITitlePrefixOption ParentListingOption => null;

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mBaseAbductionChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "AlienPregnancyInteraction";
        }
    }
}
