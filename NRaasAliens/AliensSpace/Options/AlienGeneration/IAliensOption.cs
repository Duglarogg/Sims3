using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public interface IAliensOption : IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>> { }
}
