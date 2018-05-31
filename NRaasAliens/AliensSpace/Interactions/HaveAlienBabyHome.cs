using NRaas.AliensSpace.Helpers;
using NRaas.AliensSpace.Proxies;
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

namespace NRaas.AliensSpace.Interactions
{
    public class HaveAlienBabyHome : Interaction<Sim, Lot>, Common.IPreLoad, Common.IAddInteraction
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public List<Sim> mNewborns;
        public static ulong kIconNameHash = ResourceUtils.HashString64("hud_interactions_baby");

        public class Definition : InteractionDefinition<Sim, Lot, HaveAlienBabyHome>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return Common.Localize("HaveAlienBabyHome:MenuName");
            }

            public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return actor.Household == target.Household && actor.BuffManager.HasElement(BuffsAndTraits.sAlienBabyIsComing);
            }
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<Sim>(Singleton);
        }

        public override void Cleanup()
        {
            AlienPregnancy pregnancy = Actor.SimDescription.Pregnancy as AlienPregnancy;
            bool wasPregnant = (pregnancy != null);

            try
            {
                if (!Actor.SimDescription.IsVampire)
                    Actor.Motives.CreateMotive(CommodityKind.Hunger);

                try
                {
                    if (mNewborns == null)
                        mNewborns = new List<Sim>();

                    if (!Actor.HasBeenDestroyed)
                        pregnancy.PregnancyComplete(mNewborns, null);

                    base.Cleanup();
                }
                catch (ResetException)
                {
                    throw;
                }
                catch(Exception e)
                {
                    if (pregnancy != null)
                        Common.Exception(Actor, Target, e);
                }
            }
            catch(ResetException)
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

        public void GetNewborns()
        {
            AlienPregnancy pregnancy = Actor.SimDescription.Pregnancy as AlienPregnancy;
            Sims3.Gameplay.Gameflow.Singleton.DisableSave(this, "Gameplay/ActorSystems/Pregnancy:DisableSave");
            mNewborns = pregnancy.CreateNewborns(0f, Actor.IsSelectable, true);
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
                        return false;

                    if (!Actor.RouteToPointRadius(point, 3f))
                    {
                        if (!GlobalFunctions.PlaceAtGoodLocation(Actor, new World.FindGoodLocationParams(point), false)
                            || !SimEx.IsPointInLotSafelyRoutable(Actor, Target, Actor.Position))
                            Actor.AttemptToPutInSafeLocation(true);
                    }    
                }

                if (Actor.Posture is SwimmingInPool)
                {
                    SwimmingInPool posture = Actor.Posture as SwimmingInPool;

                    if (!posture.ContainerPool.RouteToEdge(Actor))
                    {
                        if (Actor.BridgeOrigin != null)
                            Actor.BridgeOrigin.MakeRequest();

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

                AlienPregnancy pregnancy = Actor.SimDescription.Pregnancy as AlienPregnancy;
                GetNewborns();
                mCurrentStateMachine = StateMachineClient.Acquire(Actor, "Pregnancy");
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

                    HaveAlienBabyHomeInternal instance = HaveAlienBabyHomeInternal.Singleton.CreateInstance(baby, Actor, GetPriority(), 
                        Autonomous, CancellableByPlayer) as HaveAlienBabyHomeInternal;
                    instance.TotalCount = mNewborns.Count;
                    instance.BabyIndex = i + 1;
                    instance.mCurrentStateMachine = mCurrentStateMachine;
                    Actor.InteractionQueue.PushAsContinuation(instance, true);
                }

                if (Actor.IsSelectable)
                    OccultImaginaryFriend.DeliverDollToHousehold(mNewborns);

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
    }
}
