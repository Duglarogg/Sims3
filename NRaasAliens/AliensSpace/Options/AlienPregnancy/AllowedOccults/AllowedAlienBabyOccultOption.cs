using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienPregnancy.AllowedOccults
{
    public abstract class AllowedAlienBabyOccultOption : AllowedOccultOption, IAllowedAlienBabyOccultsOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        protected override List<OccultTypes> Setting => Aliens.Settings.mAllowedBabyOccults;
    }
}
