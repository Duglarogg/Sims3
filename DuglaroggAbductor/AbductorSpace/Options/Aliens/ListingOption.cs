using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.Aliens
{
    public class ListingOption : OptionList<IAliensOption>, IPrimaryOption<GameObject>
    {
        public override string GetTitlePrefix()
        {
            return "AliensInteraction";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return null; }
        }

        public override List<IAliensOption> GetOptions()
        {
            return base.GetOptions();
        }
    }
}
