using NRaas.AliensSpace;
using NRaas.AliensSpace.Buffs;
using NRaas.AliensSpace.Helpers;
using NRaas.AliensSpace.Interactions;
using NRaas.AliensSpace.Proxies;
using NRaas.CommonSpace.Booters;
using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System.Collections.Generic;

namespace NRaas
{
    public class Aliens : Common, Common.IWorldLoadFinished
    {
        [Tunable, TunableComment("Scripting Mod Instantiator")]
        protected static bool kInstantiator = true;

        [PersistableStatic]
        protected static PersistedSettings sSettings = null;

        public static PersistedSettings Settings
        {
            get => (sSettings == null) ? sSettings = new PersistedSettings() : sSettings;
        }

        static Aliens()
        {
            sEnableLoadLog = true;
            StatValueCount.sFullLog = true;
            Bootstrap();
            BooterHelper.Add(new BuffBooter());
        }

        public Aliens() { }

        public static InteractionTuning InjectAndReset<Target, OldType, NewType>(bool clone)
            where Target : IGameObject
            where OldType : InteractionDefinition
            where NewType : InteractionDefinition
        {
            return AbductionTuningControl.ResetTuning(Tunings.Inject<Target, OldType, NewType>(clone), false, false);
        }

        // Externalized to WooHooer for pregnancy tests
        public static bool IsAlienPregnant(Sim sim)
        {
            if (!sim.SimDescription.IsPregnant)
                return false;

            if (sim.BuffManager != null && !sim.BuffManager.HasElement(BuffsAndTraits.sAbductedEx))
                return false;

            BuffAbductedEx.BuffInstanceAbductedEx instance = sim.BuffManager.GetElement(BuffsAndTraits.sAbductedEx) as BuffAbductedEx.BuffInstanceAbductedEx;

            return instance.IsAlienPregnant;
        }

        // Externalized to WooHooer for pregnancy tests
        public static bool OnPositivePregnancy(Sim sim)
        {
            if (!sim.SimDescription.IsPregnant)
                return false;

            AlienPregnancy pregnancy = new AlienPregnancy(sim.SimDescription.Pregnancy);
            sim.RemoveAlarm(pregnancy.PreggersAlarm);
            pregnancy.PreggersAlarm = sim.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(pregnancy.HourlyCallback),
                1f, TimeUnit.Hours, "Hourly Alien Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
            pregnancy.mHourOfPregnancy = Settings.mPregnancyShow;
            InteractionInstance instance = ShowAlienPregnancy.Singleton.CreateInstance(sim, sim, 
                new InteractionPriority(InteractionPriorityLevel.ESRB), false, false);
            instance.Hidden = true;
            sim.InteractionQueue.AddNext(instance);

            return true;
        }

        public void OnWorldLoadFinished()
        {
            kDebugging = Settings.Debugging;
        }

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
