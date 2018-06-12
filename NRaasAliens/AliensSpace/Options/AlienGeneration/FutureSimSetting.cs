using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class FutureSimSetting : BooleanSettingOption<GameObject>, IAliensOption
    {
        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP11);
        }

        public override string GetTitlePrefix()
        {
            return "FutureSim";
        }

        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override bool Value
        {
            get => Aliens.Settings.mFutureSim;
            set => Aliens.Settings.mFutureSim = value;
        }
    }
}
