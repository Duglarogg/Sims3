using Duglarogg.AbductorSpace.Helpers;
using Duglarogg.AbductorSpace.Interactions;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Buffs
{
    public class AbductedEx : Buff
    {
        public AbductedEx(BuffData info) : base(info)
        { }

        public override BuffInstance CreateBuffInstance()
        {
            return new BuffInstanceAbductedEx(this, BuffGuid, EffectValue, TimeoutSimMinutes);
        }

        public override void OnAddition(BuffManager bm, BuffInstance bi, bool travelReaddition)
        {
            BuffInstanceAbductedEx instance = bi as BuffInstanceAbductedEx;

            List<SimDescription> aliens = AlienUtilsEx.GetAliens(true);

            if (aliens == null)
            {
                return;
            }

            instance.Abductee = bm.Actor;
            instance.Alien = RandomUtil.GetRandomObjectFromList<SimDescription>(aliens);
            instance.IsAutonomous = false;

            if (CommonPregnancy.IsSuccess(instance.Abductee, instance.Alien))
            {
                Pregnancy pregnancy = CommonPregnancy.CreatePregnancy(instance.Abductee, instance.Alien, !CommonPregnancy.AllowPlantSimPregnancy());

                if (pregnancy != null)
                {
                    instance.IsAlienPregnancy = true;
                    instance.Abductee.SimDescription.Pregnancy = pregnancy;
                    EventTracker.SendEvent(EventTypeId.kGotPregnant, instance.Abductee);
                }
            }
        }

        public override void OnTimeout(BuffManager bm, BuffInstance bi, OnTimeoutReasons reason)
        {
            BuffInstanceAbductedEx instance = bi as BuffInstanceAbductedEx;

            if (instance.IsAlienPregnancy)
            {
                InteractionInstance interaction = ShowAlienPregnancy.Singleton.CreateInstance(instance.Abductee, 
                    instance.Abductee, new InteractionPriority(InteractionPriorityLevel.ESRB), false, false);
                interaction.Hidden = true;
                instance.Abductee.InteractionQueue.AddNext(interaction);
            }
        }

        public class BuffInstanceAbductedEx : BuffInstance
        {
            SimDescription mAlien;
            Sim mAbductee;
            bool mIsAutonomous = false;
            bool mIsAlienPregnancy = false;

            public Sim Abductee
            {
                get { return mAbductee; }
                set { mAbductee = value; }
            }

            public SimDescription Alien
            {
                get { return mAlien; }
                set { mAlien = value; }
            }

            public bool IsAutonomous
            {
                get { return mIsAutonomous; }
                set { mIsAutonomous = value; }
            }

            public bool IsAlienPregnancy
            {
                get { return mIsAlienPregnancy; }
                set { mIsAlienPregnancy = value; }
            }

            public BuffInstanceAbductedEx()
            { }

            public BuffInstanceAbductedEx(Buff buff, BuffNames buffGuid, int effectValue, float timeoutCount)
                : base(buff, buffGuid, effectValue, timeoutCount)
            { }

            public override BuffInstance Clone()
            {
                BuffInstanceAbductedEx bi = new BuffInstanceAbductedEx(mBuff, mBuffGuid, mEffectValue, mTimeoutCount);
                bi.Abductee = mAbductee;
                bi.Alien = mAlien;
                bi.IsAlienPregnancy = mIsAlienPregnancy;
                bi.IsAutonomous = mIsAutonomous;

                return bi;
            }
        }
    }
}
