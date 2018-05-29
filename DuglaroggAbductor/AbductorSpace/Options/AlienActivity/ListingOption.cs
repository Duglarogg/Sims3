using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienActivity
{
    public class ListingOption : OptionList<IAlienActivityOption>, IPrimaryOption<GameObject>
    {
        public override string GetTitlePrefix()
        {
            return "AlienActivityInteraction";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return null; }
        }

        public override List<IAlienActivityOption> GetOptions()
        {
            return base.GetOptions();
        }
    }
}
