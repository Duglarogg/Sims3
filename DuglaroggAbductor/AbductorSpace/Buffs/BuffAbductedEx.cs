using NRaas.AbductorSpace.Helpers;
using NRaas.AbductorSpace.Interactions;
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

namespace NRaas.AbductorSpace.Buffs
{
    public class BuffAbductedEx : SharedTemperatureBuffs
    {
        //                          0x41B8A22DF45918A4
        //private const ulong kGuid = 0x41B8A22DF45918A4;
        private const ulong kGuid = 0xCFDEF00B5671CB3F;

        public static ulong GUID
        {
            get { return kGuid; }
        }

        public BuffAbductedEx(BuffData info) : base(info)
        { }

        public override BuffInstance CreateBuffInstance()
        {
            return new BuffInstanceAbductedEx(this, BuffGuid, EffectValue, TimeoutSimMinutes);
        }
        
        /*
        public override void OnAddition(BuffManager bm, BuffInstance bi, bool travelReaddition)
        {
            SimDescription description = bm.Actor.SimDescription;
            BuffInstanceAbductedEx instance = bi as BuffInstanceAbductedEx;

            if (CommonPregnancy.IsSuccess(instance.Abductee, instance.Abductor.CreatedSim, instance.IsAutonomous))
            {
                Pregnancy pregnancy = CommonPregnancy.CreatePregnancy(instance.Abductee, instance.Abductor, !CommonPregnancy.AllowPlantSimPregnancy());

                if (pregnancy != null)
                {
                    instance.IsAlienPregnancy = true;
                    instance.Abductee.SimDescription.Pregnancy = pregnancy;
                    EventTracker.SendEvent(EventTypeId.kGotPregnant, instance.Abductee);
                }
            }
        }
        */

        public override void OnTimeout(BuffManager bm, BuffInstance bi, OnTimeoutReasons reason)
        {
            BuffInstanceAbductedEx instance = bi as BuffInstanceAbductedEx;

            if (instance.IsAlienPregnancy)
            {
                InteractionInstance interaction = ShowAlienPregnancy.Singleton.CreateInstance(instance.Abductee, instance.Abductor.CreatedSim,
                    new InteractionPriority(InteractionPriorityLevel.ESRB), false, false);
                interaction.Hidden = true;
                instance.Abductee.InteractionQueue.AddNext(interaction);
            }
        }

        public class BuffInstanceAbductedEx : BuffInstance
        {
            private SimDescription mAbductor;
            private Sim mAbductee;
            private bool mIsAutonomous;
            private bool mIsAlienPregnancy;

            public Sim Abductee
            {
                get { return mAbductee; }
                set { mAbductee = value; }
            }

            public SimDescription Abductor
            {
                get { return mAbductor; }
                set { mAbductor = value; }
            }

            public bool IsAlienPregnancy
            {
                get { return mIsAlienPregnancy; }
                set { mIsAlienPregnancy = value; }
            }

            public bool IsAutonomous
            {
                get { return mIsAutonomous; }
                set { mIsAutonomous = value; }
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
                bi.Abductor = mAbductor;
                bi.IsAutonomous = mIsAutonomous;
                bi.IsAlienPregnancy = mIsAlienPregnancy;

                return bi;
            }

            public void Impregnate()
            {
                if (CommonPregnancy.IsSuccess(Abductee, Abductor.CreatedSim, IsAutonomous))
                {
                    Pregnancy pregnancy = CommonPregnancy.CreatePregnancy(Abductee, Abductor, !CommonPregnancy.AllowPlantSimPregnancy());

                    if (pregnancy != null)
                    {
                        IsAlienPregnancy = true;
                        Abductee.SimDescription.Pregnancy = pregnancy;
                        EventTracker.SendEvent(EventTypeId.kGotPregnant, Abductee);
                    }
                }
            }
        }
    }
}
