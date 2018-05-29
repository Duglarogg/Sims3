using Duglarogg.AbductorSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace.Interactions
{
    public class VolunteerForExamination : RabbitHole.RabbitHoleInteraction<Sim, RabbitHole>
    {
        public static readonly InteractionDefinition Singleton = new Definition();

        public static void AddInteraction(RabbitHole lab)
        {
            if (lab.Guid != RabbitHoleType.ScienceLab)
            {
                return;
            }

            foreach (InteractionObjectPair pair in lab.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == Singleton.GetType())
                {
                    return;
                }
            }

            lab.AddInteraction(Singleton);
        }

        public override bool InRabbitHole()
        {
            BeginCommodityUpdates();
            bool flag = DoTimedLoop(Abductor.Settings.mExamDuration * 60f);
            EndCommodityUpdates(flag);

            if (flag)
            {
                int compensation = Abductor.Settings.mBaseCompensation * RandomUtil.GetInt(1, Abductor.Settings.mMaxMultiplier);
                string text = Localization.LocalizeString("Duglarogg/Abductor/Interactions/VolunteerForExamination:TNS", new object[] { compensation });
                StyledNotification.Format format = new StyledNotification.Format(text, Target.ObjectId, Actor.ObjectId, StyledNotification.NotificationStyle.kGameMessagePositive);
                StyledNotification.Show(format);
                Actor.ModifyFunds(compensation);
                Actor.TraitManager.AddHiddenElement(AlienUtilsEx.sAlreadyExamined);
            }

            return flag;
        }

        public class Definition : InteractionDefinition<Sim, RabbitHole, VolunteerForExamination>
        {
            public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Duglarogg/Abductor/Interactions/VolunteerForExamination:MenuName");
            }

            public override bool Test(Sim actor, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.Guid != RabbitHoleType.ScienceLab)
                {
                    greyedOutTooltipCallback = Abductor.Settings.mDebugging ? CreateTooltipCallback("Target is not Science Lab") : null;
                    return false;
                }

                if (!actor.BuffManager.HasElement(AlienUtilsEx.sXenogenesis))
                {
                    greyedOutTooltipCallback = Abductor.Settings.mDebugging ? CreateTooltipCallback("Actor is not Alien Pregnant") : null;
                    return false;
                }

                if (actor.BuffManager.HasElement(AlienUtilsEx.sBabyIsComing))
                {
                    greyedOutTooltipCallback = Abductor.Settings.mDebugging ? CreateTooltipCallback("Actor is in Labor") : null;
                    return false;
                }

                if (actor.TraitManager.HasElement(AlienUtilsEx.sAlreadyExamined))
                {
                    greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Duglarogg/Abductor/Interactions/VolunteerForExamination:AlreadyExamined"));
                    return false;
                }
                return true;
            }
        }
    }
}
