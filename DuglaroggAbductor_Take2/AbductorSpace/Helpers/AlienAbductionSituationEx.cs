using Duglarogg.AbductorSpace.Interactions;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Helpers
{
    class AlienAbductionSituationEx : RootSituation
    {
        public Sim Alien;
        public Sim Abductee;

        public AlienAbductionSituationEx()
        { }

        public AlienAbductionSituationEx(SimDescription alien, Sim abductee, Lot lot) : base(lot)
        {
            if (alien == null || abductee == null || lot == null)
            {
                Exit();
                return;
            }

            Alien = alien.InstantiateOffScreen(LotManager.GetFarthestLot(lot));
            Alien.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday, 0);
            Alien.AssignRole(this);

            Abductee = abductee;
            Abductee.AssignRole(this);
            SetState(new AbductSimEx(this));
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
            if (participant != Alien && participant != Abductee) return;

            if (Alien != null) Alien.RemoveRole(this);

            if (Abductee != null) Abductee.RemoveRole(this);

            Exit();
        }

        public class AbductSimEx : ChildSituation<AlienAbductionSituationEx>
        {
            public AbductSimEx()
            { }

            public AbductSimEx(AlienAbductionSituationEx parent) : base(parent)
            { }

            public override void Init(AlienAbductionSituationEx parent)
            {
                CarUFO ufo = parent.Alien.Inventory.Find<CarUFO>();

                if (ufo == null)
                {
                    parent.CleanupAbduction();
                    return;
                }

                AbductSimAEx abductSim = ForceSituationSpecificInteraction(ufo, parent.Alien, AbductSimAEx.Singleton, null, new Callback(OnCompletion),
                    new Callback(OnFailure), new InteractionPriority(InteractionPriorityLevel.CriticalNPCBehavior)) as AbductSimAEx;
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
    }
}
