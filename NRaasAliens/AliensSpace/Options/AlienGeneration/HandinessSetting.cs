using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class HandinessSetting : SkillRangeOption, IAliensOption
    {
        protected override int[] Setting => Aliens.Settings.mHandinessSkill;

        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        public override string GetTitlePrefix()
        {
            return "Handiness";
        }
    }
}
