using Duglarogg.AbductorSpace.Helpers;
using Duglarogg.AbductorSpace.Proxies;
//using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Pools;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class HaveAlienBabyHome : Interaction<Sim, Lot>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public List<Sim> mNewborns;
        public static ulong kIconNameHash = ResourceUtils.HashString64("hud_interactions_baby");

        public void AcquirePregnancyStateMachine()
        {
            mCurrentStateMachine = StateMachineClient.Acquire(Actor, "Pregnancy");
        }

        public static void AddInteraction(Lot lot)
        {
            foreach (InteractionObjectPair pair in lot.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == Singleton.GetType())
                {
                    return;
                }
            }

            lot.AddInteraction(Singleton);
        }

        public override void Cleanup()
        {
            Pregnancy pregnancy = Actor.SimDescription.Pregnancy;
            bool wasPregnant = (pregnancy != null);

            try
            {
                if (!Actor.SimDescription.IsVampire)
                {
                    Actor.Motives.CreateMotive(CommodityKind.Hunger);
                }

                try
                {
                    if (mNewborns == null)
                    {
                        mNewborns = new List<Sim>();
                    }

                    if (!Actor.HasBeenDestroyed)
                    {
                        new PregnancyProxy(Actor.SimDescription.Pregnancy).PregnancyComplete(mNewborns, null);
                    }

                    base.Cleanup();
                }
                catch (ResetException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    if (pregnancy != null)
                    {
                        Logger.WriteExceptionLog(e, this, "Duglarogg.AbductorSpace.Proxies.PregnancyProxy.PregnancyComplete() Error");
                    }
                }

                Sims3.Gameplay.Gameflow.Singleton.EnableSave(this);

                if (Actor.IsMale && mNewborns != null)
                {
                    if (!Actor.HasBeenDestroyed && pregnancy != null)
                    {
                        new PregnancyProxy(pregnancy).PregnancyComplete(mNewborns, null);
                    }
                }

                /*
                if (mNewborns != null && mNewborns.Count == 4)
                {
                    if (Actor != null && Actor.BuffManager != null)
                    {
                        Actor.BuffManager.RemoveElement(BuffNames.ItsABoy);
                        Actor.BuffManager.AddElement(PregnancyProxy.sItsQuadruplets, Origin.FromNewBaby);
                    }
                }
                */
            }
            catch (ResetException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.WriteExceptionLog(e, this, "Duglarogg.AbductorSpace.Interactions.HaveAlienBabyHome.Cleanup() Error");
            }
            finally
            {
                Sims3.Gameplay.Gameflow.Singleton.EnableSave(this);
            }
        }

        public override ThumbnailKey GetIconKey()
        {
            return new ThumbnailKey(new ResourceKey(kIconNameHash, 796721156u, 0u), ThumbnailSize.Medium);
        }

        public void GetNewBorns()
        {
            Pregnancy pregnancy = Actor.SimDescription.Pregnancy;
            bool isSelectable = Actor.IsSelectable;
            Sims3.Gameplay.Gameflow.Singleton.DisableSave(this, "Gameplay/ActorSystems/Pregnancy:DisableSave");
            mNewborns = new PregnancyProxy(pregnancy).CreateNewborns(0f, isSelectable, true);
        }

        public override bool Run()
        {
            try
            {
                if (Actor.LotCurrent != Target)
                {
                    Vector3 point = World.LotGetPtInside(Target.LotId);

                    if (point == Vector3.Invalid)
                    {
                        return false;
                    }

                    if (!Actor.RouteToPointRadius(point, 3f) && (!GlobalFunctions.PlaceAtGoodLocation(Actor, new World.FindGoodLocationParams(point), false)
                        || Actor.IsPointInLotSafelyRoutable(Target, Actor.Position)))
                    {
                        Actor.AttemptToPutInSafeLocation(true);
                    }
                }

                if (Actor.Posture is SwimmingInPool)
                {
                    SwimmingInPool posture = Actor.Posture as SwimmingInPool;

                    if (!posture.ContainerPool.RouteToEdge(Actor))
                    {
                        if (Actor.BridgeOrigin != null)
                        {
                            Actor.BridgeOrigin.MakeRequest();
                        }

                        Actor.PopPosture();
                        IGameObject reservedTile = null;
                        Actor.FindRoutablePointInsideNearFrontDoor(Actor.Household.LotHome, out reservedTile);
                        Vector3 position = reservedTile.Position;
                        Terrain.TeleportMeHere here = Terrain.TeleportMeHere.Singleton.CreateInstance(Terrain.Singleton, Actor,
                            new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false) as Terrain.TeleportMeHere;
                        here.SetAndReserveDestination(reservedTile);

                        try
                        {
                            here.RunInteractionWithoutCleanup();
                        }
                        catch
                        {
                            Actor.SetPosition(position);
                        }
                        finally
                        {
                            here.Cleanup();
                        }

                        Actor.LoopIdle();
                    }
                }

                Pregnancy pregnancy = Actor.SimDescription.Pregnancy;
                GetNewBorns();
                AcquirePregnancyStateMachine();
                mCurrentStateMachine.SetActor("x", Actor);

                for (int i = 0; i < mNewborns.Count; i++)
                {
                    Sim baby = mNewborns[i];
                    Relationship.Get(Actor, baby, true).LTR.ForceChangeState(LongTermRelationshipTypes.Friend);

                    if (baby.BridgeOrigin != null)
                    {
                        baby.BridgeOrigin.MakeRequest();
                        baby.BridgeOrigin = null;
                    }

                    InternalHaveAlienBabyHome instance = InternalHaveAlienBabyHome.Singleton.CreateInstance(baby, Actor, 
                        GetPriority(), Autonomous, CancellableByPlayer) as InternalHaveAlienBabyHome;
                    instance.TotalCount = mNewborns.Count;
                    instance.BabyIndex = i + 1;
                    instance.mCurrentStateMachine = mCurrentStateMachine;
                    Actor.InteractionQueue.PushAsContinuation(instance, true);
                }

                TryDeliverImaginaryFriend();
                return true;
            }
            catch (ResetException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.WriteExceptionLog(e, this, "HaveAlienBabyHome.Run() Error");
                return false;
            }
        }

        public void TryDeliverImaginaryFriend()
        {
            if (Actor.IsSelectable)
            {
                OccultImaginaryFriend.DeliverDollToHousehold(mNewborns);
            }
        }

        public class Definition : InteractionDefinition<Sim, Lot, HaveAlienBabyHome>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/HaveAlienBabyHome:MenuName");
            }

            public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return (actor.Household == target.Household) && (actor.BuffManager.HasElement(AlienUtilsEx.sBabyIsComing));
            }
        }
    }
}
