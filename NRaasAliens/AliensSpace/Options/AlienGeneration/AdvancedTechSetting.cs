using NRaas.CommonSpace.Options;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Options.AlienGeneration
{
    public class AdvancedTechSetting : IntegerRangeSettingOption<GameObject>, IAliensOption
    {
        public override ITitlePrefixOption ParentListingOption => new ListingOption();

        protected override Pair<int, int> Value
        {
            get => Aliens.Settings.mFutureSkill;
            set => Aliens.Settings.mFutureSkill = Validate(value.First, value.Second);
        }

        public string Name => throw new NotImplementedException();

        public ThumbnailKey Thumbnail => throw new NotImplementedException();

        public string DisplayValue => throw new NotImplementedException();

        public bool UsingCount => throw new NotImplementedException();

        public int Count { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string DisplayKey => throw new NotImplementedException();

        public int ValueWidth => throw new NotImplementedException();

        protected override bool Allow(GameHitParameters<GameObject> parameters)
        {
            return GameUtils.IsInstalled(ProductVersion.EP11) && Aliens.Settings.mFutureSim;
        }

        public override string GetTitlePrefix()
        {
            return "AdvancedTech";
        }

        protected override Pair<int, int> Validate(int value1, int value2)
        {
            Pair<int, int> result = base.Validate(value1, value2);

            if (result.First < 0)
                result.First = 0;

            if (result.Second > 10)
                result.Second = 10;

            return result;
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
