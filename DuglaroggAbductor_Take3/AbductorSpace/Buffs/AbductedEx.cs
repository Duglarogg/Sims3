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
            Sim abductee = bm.Actor;

            if (abductee.TraitManager.HasElement(AlienUtilsEx.sAlienPregnancy))
            {
                BuffInstanceAbductedEx instance = bi as BuffInstanceAbductedEx;
                instance.Abductee = abductee;
                instance.Alien = SimDescription.Find(abductee.SimDescription.Pregnancy.DadDescriptionId);
                instance.IsAlienPregnancy = true;
                EventTracker.SendEvent(EventTypeId.kGotPregnant, abductee);
                abductee.TraitManager.RemoveElement(AlienUtilsEx.sAlienPregnancy);
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
                bi.mAbductee = mAbductee;
                bi.mAlien = mAlien;
                bi.mIsAlienPregnancy = mIsAlienPregnancy;

                return bi;
            }
        }
    }
}
