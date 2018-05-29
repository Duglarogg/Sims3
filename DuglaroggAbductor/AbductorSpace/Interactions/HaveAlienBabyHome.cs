using NRaas.AbductorSpace.Helpers;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.CelebritySystem;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Pools;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace.Interactions
{
    public class HaveAlienBabyHome : Interaction<Sim, Lot>, Common.IPreLoad, Common.IAddInteraction
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public List<Sim> mNewborns;
        public static ulong kIconNameHash = ResourceUtils.HashString64("hud_interactions_baby");

        public void AcquirePregnancyStateMachine()
        {
            mCurrentStateMachine = StateMachineClient.Acquire(Actor, "Pregnancy");
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<Sim>(Singleton);
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
                        new Proxies.PregnancyProxy(Actor.SimDescription.Pregnancy).PregnancyComplete(mNewborns, null);
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
                        Common.Exception(Actor, Target, e);
                    }
                }

                Sims3.Gameplay.Gameflow.Singleton.EnableSave(this);

                if (Actor.IsMale && mNewborns != null)
                {
                    if (!Actor.HasBeenDestroyed && pregnancy != null)
                    {
                        new Proxies.PregnancyProxy(pregnancy).PregnancyComplete(mNewborns, null);
                    }
                }

                if (mNewborns != null && mNewborns.Count == 4)
                {
                    if (Actor != null && Actor.BuffManager != null)
                    {
                        Actor.BuffManager.RemoveElement(BuffNames.ItsABoy);
                        Actor.BuffManager.AddElement(CommonPregnancy.sItsQuadruplets, Origin.FromNewBaby);
                    }

                    
                }
            }
            catch (ResetException)
            {
                throw;
            }
            catch (Exception e)
            {
                Common.Exception(Actor, Target, e);
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
            mNewborns = new Proxies.PregnancyProxy(pregnancy).CreateNewborns(0f, isSelectable, true);
        }

        public void OnPreLoad()
        {
            Tunings.Inject<Sim, Pregnancy.HaveBabyHome.Definition, Definition>(true);
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

                    if (!Actor.RouteToPointRadius(point, 3f) && (!GlobalFunctions.PlaceAtGoodLocation(Actor, new World.FindGoodLocationParams(point), false) || !SimEx.IsPointInLotSafelyRoutable(Actor, Target, Actor.Position)))
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
                        Terrain.TeleportMeHere here = Terrain.TeleportMeHere.Singleton.CreateInstance(Terrain.Singleton, Actor, new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false) as Terrain.TeleportMeHere;
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

                    InternalHaveAlienBabyHome instance = InternalHaveAlienBabyHome.Singleton.CreateInstance(baby, Actor, GetPriority(), Autonomous, CancellableByPlayer) as InternalHaveAlienBabyHome;
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
                Common.Exception(Actor, Target, e);
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
                return Common.Localize("HaveAlienBabyHome:MenuName");
            }

            public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return actor.Household == target.Household && actor.BuffManager.HasElement(AbductionBuffs.sAlienBabyIsComing);
            }
        }
    }
}
