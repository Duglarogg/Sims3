using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienPregnancy
{
    public class ListingOption : OptionList<IAlienPregnancyOption>, IPrimaryOption<GameObject>
    {
        public override string GetTitlePrefix()
        {
            return "AlienPregnancyInteraction";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return null; }
        }

        public override List<IAlienPregnancyOption> GetOptions()
        {
            return base.GetOptions();
        }
    }
}
