using Duglarogg.AbductorSpace.Helpers;
using Duglarogg.AbductorSpace.Proxies;
//using NRaas;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class HaveAlienBabyHospital : RabbitHole.RabbitHoleInteraction<Sim, RabbitHole>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public bool BabyShouldBeBorn = false;
        public bool BabyBorn = false;
        public List<Sim> mNewborns = null;

        public static void AddInteraction(RabbitHole hospital)
        {
            foreach (InteractionObjectPair pair in hospital.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == Singleton.GetType())
                {
                    return;
                }
            }

            hospital.AddInteraction(Singleton);
        }

        public override void Cleanup()
        {
            bool wasPregnant = (Actor.SimDescription.Pregnancy != null);

            try
            {
                if (!Actor.SimDescription.IsVampire)
                {
                    Actor.Motives.CreateMotive(CommodityKind.Hunger);
                }

                BabyShouldBeBorn = false;
                BabyBorn = true;
                Sims3.Gameplay.Gameflow.Singleton.EnableSave(this);
            }
            catch (ResetException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (wasPregnant)
                {
                    Logger.WriteExceptionLog(e, this, "HaveAlienBabyHospital.Cleanup() Error - Was Pregnant");
                }
                else
                {
                    Logger.WriteExceptionLog(e, this, "HaveAlienBabyHospital.Cleanup() Error - Wasn't Pregnant");
                }
            }
            finally
            {
                Sims3.Gameplay.Gameflow.Singleton.EnableSave(this);
            }

            base.Cleanup();
        }

        public override bool InRabbitHole()
        {
            string msg = "HaveAlienBabyHosptial:InRabbitHole" + "\n";

            try
            {
                while (!Actor.WaitForExitReason(Sim.kWaitForExitReasonDefaultTime, ExitReason.CanceledByScript))
                {
                    if (BabyShouldBeBorn)
                    {
                        break;
                    }
                }

                msg += "A";

                if (!BabyShouldBeBorn && Actor.HasExitReason(ExitReason.CanceledByScript))
                {
                    return false;
                }

                msg += "B";

                Pregnancy pregnancy = Actor.SimDescription.Pregnancy;
                bool isSelectable = Actor.IsSelectable;
                Sims3.Gameplay.Gameflow.Singleton.DisableSave(this, "Gameplay/ActorSystems/Pregnancy:DisableSave");
                mNewborns = new PregnancyProxy(pregnancy).CreateNewborns(Pregnancy.HaveBabyHospital.kBonusMoodPointsForHospitalBirth, isSelectable, false);

                msg += "C";

                Actor.SimDescription.SetPregnancy(0f);
                List<Sim> simFollowers = SimFollowers;
                new PregnancyProxy(pregnancy).PregnancyComplete(mNewborns, simFollowers);

                /*
                if (mNewborns.Count == 4)
                {
                    Actor.BuffManager.RemoveElement(BuffNames.ItsABoy);
                    Actor.BuffManager.AddElement(CommonPregnancy.sItsQuadruplets, Origin.FromNewBaby);
                }
                */

                msg += "D";

                Simulator.Sleep(0u);
                //SpeedTrap.Sleep(0x0);

                List<Sim> list2 = new List<Sim>();
                list2.Add(Actor);

                if (simFollowers != null)
                {
                    foreach (Sim sim in simFollowers)
                    {
                        if (sim.SimDescription.TeenOrAbove && sim.GetObjectInRightHand() == null)
                        {
                            list2.Add(sim);
                        }
                    }
                }

                msg += "E";

                if (mNewborns.Count <= list2.Count)
                {
                    for (int i = 0; i < mNewborns.Count; i++)
                    {
                        Sim target = list2[i];
                        Posture posture = target.Posture;
                        target.Posture = null;
                        Sim actor = mNewborns[i];
                        InteractionInstance entry = Pregnancy.PregnancyPlaceholderInteraction.Singleton.CreateInstance(target, actor,
                            new InteractionPriority(InteractionPriorityLevel.Zero), false, false);
                        actor.InteractionQueue.Add(entry);

                        while (actor.CurrentInteraction != entry && actor.InteractionQueue.HasInteraction(entry))
                        {
                            Simulator.Sleep(0u);
                            //SpeedTrap.Sleep();
                        }

                        try
                        {
                            ChildUtils.CarryChild(target, actor, false);
                        }
                        catch (Exception e)
                        {
                            Logger.WriteExceptionLog(e, this, "HaveAlienBabyHospital.Run() Error - CarryChild()");
                        }

                        target.Posture = posture;
                        AddFollower(mNewborns[i]);
                    }
                }
                else
                {
                    BabyBasket basket = GlobalFunctions.CreateObject("BabyBasket", Vector3.OutOfWorld, 0, Vector3.UnitZ) as BabyBasket;
                    basket.AddBabiesToBasket(mNewborns);
                    CarrySystem.EnterWhileHolding(Actor, basket);
                    CarrySystem.VerifyAnimationParent(basket, Actor);
                }

                msg += "F";

                if (Actor.IsSelectable)
                {
                    OccultImaginaryFriend.DeliverDollToHousehold(mNewborns);
                }

                return true;
            }
            catch (ResetException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.WriteExceptionLog(e, this, "HaveAlienBabyHospital.Run() Error");
                return false;
            }
        }

        public class Definition : InteractionDefinition<Sim, RabbitHole, HaveAlienBabyHospital>, IUsableDuringBirthSequence
        {
            public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/HaveAlienBabyHospital:MenuName");
            }

            public override bool Test(Sim actor, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return (target.Guid == RabbitHoleType.Hospital) && (actor.BuffManager.HasElement(AlienUtilsEx.sBabyIsComing));
            }
        }
    }
}
