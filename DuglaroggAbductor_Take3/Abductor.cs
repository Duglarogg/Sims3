using Duglarogg.AbductorSpace;
using Duglarogg.AbductorSpace.Booters;
using Duglarogg.AbductorSpace.Helpers;
using Duglarogg.AbductorSpace.Interactions;
using Duglarogg.AbductorSpace.Proxies;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg
{
    public class Abductor
    {
        [Tunable, TunableComment("Scripting mod instantiator; value does not matter, only its existence.")]
        protected static bool kInstantiator = false;

        [PersistableStatic]
        protected static PersistedSettings sSettings = null;

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
            LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(OnPreLoad);

            /*
            World.OnWorldLoadFinishedEventHandler += new EventHandler(Logger.OnWorldLoadFinished);
            World.OnWorldLoadFinishedEventHandler += new EventHandler(OnWorldLoadFinished);
            World.OnWorldLoadFinishedEventHandler += new EventHandler(AlienUtilsEx.OnWorldLoadFinished);
            
            World.OnWorldQuitEventHandler += new EventHandler(AlienUtilsEx.OnWorldQuit);
            */
        }

        /*
        public static void AddInteraction(Lot lot)
        {
            bool hasDebugTriggerAbduction = false;
            bool hasDebugTriggerVisit = false;
            bool hasHaveBabyHome = false;

            foreach (InteractionObjectPair pair in lot.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == DebugTriggerAlienAbduction.Singleton.GetType())
                {
                    hasDebugTriggerAbduction = true;
                }

                if (pair.InteractionDefinition.GetType() == DebugTriggerAlienVisit.Singleton.GetType())
                {
                    hasDebugTriggerVisit = true;
                }

                if (pair.InteractionDefinition.GetType() == HaveAlienBabyHome.Singleton.GetType())
                {
                    hasHaveBabyHome = true;
                }

                if (hasDebugTriggerAbduction && hasDebugTriggerVisit && hasHaveBabyHome)
                {
                    return;
                }
            }

            if(!hasDebugTriggerAbduction)
            {
                lot.AddInteraction(DebugTriggerAlienAbduction.Singleton);
            }

            if (!hasDebugTriggerVisit)
            {
                lot.AddInteraction(DebugTriggerAlienVisit.Singleton);
            }

            if (!hasHaveBabyHome)
            {
                lot.AddInteraction(HaveAlienBabyHome.Singleton);
            }
        }

        public static void AddInteraction(RabbitHole rabbitHole)
        {
            if (rabbitHole.Guid == RabbitHoleType.Hospital)
            {
                foreach (InteractionObjectPair pair in rabbitHole.Interactions)
                {
                    if (pair.InteractionDefinition.GetType() == HaveAlienBabyHospital.Singleton.GetType())
                    {
                        return;
                    }
                }

                rabbitHole.AddInteraction(HaveAlienBabyHospital.Singleton);
            }


            if (rabbitHole.Guid == RabbitHoleType.ScienceLab)
            {
                foreach (InteractionObjectPair pair in rabbitHole.Interactions)
                {
                    if (pair.InteractionDefinition.GetType() == VolunteerForExamination.Singleton.GetType())
                    {
                        return;
                    }
                }

                rabbitHole.AddInteraction(VolunteerForExamination.Singleton);
            }
        }

        public static void AddInteraction(Sim sim)
        {
            bool hasDebugInducePregnancy = false;
            bool hasReturnBaby = false;

            foreach(InteractionObjectPair pair in sim.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == DebugInduceAlienPregnancy.Singleton.GetType())
                {
                    hasDebugInducePregnancy = true;
                }

                if (pair.InteractionDefinition.GetType() == ReturnAlienBabyEx.Singleton.GetType())
                {
                    hasReturnBaby = true;
                }

                if (hasDebugInducePregnancy && hasReturnBaby)
                {
                    return;
                }
            }

            if (!hasDebugInducePregnancy)
            {
                sim.AddInteraction(DebugInduceAlienPregnancy.Singleton);
            }

            if (!hasReturnBaby)
            {
                sim.AddInteraction(ReturnAlienBabyEx.Singleton);
            }
        }

        public static ListenerAction OnBabyBorn(Event evt)
        {
            if (evt.TargetObject is Sim)
            {
                AddInteraction(evt.TargetObject as Sim);
                //DebugInduceAlienPregnancy.AddInteraction(evt.TargetObject as Sim);
                //ReturnAlienBabyEx.AddInteraction(evt.TargetObject as Sim);
            }

            return ListenerAction.Keep;
        }

        public static ListenerAction OnObjectChanged(Event evt)
        {
            if (evt.TargetObject is Lot)
            {
                AddInteraction(evt.TargetObject as Lot);
                //DebugTriggerAlienAbduction.AddInteraction(evt.TargetObject as Lot);
                //DebugTriggerAlienVisit.AddInteraction(evt.TargetObject as Lot);
                //HaveAlienBabyHome.AddInteraction(evt.TargetObject as Lot);

                return ListenerAction.Keep;
            }

            if (evt.TargetObject is RabbitHole)
            {
                AddInteraction(evt.TargetObject as RabbitHole);
                ReplaceInteraction(evt.TargetObject as RabbitHole);
                //GetMedicalAdviceEx.ReplaceInteraction(evt.TargetObject as RabbitHole);
                //HaveAlienBabyHospital.AddInteraction(evt.TargetObject as RabbitHole);
                //VolunteerForExamination.AddInteraction(evt.TargetObject as RabbitHole);

                return ListenerAction.Keep;
            }

            if (evt.TargetObject is CarUFO)
            {
                ReplaceInteraction(evt.TargetObject as CarUFO);
                //AbductSimAEx.ReplaceInteraction(evt.TargetObject as CarUFO);
            }

            return ListenerAction.Keep;
        }
        */

        public static void OnPreLoad()
        {
            new BuffBooter().LoadBuffData();
            new TraitBooter().LoadTraitData();

            /*
            AbductSimAEx.OnPreLoad();
            GetMedicalAdviceEx.OnPreLoad();
            ReturnAlienBabyEx.OnPreLoad();
            */
        }

        /*
        public static void OnWorldLoadFinished(object sender, EventArgs evtArgs)
        {
            foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
            {
                AddInteraction(sim);
                //DebugInduceAlienPregnancy.AddInteraction(sim);
                //ReturnAlienBabyEx.AddInteraction(sim);
            }

            foreach (Lot lot in Sims3.Gameplay.Queries.GetObjects<Lot>())
            {
                AddInteraction(lot);
                //DebugTriggerAlienAbduction.AddInteraction(lot);
                //DebugTriggerAlienVisit.AddInteraction(lot);
                //HaveAlienBabyHome.AddInteraction(lot);
            }

            foreach (RabbitHole rabbitHole in Sims3.Gameplay.Queries.GetObjects<RabbitHole>())
            {
                AddInteraction(rabbitHole);
                ReplaceInteraction(rabbitHole);
                //HaveAlienBabyHospital.AddInteraction(rabbitHole);
                //VolunteerForExamination.AddInteraction(rabbitHole);
            }

            foreach (CarUFO ufo in Sims3.Gameplay.Queries.GetObjects<CarUFO>())
            {
                ReplaceInteraction(ufo);
                //AbductSimAEx.ReplaceInteraction(ufo);
            }

            EventTracker.AddListener(EventTypeId.kChildBornOrAdopted, new ProcessEventDelegate(OnBabyBorn));
            EventTracker.AddListener(EventTypeId.kBoughtObject, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kBoughtObjectInEditTownMode, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kInventoryObjectAdded, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kObjectStateChanged, new ProcessEventDelegate(OnObjectChanged));
        }

        public static void ReplaceInteraction(CarUFO ufo)
        {
            ufo.RemoveInteractionByType(AbductSimAEx.sOldSingleton.GetType());

            foreach (InteractionObjectPair pair in ufo.Interactions)
            {
                if (pair.InteractionDefinition.GetType() == AbductSimAEx.Singleton.GetType())
                {
                    return;
                }
            }

            ufo.AddInteraction(AbductSimAEx.Singleton);
        }

        public static void ReplaceInteraction(RabbitHole rabbitHole)
        {
            if (rabbitHole.Guid == RabbitHoleType.Hospital)
            {
                rabbitHole.RemoveInteractionByType(GetMedicalAdviceEx.sOldSingleton.GetType());

                foreach (InteractionObjectPair pair in rabbitHole.Interactions)
                {
                    if (pair.InteractionDefinition.GetType() == GetMedicalAdviceEx.Singleton.GetType())
                    {
                        return;
                    }
                }

                rabbitHole.AddInteraction(GetMedicalAdviceEx.Singleton);
            }
        }

        public static void ResetSettings()
        {
            sSettings = null;
        }
        */
    }
}
