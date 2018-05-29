using NRaas.AliensSpace.Helpers;
using NRaas.AliensSpace.Interactions;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

/* <NOTES>
 * 
 *  AbductSimEx.Init
 *      Need to create AbductSimAEx interaction as a forced situation interaction and assign the Abductee to it as SimToAbduct, once
 *      AbductSimAEx has been implemented.
 */

namespace NRaas.AliensSpace.Helpers
{
    public class AlienAbductionSituationEx : RootSituation
    {
        public Sim Alien, Abductee;

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

                // <NOTE> create AbductSimAEX interaction here and assign the abductee to the interaction
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
