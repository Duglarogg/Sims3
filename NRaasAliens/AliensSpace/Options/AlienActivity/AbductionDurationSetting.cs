using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienActivity
{
    public class AbductionDurationSetting : IntegerSettingOption<GameObject>, IAlienActivityOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override int Value
        {
            get => Aliens.Settings.mAbductionLength;
            set => Aliens.Settings.mAbductionLength = Validate(value);
        }

        public string Name => throw new NotImplementedException();

        public ThumbnailKey Thumbnail => throw new NotImplementedException();

        public bool UsingCount => throw new NotImplementedException();

        public int Count { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string DisplayKey => throw new NotImplementedException();

        public int ValueWidth => throw new NotImplementedException();

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return Aliens.Settings.mBaseActivityChance > 0 && Aliens.Settings.mBaseAbductionChance > 0;
        }

        public override string GetTitlePrefix()
        {
            return "AbductionDuration";
        }

        protected override int Validate(int value)
        {
            if (value < 1)
                return 1;

            if (value > 120)
                return 120;

            return value;
        }

        public bool Test(GameHitParameters<GameObject> parameters)
        {
            throw new NotImplementedException();
        }

        public OptionResult Perform(GameHitParameters<GameObject> parameters)
        {
            throw new NotImplementedException();
        }

        public ICommonOptionItem Clone()
        {
            throw new NotImplementedException();
        }
    }
}
