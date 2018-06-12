using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class ScienceSetting : SkillRangeOption, IAliensOption
    {
        protected override int[] Setting => Aliens.Settings.mScienceSkill;

        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP9) && Aliens.Settings.mAlienScience;
        }

        public override string GetTitlePrefix()
        {
            return "Science";
        }
    }
}
