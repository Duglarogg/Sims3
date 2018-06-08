using System;
using System.Collections.Generic;
using System.Text;
using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.SimIFace;
using Sims3.UI.Hud;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class GardeningSetting : SkillRangeOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int[] Setting => Aliens.Settings.mGardeningSkill;

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mAllowOccultAliens && ((GameUtils.IsInstalled(ProductVersion.EP7) 
                && Aliens.Settings.mValidAlienOccults.Contains(OccultTypes.Fairy)) || (GameUtils.IsInstalled(ProductVersion.EP9)
                && Aliens.Settings.mValidAlienOccults.Contains(OccultTypes.PlantSim)));
        }

        public override string GetTitlePrefix()
        {
            return "Gardening";
        }
    }
}
