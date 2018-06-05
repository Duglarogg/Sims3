using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienActivity
{
    public class ListingOption : OptionList<IAlienActivityOption>, IPrimaryOption<GameObject>
    {
        public override ITitlePrefixOption ParentListingOption => null;

        public override List<IAlienActivityOption> GetOptions()
        {
            return base.GetOptions();
        }

        public override string GetTitlePrefix()
        {
            return "AlienActivity";
        }
    }
}
