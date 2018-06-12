using System;
using System.Collections.Generic;
using System.Text;
using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.SimIFace;
using Sims3.UI.Hud;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class SpellcastingSetting : SkillRangeOption, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int[] Setting => Aliens.Settings.mSpellcastingSkill;

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP7) && Aliens.Settings.mAllowOccultAliens &&
                Aliens.Settings.mValidAlienOccults.Contains(OccultTypes.Witch);
        }

        public override string GetTitlePrefix()
        {
            return "Spellcasting";
        }
    }
}
