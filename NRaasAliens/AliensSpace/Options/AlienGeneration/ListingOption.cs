using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class ListingOption : OptionList<IAliensOption>, IPrimaryOption<GameObject>
    {
        public override List<IAliensOption> GetOptions()
        {
            return base.GetOptions();
        }

        public override string GetTitlePrefix()
        {
            return "AliensInteraction";
        }

        public override ITitlePrefixOption ParentListingOption => null;
    }
}
