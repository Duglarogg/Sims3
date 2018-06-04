﻿using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options
{
    [Persistable]
    public abstract class OptionItem : OperationSettingOption<GameObject>, ICommonOptionItem
    {
    }
}
