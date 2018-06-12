using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options
{
    public abstract class ToggleSettingOption<TType> : BooleanSettingOption<GameObject>
    {
        protected abstract TType Type { get; }

        protected abstract List<TType> Setting { get; }

        protected override bool Value
        {
            get => Setting.Contains(Type);
            set => Toggle(value);
        }

        public override string GetTitlePrefix()
        {
            return null;
        }

        public override void SetImportValue(string value)
        {
            base.SetImportValue(value);

            if (Value && !Setting.Contains(Type))
                Setting.Add(Type);

            if (!Value && Setting.Contains(Type))
                Setting.Remove(Type);
        }

        public bool Toggle(bool value)
        {
            if (value && !Setting.Contains(Type))
                Setting.Add(Type);

            if (!value && Setting.Contains(Type))
                Setting.Remove(Type);

            return Setting.Contains(Type);
        }
    }
}
