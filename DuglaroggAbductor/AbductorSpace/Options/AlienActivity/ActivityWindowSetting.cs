using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Options.AlienActivity
{
    public class ActivityWindowSetting : IntegerSettingOption<GameObject>, IAlienActivityOption
    {
        protected override int Value
        {
            get
            {
                return Abductor.Settings.mVisitWindow;
            }

            set
            {
                if (value < 0) value = 0;
                if (value > 24) value = 24;
                Abductor.Settings.mVisitWindow = value;
            }
        }

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return (Abductor.Settings.mBaseActivityChance > 0);
        }

        public override string GetTitlePrefix()
        {
            return "ActivityWindow";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return new ListingOption(); }
        }
    }
}
