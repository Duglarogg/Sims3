using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.ScoringMethods;
using NRaas.WoohooerSpace.Helpers;
using NRaas.WoohooerSpace.Options.Woohoo;
using NRaas.WoohooerSpace.Scoring;
using NRaas.WoohooerSpace.Skills;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
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
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Objects.Decorations;
using Sims3.Gameplay.Objects.Entertainment;
using Sims3.Gameplay.Objects.Environment;
using Sims3.Gameplay.Objects.Elevator;
using Sims3.Gameplay.Objects.HobbiesSkills.Inventing;
using Sims3.Gameplay.Objects.Misc;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.Pets;
using Sims3.Gameplay.Objects.Plumbing;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Objects.ShelvesStorage;
using Sims3.Gameplay.Objects.TombObjects;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.UI;
using Sims3.Gameplay.TuningValues;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NRaas.WoohooerSpace.Interactions
{
    public class CommonSurrogate : Common.IWorldLoadFinished
    {
        // CommonWoohoo.WoohooStyle
            // Safe => Donate
            // Risky/TryForBaby => Impregnate

        public static bool CanDonate(Sim simA, Sim simB, ref GreyedOutTooltipCallback callback)
        {
            if (simA.SimDescription.Gender != simB.SimDescription.Gender)
            {
                callback = Common.DebugTooltip("Male-Female Donating Not Allowed (Yet)");
                return false;
            }

            if (simA.IsRobot && simB.IsRobot)
            {
                callback = Common.DebugTooltip("Robot-Robot Donating Not Allowed");
                return false;
            }

            if (!simA.IsRobot && !simB.IsRobot)
            {
                callback = Common.DebugTooltip("Meatbag-Meatbag Donating Not Allowed");
                return false;
            }

            return true;
        }

        public static void DonateDNA(Sim actor, Sim target)
        {
            if (!actor.IsRobot && !target.IsRobot)
            {
                Common.DebugNotify("CommonSurrogate.DonateDNA" + Common.NewLine + " - Fail: Participants are Robots");
                return;
            }

            if (actor.IsRobot && target.IsRobot)
            {
                Common.DebugNotify("CommonSurrogate.DonateDNA" + Common.NewLine + " - Fail: Participants are Humans");
                return;
            }

            if (actor.SimDescription.Gender != target.SimDescription.Gender)
            {
                Common.DebugNotify("CommonSurrogate.DonateDNA" + Common.NewLine + " - Fail: Participants are not Same Sex");
                return;
            }

            ScientificSample.DnaSampleSubject subject;

            if (!actor.IsRobot)
            {
                subject = new ScientificSample.DnaSampleSubject(actor.SimDescription);
                ScientificSample.CreateAndAddToInventory(target, subject);
            }
            else
            {
                subject = new ScientificSample.DnaSampleSubject(target.SimDescription);
                ScientificSample.CreateAndAddToInventory(actor, subject);
            }
        }

        public void OnWorldLoadFinished()
        {
            new Common.ImmediateEventListener(EventTypeId.kWooHooed, OnWoohooed);
        }

        public static void OnWoohooed(Event e)
        {
            using (Common.TestSpan span = new Common.TestSpan(ScoringLookup.Stats, "Duration CommonWoohoo:OnWoohooed"))
            {
                WooHooEvent wEvent = e as WooHooEvent;

                if (wEvent == null)
                    return;

                Sim actor = wEvent.Actor as Sim;

                if (actor == null)
                    return;

                SimDescription targetDesc = null;
                Sim target = wEvent.TargetObject as Sim;

                if (target != null)
                    targetDesc = target.SimDescription;
                else if (actor.SimDescription.IsPregnant)
                    targetDesc = SimDescription.Find(actor.SimDescription.Pregnancy.DadDescriptionId);

                if (targetDesc == null)
                    return;

                CommonWoohoo.WoohooLocation location = CommonWoohoo.WoohooLocation.Bed;
                CommonWoohoo.WoohooStyle style = CommonWoohoo.WoohooStyle.Safe;
                IWooHooDefinition woohoo = null;
                CommonWoohoo.NRaasWooHooEvent customEvent = wEvent as CommonWoohoo.NRaasWooHooEvent;

                if (customEvent != null)
                {
                    location = customEvent.Location;
                    style = customEvent.Style;
                }
                else
                {
                    if (actor.CurrentInteraction != null)
                    {
                        woohoo = actor.CurrentInteraction.InteractionDefinition as IWooHooDefinition;

                        if (woohoo == null)
                        {
                            if (actor.CurrentInteraction is Shower.TakeShower)
                            {
                                foreach (Sim sim in actor.LotCurrent.GetAllActors())
                                {
                                    if (sim.CurrentInteraction != null && sim.CurrentInteraction.Target == actor)
                                    {
                                        woohoo = sim.CurrentInteraction.InteractionDefinition as IWooHooDefinition;

                                        if (woohoo != null)
                                            break;
                                    }
                                }
                            }
                        }

                        if (woohoo != null)
                        {
                            location = woohoo.GetLocation(wEvent.ObjectUsed);
                            style = woohoo.GetStyle(actor.CurrentInteraction);
                        }

                        if (wEvent.BedUsed != null)
                        {
                            if (wEvent.BedUsed is Tent)
                                location = CommonWoohoo.WoohooLocation.Tent;
                            else if (wEvent.BedUsed is Igloo)
                                location = CommonWoohoo.WoohooLocation.Igloo;
                            else if (wEvent.BedUsed is FairyHouse)
                                location = CommonWoohoo.WoohooLocation.FairyHouse;
                        }
                        else if (woohoo == null && wEvent.ObjectUsed != null)
                        {
                            foreach (WoohooLocationControl check in Common.DerivativeSearch.Find<WoohooLocationControl>())
                            {
                                if (check.Matches(wEvent.ObjectUsed))
                                {
                                    location = check.Location;
                                    break;
                                }
                            }
                        }
                    }
                }

                switch(style)
                {
                    case CommonWoohoo.WoohooStyle.Safe:
                        KamaSimtra.AddNotch(actor.SimDescription, targetDesc, actor.LotCurrent, location, style);
                        break;

                    case CommonWoohoo.WoohooStyle.Risky:
                    case CommonWoohoo.WoohooStyle.TryForBaby:
                        KamaSimtra.AddNotch(actor.SimDescription, targetDesc, actor.LotCurrent, location, CommonWoohoo.WoohooStyle.TryForBaby);
                        break;
                }

                Woohooer.Settings.AddCount(actor);
                WoohooBuffs.Apply(actor, target, style == CommonWoohoo.WoohooStyle.Risky);
                KamaSimtraSettings.ServiceData data = KamaSimtra.Settings.GetServiceData(targetDesc.SimDescriptionId, true);

                if (data != null)
                    data.Dispose();
            }
        }

        public static string GetSocialName(CommonWoohoo.WoohooStyle style)
        {
            string action = "NRaas";

            switch(style)
            {
                case CommonWoohoo.WoohooStyle.Safe:
                    return action + "Donate";

                case CommonWoohoo.WoohooStyle.Risky:
                case CommonWoohoo.WoohooStyle.TryForBaby:
                    return action + "Impregnate";
            }

            return null;
        }

        internal static bool SatisfiesDonate(Sim actor, Sim target, string v1, bool isAutonomous, bool v2, bool v3, ref GreyedOutTooltipCallback callback)
        {
            if (actor.SimDescription.Gender != target.SimDescription.Gender)
                return false;

            if (!actor.IsRobot && !target.IsRobot)
                return false;

            if (actor.IsRobot && target.IsRobot)
                return false;

            if (!CanDonate(actor, target, ref callback))
                return false;

            // more requirements to come

            return true;
        }
    }
}
