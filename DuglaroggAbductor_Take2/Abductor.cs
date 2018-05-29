using Duglarogg.AbductorSpace;
using Duglarogg.AbductorSpace.Helpers;
using Duglarogg.AbductorSpace.Interactions;
using Duglarogg.CommonSpace.Booters;
using Duglarogg.CommonSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg
{
    public class Abductor
    {
        protected static bool kDebugging = false;
        public static bool sEnableLogs = false;

        [PersistableStatic]
        protected static PersistedSettings sSettings = null;

        [Tunable, TunableComment("Scripting Mod Instantiator")]
        protected static bool kInstantiator = false;

        public static PersistedSettings Settings
        {
            get
            {
                if (sSettings == null)
                {
                    sSettings = new PersistedSettings();
                }

                return sSettings;
            }
        }

        static Abductor()
        {
            sEnableLogs = true;

            //LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(OnPreLoad);
            //LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(AbductSimAEx.OnPreLoad);
            //LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(HaveAlienBabyHome.OnPreLoad);
            //LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(HaveAlienBabyHospital.OnPreLoad);
            //LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(ReactToContractionEx.OnPreLoad);
            //LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(TakeToHospitalEx.OnPreLoad);

            //World.OnWorldLoadFinishedEventHandler += new EventHandler(Logger.OnWorldLoadFinished);
            //World.OnWorldLoadFinishedEventHandler += new EventHandler(OnWorldLoadFinished);
            //World.OnWorldLoadFinishedEventHandler += new EventHandler(AlienUtilsEx.OnWorldLoadFinished);

            //World.OnWorldQuitEventHandler += new EventHandler(OnWorldQuit);
            //World.OnWorldQuitEventHandler += new EventHandler(AlienUtilsEx.OnWorldQuit);

            new BuffBooter().LoadBuffData();
            new TraitBooter().LoadTraitData();
        }

        public Abductor()
        { }

        public static ListenerAction OnNewBaby(Event evt)
        {
            if (evt.TargetObject is Sim)
            {
                DebugInduceAlienPregnancy.AddInteraction(evt.TargetObject as Sim);
                ReturnAlienBabyEx.AddInteraction(evt.TargetObject as Sim);
            }

            return ListenerAction.Keep;
        }

        public static ListenerAction OnObjectChanged(Event evt)
        {
            if (evt.TargetObject is CarUFO)
            {
                AbductSimAEx.AddInteraction(evt.TargetObject as CarUFO);

                return ListenerAction.Keep;
            }

            if (evt.TargetObject is RabbitHole)
            {
                if ((evt.TargetObject as RabbitHole).Guid == RabbitHoleType.Hospital)
                {
                    HaveAlienBabyHospital.AddInteraction(evt.TargetObject as RabbitHole);
                }

                return ListenerAction.Keep;
            }

            if (evt.TargetObject is Lot)
            {
                DebugTriggerAbduction.AddInteraction(evt.TargetObject as Lot);
                HaveAlienBabyHome.AddInteraction(evt.TargetObject as Lot);

                return ListenerAction.Keep;
            }

            return ListenerAction.Keep;
        }

        public static void OnPreLoad()
        {
            new BuffBooter().LoadBuffData();
            new TraitBooter().LoadTraitData();

            //AbductSimAEx.OnPreLoad();
            //HaveAlienBabyHome.OnPreLoad();
            //HaveAlienBabyHospital.OnPreLoad();
            //ReactToContractionEx.OnPreLoad();
            //TakeToHospitalEx.OnPreLoad();
        }

        public static void OnWorldLoadFinished(object sender, EventArgs evtArgs)
        {
            kDebugging = Settings.mDebugging;
            
            foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
            {
                DebugInduceAlienPregnancy.AddInteraction(sim);
                ReturnAlienBabyEx.AddInteraction(sim);
            }

            /*
            foreach (CarUFO ufo in Sims3.Gameplay.Queries.GetObjects<CarUFO>())
            {
                AbductSimAEx.AddInteraction(ufo);
            }
            */

            foreach (Lot lot in Sims3.Gameplay.Queries.GetObjects<Lot>())
            {
                DebugTriggerAbduction.AddInteraction(lot);
                HaveAlienBabyHome.AddInteraction(lot);
            }

            foreach (RabbitHole hospital in RabbitHole.GetRabbitHolesOfType(RabbitHoleType.Hospital))
            {
                HaveAlienBabyHospital.AddInteraction(hospital);
            }

            EventTracker.AddListener(EventTypeId.kChildBornOrAdopted, new ProcessEventDelegate(OnNewBaby));
            EventTracker.AddListener(EventTypeId.kBoughtObject, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kBoughtObjectInEditTownMode, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kInventoryObjectAdded, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kObjectStateChanged, new ProcessEventDelegate(OnObjectChanged));
        }

        public static void OnWorldQuit(object sender, EventArgs evtArgs)
        {
        }

        public static void ResetSettings()
        {
            sSettings = null;
        }
    }
}
