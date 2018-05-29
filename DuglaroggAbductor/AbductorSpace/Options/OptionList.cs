using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options
{
    public abstract class OptionList<T> : InteractionOptionList<T, GameObject>, IOptionItem
        where T : class, IOptionItem
    { }
}
