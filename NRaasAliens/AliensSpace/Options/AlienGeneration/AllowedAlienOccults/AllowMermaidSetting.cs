﻿using System;
using System.Collections.Generic;
using System.Text;
using Sims3.UI.Hud;

namespace NRaas.AliensSpace.Options.AlienGeneration.AllowedAlienOccults
{
    public class AllowMermaidSetting : AllowedAlienOccultsOption, IAllowedAlienOccultsOption
    {
        protected override OccultTypes Type => OccultTypes.Mermaid;
    }
}
