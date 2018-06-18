using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Interactions
{
    public class DebugGetSkinToneKey : ImmediateInteraction<Sim, Sim>, Common.IAddInteraction
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Sim, DebugGetSkinToneKey>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return "DEBUG - Get Skin Tone Index";
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return Aliens.Settings.Debugging;
            }
        }

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Add<Sim>(Singleton);
        }

        public override bool Run()
        {
            if (Target == null)
                return false;

            string titleText = Target.FullName + Common.NewLine 
                + "Skin Tone Key: " + Target.SimDescription.SkinToneKey.ToString() + Common.NewLine 
                + "Skin Tone Type: " + Target.SimDescription.Skintone.ToString();

            StyledNotification.Show(new StyledNotification.Format(titleText, StyledNotification.NotificationStyle.kDebugAlert));

            return true;
        }
    }
}
