using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.UI;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options
{
    public abstract class AllowedOccultOption : BooleanSettingOption<GameObject>, IAllowedOccultsOption, IOptionItem, IInteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICommonOptionItem
    {
        protected abstract OccultTypes Type { get; }

        protected abstract List<OccultTypes> Setting { get; }

        public override string GetTitlePrefix()
        {
            return null;
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return OccultTypeHelper.IsInstalled(Type);
        }

        public override string Name => OccultTypeHelper.GetLocalizedName(Type);

        protected override OptionResult Run(GameHitParameters<GameObject> parameters)
        {
            string prompt = GetPrompt();

            if (prompt != null && !AcceptCancelDialog.Show(prompt))
                return OptionResult.Failure;

            Value = !Value;
            
            if (Value && !Setting.Contains(Type))
                Setting.Add(Type);

            if (!Value && Setting.Contains(Type))
                Setting.Remove(Type);

            Common.Notify(ToString());

            return OptionResult.SuccessRetain;
        }

        protected override bool Value
        {
            get => Setting.Contains(Type);
            set => Value = !Value;
        }
    }
}
