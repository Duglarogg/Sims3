using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienActivity
{
    public class EarliestHourSetting : IntegerSettingOption<GameObject>, IAlienActivityOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mEarliestHour;
            set => Aliens.Settings.mEarliestHour = Validate(value);
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mBaseActivityChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "EarliestHour";
        }

        protected override int Validate(int value)
        {
            if (value < 0)
                return 0;

            if (value > 23)
                return 23;

            return value;
        }
    }
}
