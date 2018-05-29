using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.Aliens
{
    public class FutureSimSetting : BooleanSettingOption<GameObject>, IAliensOption
    {
        protected override bool Value
        {
            get
            {
                return Abductor.Settings.mFutureSim;
            }

            set
            {
                Abductor.Settings.mFutureSim = value;
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP11);
        }

        public override string GetTitlePrefix()
        {
            return "FutureSim";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
