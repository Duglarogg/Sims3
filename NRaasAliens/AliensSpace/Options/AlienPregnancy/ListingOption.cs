using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy
{
    public class ListingOption : OptionList<IAlienPregnancyOption>, IPrimaryOption<GameObject>
    {
        public override ITitlePrefixOption ParentListingOption => null;

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP8) && Aliens.Settings.mBaseAbductionChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "AlienPregnancy";
        }
    }
}
