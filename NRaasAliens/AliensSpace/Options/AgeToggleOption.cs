using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options
{
    public abstract class AgeToggleOption : ToggleSettingOption<CASAgeGenderFlags>
    {
        public override string Name => Common.LocalizeEAString("UI/Feedback/CAS:" + Type);
    }
}
