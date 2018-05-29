﻿using NRaas.AliensSpace;
using NRaas.AliensSpace.Booters;
using NRaas.AliensSpace.Helpers;
using NRaas.CommonSpace.Booters;
using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.Options;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Socializing;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas
{
    public class Aliens : Common, Common.IPreLoad, Common.IWorldLoadFinished, Common.IWorldQuit
    {
        [Tunable, TunableComment("Scripting Mod Instantiator")]
        protected static bool kInstantiator = false;

        [PersistableStatic]
        protected static PersistedSettings sSettings = null;

        public static PersistedSettings Settings
        {
            get
            {
                if (sSettings == null)
                    sSettings = new PersistedSettings();

                return sSettings;
            }
        }

        static Aliens()
        {
            sEnableLoadLog = true;
            StatValueCount.sFullLog = true;
            Bootstrap();
            BooterHelper.Add(new BuffBooter());
            BooterHelper.Add(new TraitBooter());
        }

        public Aliens()
        { }

        public static InteractionTuning InjectAndReset<Target, OldType, NewType>(bool clone)
            where Target : IGameObject
            where OldType : InteractionDefinition
            where NewType : InteractionDefinition
        {
            return AbductionTuningControl.ResetTuning(Tunings.Inject<Target, OldType, NewType>(clone), false, false);
        }

        public void OnPreLoad()
        { }

        public void OnWorldLoadFinished()
        {
            kDebugging = Settings.Debugging;
        }

        public void OnWorldQuit()
        { }

        public static void ResetSettings()
        {
            sSettings = null;
        }

        public static GreyedOutTooltipCallback StoryProgressionTooltip(string debuggingReason, bool debuggingOnly)
        {
            if (kDebugging)
                return DebugTooltip("Story Progression: " + debuggingReason);
            else
            {
                if (debuggingOnly)
                    return null;

                return delegate { return Localize("Socials:ProgressionDenied"); };
            }
        }
    }
}
