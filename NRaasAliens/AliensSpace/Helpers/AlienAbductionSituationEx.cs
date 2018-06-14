﻿using NRaas.AliensSpace.Interactions;
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
    public class AlienAbductionSituationEx : AlienAbductionSituation
    {
        public class AbductSimEx : ChildSituation<AlienAbductionSituationEx>
        {
            public AbductSimEx() { }

            public AbductSimEx(AlienAbductionSituationEx parent) : base(parent) { }

            public override void Init(AlienAbductionSituationEx parent)
            {
                CarUFO ufo = parent.Alien.Inventory.Find<CarUFO>();

                if (ufo == null)
                {
                    Common.DebugNotify("Alien Abduction Situation - UFO is null");
                    parent.CleanupAbduction();
                    return;
                }

                AbductSimAEx abduction = ForceSituationSpecificInteraction(ufo, parent.Alien, AbductSimAEx.Singleton, null,
                    new Callback(OnCompletion), new Callback(OnFailure), new InteractionPriority(InteractionPriorityLevel.CriticalNPCBehavior))
                    as AbductSimAEx;

                if (abduction == null)
                {
                    Common.DebugNotify("Alien Abduction Situation - Abduct Sim interaction is null");
                    parent.CleanupAbduction();
                    return;
                }

                abduction.SimToAbduct = parent.Abductee;
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
                SetState(new AbductSimEx(this));
                return;
            }

            Alien.Destroy();
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
