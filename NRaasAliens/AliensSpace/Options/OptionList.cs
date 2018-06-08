using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options
{
    public abstract class OptionList<T> : InteractionOptionList<T, GameObject>, IOptionItem where T : class, IOptionItem { }
}
