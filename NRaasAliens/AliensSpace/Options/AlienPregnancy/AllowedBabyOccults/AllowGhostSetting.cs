using System;
using System.Collections.Generic;
using System.Text;
using Sims3.UI.Hud;

namespace NRaas.AliensSpace.Options.AlienPregnancy.AllowedBabyOccults
{
    public class AllowGhostSetting : AllowedBabyOccultsOption, IAllowedBabyOccultsOption
    {
        protected override OccultTypes Type => OccultTypes.Ghost;
    }
}
