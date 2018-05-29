using Duglarogg.AbductorSpace.Helpers;
using Duglarogg.AbductorSpace.Interactions;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Buffs
{
    public class Xenogenesis : Buff
    {
        public Xenogenesis(BuffData info) : base(info)
        { }

        public override BuffInstance CreateBuffInstance()
        {
            return new BuffInstanceXenogenesis(this, BuffGuid, EffectValue, TimeoutSimMinutes);
        }

        public override void OnAddition(BuffManager bm, BuffInstance bi, bool travelReaddition)
        {
            Sim abductee = bm.Actor;
            BuffInstanceXenogenesis instance = bi as BuffInstanceXenogenesis;
            instance.Abductee = abductee;
            instance.Alien = SimDescription.Find(abductee.SimDescription.Pregnancy.DadDescriptionId);
            instance.Pregnancy = abductee.SimDescription.Pregnancy;
            instance.StartPregnancy();
        }

        public class BuffInstanceXenogenesis : BuffInstance
        {
            Sim mAbductee;
            SimDescription mAlien;
            Pregnancy mPregnancy;

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

            public Pregnancy Pregnancy
            {
                get { return mPregnancy; }
                set { mPregnancy = value; }
            }

            public BuffInstanceXenogenesis()
            { }

            public BuffInstanceXenogenesis(Buff buff, BuffNames buffGuid, int effectValue, float timeoutCount)
                : base(buff, buffGuid, effectValue, timeoutCount)
            { }

            public void CancelReaction(Sim sim, ReactionBroadcaster broadcaster)
            {
                sim.InteractionQueue.CancelInteractionByType(ReactToContractionEx.Singleton);
            }

            public override BuffInstance Clone()
            {
                BuffInstanceXenogenesis bi = new BuffInstanceXenogenesis(mBuff, mBuffGuid, mEffectValue, mTimeoutCount);
                bi.mAbductee = mAbductee;
                bi.mAlien = mAlien;
                bi.mPregnancy = mPregnancy;

                return bi;
            }

            public void HaveTheBaby()
            {
                if (Pregnancy.mContractionBroadcast != null)
                {
                    Pregnancy.mContractionBroadcast.Dispose();
                }

                Abductee.RemoveAlarm(Pregnancy.PreggersAlarm);
                Abductee.RemoveAlarm(Pregnancy.mContractionsAlarm);
                bool flag = false;

                foreach (InteractionInstance current in Abductee.InteractionQueue.InteractionList)
                {
                    HaveAlienBabyHospital haveBabyHospital = current as HaveAlienBabyHospital;

                    if (haveBabyHospital != null)
                    {
                        haveBabyHospital.CancellableByPlayer = false;
                        haveBabyHospital.BabyShouldBeBorn = true;
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    List<RabbitHole> hospitals = RabbitHole.GetRabbitHolesOfType(RabbitHoleType.Hospital);
                    float distanceToBirthplace = Abductee.LotHome.GetDistanceToObject(Abductee);
                    RabbitHole hospital = null;

                    foreach (RabbitHole current in hospitals)
                    {
                        float distanceToHospital = current.RabbitHoleProxy.GetDistanceToObject(Abductee);

                        if (distanceToHospital < distanceToBirthplace)
                        {
                            distanceToBirthplace = distanceToHospital;
                            hospital = current;
                        }
                    }

                    InteractionInstance instance;

                    if (hospital != null)
                    {
                        instance = HaveAlienBabyHospital.Singleton.CreateInstance(hospital, Abductee, 
                            new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false);
                        (instance as HaveAlienBabyHospital).BabyShouldBeBorn = true;
                    }
                    else
                    {
                        instance = HaveAlienBabyHome.Singleton.CreateInstance(Abductee.LotHome, Abductee,
                            new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false);
                    }

                    Abductee.InteractionQueue.Add(instance);
                    ActiveTopic.AddToSim(Abductee, "Recently Had Baby");
                }
            }

            public void HourlyCallback()
            {
                if (GameUtils.IsOnVacation() || GameUtils.IsUniversityWorld())
                {
                    return;
                }

                Pregnancy.mHourOfPregnancy++;

                if (Pregnancy.mHourOfPregnancy >= Abductor.Settings.mHourToStartPregnantWalk)
                {
                    ActiveTopic.AddToSim(Abductee, "Pregnant", Abductee.SimDescription);

                    if (!Pregnancy.mHasRequestedWalkStyle)
                    {
                        Pregnancy.mHasRequestedWalkStyle = Abductee.RequestWalkStyle(Sim.WalkStyle.Pregnant);
                    }
                }

                if (Pregnancy.mHourOfPregnancy == Abductor.Settings.mHourToStartLabor)
                {
                    for (int i = 0; i < Pregnancy.kNumberOfPuddlesForWaterBreak; i++)
                    {
                        PuddleManager.AddPuddle(Abductee.PositionOnFloor);
                    }

                    if (Abductee.IsSelectable)
                    {
                        StyledNotification.Show(new StyledNotification.Format(
                            Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:BabyIsComingTNS", new object[] { Abductee }),
                            StyledNotification.NotificationStyle.kGameMessageNegative));
                    }

                    //Abductee.BuffManager.RemoveElement(AlienUtilsEx.sXenogenesis);
                    Abductee.BuffManager.AddElement(AlienUtilsEx.sBabyIsComing, Origin.FromPregnancy);

                    if (Pregnancy.mContractionBroadcast != null)
                    {
                        Pregnancy.mContractionBroadcast.Dispose();
                    }

                    Pregnancy.mContractionBroadcast = new ReactionBroadcaster(Abductee, Pregnancy.kContractionBroadcasterParams,
                        new ReactionBroadcaster.BroadcastCallback(StartReaction), new ReactionBroadcaster.BroadcastCallback(CancelReaction));

                    Abductee.AddInteraction(TakeToHospitalEx.Singleton);
                    InteractionInstance interaction = Pregnancy.HaveContraction.Singleton.CreateInstance(Abductee, Abductee,
                        new InteractionPriority(InteractionPriorityLevel.High, 10f), false, false);
                    interaction.Hidden = true;
                    Abductee.InteractionQueue.Add(interaction);
                    Pregnancy.mContractionsAlarm = Abductee.AddAlarmRepeating(5f, TimeUnit.Minutes, new AlarmTimerCallback(Pregnancy.TriggerContraction),
                        5f, TimeUnit.Minutes, "Alien Pregnancy Trigger Contractions Alarm", AlarmType.AlwaysPersisted);
                    EventTracker.SendEvent(EventTypeId.kPregnancyContractionsStarted, Abductee);
                }

                if (Pregnancy.mHourOfPregnancy == Abductor.Settings.mHoursOfPregnancy)
                {
                    HaveTheBaby();
                }

                SetPregnancyMorph();
            }

            public void SetPregnancyMorph()
            {
                int num = Math.Min(Pregnancy.mHourOfPregnancy, Abductor.Settings.mHoursToShowPregnantMorph);

                if (num >= 0)
                {
                    float num2 = num / Abductor.Settings.mHoursToShowPregnantMorph;

                    if (num2 == 0f)
                    {
                        num2 = 0.01f;
                    }

                    Abductee.SimDescription.SetPregnancy(num2, false);

                    if (RandomUtil.RandomChance(Pregnancy.kChanceOfBackache))
                    {
                        Abductee.BuffManager.AddElement(BuffNames.Backache, Origin.FromPregnancy);
                    }
                }
            }

            public void StartPregnancy()
            {
                AgingManager.Singleton.CancelAgingAlarmsForSim(Abductee.SimDescription.AgingState);
                Pregnancy.PreggersAlarm = Abductee.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(HourlyCallback),
                    1f, TimeUnit.Hours, "Hourly Alien Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
            }

            public void StartReaction(Sim sim, ReactionBroadcaster broadcaster)
            {
                if (sim.SimDescription.ChildOrAbove)
                {
                    sim.InteractionQueue.Add(ReactToContractionEx.Singleton.CreateInstance(Abductee, sim,
                        new InteractionPriority(InteractionPriorityLevel.Pregnancy, -1f), true, true));
                }
            }
        }
    }
}
