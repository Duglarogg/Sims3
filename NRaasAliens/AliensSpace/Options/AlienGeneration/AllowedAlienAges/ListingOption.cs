﻿using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienAges
{
    public class ListingOption : OptionList<IAllowedAlienAgesOption>, IPrimaryOption<GameObject>, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => null;

        public override string GetTitlePrefix()
        {
            return "AllowedAlienAges";
        }
    }
}