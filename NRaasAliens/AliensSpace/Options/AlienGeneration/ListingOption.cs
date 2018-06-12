using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class ListingOption : OptionList<IAliensOption>, IPrimaryOption<GameObject>
    {
        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP8);
        }

        public override List<IAliensOption> GetOptions()
        {
            return base.GetOptions();
        }

        public override string GetTitlePrefix()
        {
            return "AlienGeneration";
        }

        public override ITitlePrefixOption ParentListingOption => null;
    }
}
