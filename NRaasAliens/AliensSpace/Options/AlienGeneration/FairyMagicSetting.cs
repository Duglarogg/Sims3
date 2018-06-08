using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class FairyMagicSetting : SkillRangeOption, IAliensOption
    {
        protected override int[] Setting => Aliens.Settings.mFairyMagicSkill;

        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP7) && Aliens.Settings.mAllowOccultAliens 
                && Aliens.Settings.mValidAlienOccults.Contains(OccultTypes.Fairy);
        }

        public override string GetTitlePrefix()
        {
            return "FairyMagic";
        }
    }
}
