using NRaas.AliensSpace.Interactions;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.Gameplay.Situations;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Helpers
{
    public class AlienAbductionSituationEx : RootSituation
    {
        public class AbductSim : ChildSituation<AlienAbductionSituationEx>
        {
            public AbductSim() { }

            public AbductSim(AlienAbductionSituationEx parent) : base(parent) { }

            public override void Init(AlienAbductionSituationEx parent)
            {
                CarUFO ufo = parent.Alien.Inventory.Find<CarUFO>();

                if (ufo == null)
                {
                    Common.DebugNotify("AlienAbductionSituation.AbductSim.Init" + Common.NewLine + " - UFO is null");
                    parent.CleanupAbduction();
                    return;
                }

                AbductSimEx abductSim = ForceSituationSpecificInteraction(ufo, parent.Alien, AbductSimEx.Singleton, null,
                    new Callback(OnCompletion), new Callback(OnFailure), new InteractionPriority(InteractionPriorityLevel.CriticalNPCBehavior))
                    as AbductSimEx;

                if (abductSim == null)
                {
                    Common.DebugNotify("Alien Abduction Situation - Abduct Sim interaction is null");
                    parent.CleanupAbduction();
                    return;
                }

                abductSim.SimToAbduct = parent.Abductee;
            }

            public void OnCompletion(Sim actor, float x)
            {
                Parent.CleanupAbduction();
            }

            public void OnFailure(Sim actor, float x)
            {
                Parent.CleanupAbduction();
            }
        }

        public Sim Alien;
        public Sim Abductee;

        public AlienAbductionSituationEx() { }

        public AlienAbductionSituationEx(SimDescription alien, Sim abductee, Lot lot) : base(lot)
        {
            if (alien == null || abductee == null || lot == null)
            {
                if (alien == null)
                    Common.DebugNotify("Alien Abduction Situation Ex: Alien is null");

                if (abductee == null)
                    Common.DebugNotify("Alien Abduction Situtaion Ex: Abductee is null");

                if (lot == null)
                    Common.DebugNotify("Alien Abduction Situation Ex: Lot is null");
                
                Exit();
                return;
            }

            Alien = alien.InstantiateOffScreen(LotManager.GetFarthestLot(lot));
            Alien.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday, 0);
            Abductee = abductee;

            if (Alien != null && Abductee != null)
            {
                Alien.AssignRole(this);
                Abductee.AssignRole(this);
                SetState(new AbductSim(this));
                return;
            }

            Alien.Destroy();
            Exit();
        }

        public void CleanupAbduction()
        {
            Abductee.RemoveRole(this);
            Alien.RemoveRole(this);
            Sim.MakeSimGoHome(Alien, false);
            Exit();
        }

        public static AlienAbductionSituationEx Create(SimDescription alien, Sim abductee, Lot lot)
        {
            return new AlienAbductionSituationEx(alien, abductee, lot);
        }

        public override void OnParticipantDeleted(Sim participant)
        {
            if (participant != Alien && participant != Abductee)
                return;

            if (Alien != null)
                Alien.RemoveRole(this);

            if (Abductee != null)
                Abductee.RemoveRole(this);

            Exit();
        }
    }
}
