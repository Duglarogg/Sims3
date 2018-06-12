using System;
using System.Collections.Generic;
using System.Text;
using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.SimIFace;
using Sims3.UI.Hud;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class LycanthropySetting : SkillRangeOption, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int[] Setting => Aliens.Settings.mLycanthropySkill;

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP7) && Aliens.Settings.mAllowOccultAliens 
                && Aliens.Settings.mValidAlienOccults.Contains(OccultTypes.Werewolf);
        }

        public override string GetTitlePrefix()
        {
            return "Lycanthropy";
        }
    }
}
