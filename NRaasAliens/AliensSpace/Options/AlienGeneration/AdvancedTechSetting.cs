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
    public class AdvancedTechSetting : SkillRangeOption, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int[] Setting => Aliens.Settings.mFutureSkill;

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP11) && Aliens.Settings.mFutureSim;
        }

        public override string GetTitlePrefix()
        {
            return "AdvancedTech";
        }
    }
}
