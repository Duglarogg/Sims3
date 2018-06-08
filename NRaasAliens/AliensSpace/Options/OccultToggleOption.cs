using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options
{
    public abstract class AllowedAlienOccultsOption : ToggleSettingOption<OccultTypes>
    {
        public override string Name => OccultTypeHelper.GetLocalizedName(Type);

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return OccultTypeHelper.IsInstalled(Type);
        }
    }
}
