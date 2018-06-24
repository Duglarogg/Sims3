using NRaas.AliensSpace.Helpers;
using NRaas.AliensSpace.Interactions;
using NRaas.AliensSpace.Proxies;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI.HUD;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Buffs
{
    public class BuffAbductedEx : SharedTemperatureBuffs
    {
        private const ulong kGuid = 0xC4DB14A3558B7B27;

        public static ulong GUID => kGuid;

        public class BuffInstanceAbductedEx : BuffInstance
        {
            public Sim Abductee { get; set; }
            public SimDescription Alien { get; set; }
            public bool IsAlienPregnant { get; set; }

            public BuffInstanceAbductedEx() { }

            public BuffInstanceAbductedEx(Buff buff, BuffNames buffGuid, int effectValue, float timeoutCount)
                : base (buff, buffGuid, effectValue, timeoutCount) { }

            public override BuffInstance Clone()
            {
                return new BuffInstanceAbductedEx(mBuff, mBuffGuid, mEffectValue, mTimeoutCount);
            }            
        }

        public static int sInspectChance = 33;
        public static int sInspectFrequency = 60;

        public BuffAbductedEx(BuffData info) : base(info) { }

        public override BuffInstance CreateBuffInstance()
        {
            return new BuffInstanceAbductedEx(this, BuffGuid, EffectValue, TimeoutSimMinutes);
        }

        public override void OnAddition(BuffManager bm, BuffInstance bi, bool travelReaddition)
        {
            SimDescription description = bm.Actor.SimDescription;
            BuffInstanceAbductedEx buffInstance = bi as BuffInstanceAbductedEx;
            buffInstance.Abductee = bm.Actor;
            List<SimDescription> list = new List<SimDescription>();

            foreach(SimDescription current in Household.AlienHousehold.SimDescriptions)
            {
                if (current.TeenOrAbove)
                    list.Add(current);
            }

            if (list.Count > 0)
            {
                buffInstance.Alien = RandomUtil.GetRandomObjectFromList(list);

                if (AlienPregnancy.ShouldImpregnate(buffInstance.Abductee, buffInstance.Alien))
                {
                    AlienPregnancy.Start(buffInstance.Abductee, buffInstance.Alien);
                    buffInstance.IsAlienPregnant = true;
                    return;
                }
            }

            buffInstance.IsAlienPregnant = false;
        }
    }
}
