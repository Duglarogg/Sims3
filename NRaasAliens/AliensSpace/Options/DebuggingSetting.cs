using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options
{
    public class DebuggingSetting : BooleanSettingOption<GameObject>, IPrimaryOption<GameObject>
    {
        public override string GetTitlePrefix()
        {
            return "Debugging";
        }

        public override ITitlePrefixOption ParentListingOption => null;

        protected override bool Value
        {
            get => Aliens.Settings.Debugging;
            set => Aliens.Settings.Debugging = value;
        }
    }
}
