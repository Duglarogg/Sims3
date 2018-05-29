using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg
{
    public class AbductorTuning
    {
        // General Tuning
        [Tunable, TunableComment("Whether to allow debugging")]
        public static bool kDebugging = false;

        [Tunable, TunableComment("Whether to link alien activity to NRaasStoryProgression")]
        public static bool kLinkToStoryProgression = false;

        [Tunable, TunableComment("Whether to link user-directed abductions to NRaasStoryProgression")]
        public static bool kStoryProgressionForUserDirected = false;

        // Alien Activity Tuning
        [Tunable, TunableComment("Range: 0 - 100. Base chance there is alien activity.")]
        public static int kBaseActivityChance = 20;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that an abduction is attempted.")]
        public static int kBaseAbductionChance = 25;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that a visitation is attempted.")]
        public static int kBaseVisitChance = 50;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that the active household is targeted for an abduction.")]
        public static int kActiveAbductionChance = 50;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that the active household is targed for a visit.")]
        public static int kActiveVisitChance = 20;

        [Tunable, TunableComment("Range: 0 - 23. Earliest hour of day which alien activity can occur.")]
        public static int kHourToStartActivity = 22;

        [Tunable, TunableComment("Range: 0 - 24. Span of time (in hours) during which alien activity can occur.")]
        public static int kHoursOfActivity = 6;

        [Tunable, TunableComment("Range: 0 - 100. Bonus chance of alien activity per space rock found.")]
        public static int kSpaceRockFoundBonus = 10;

        [Tunable, TunableComment("Range: 0 - 100. Maximum bonus chance of alien activity for finding space rocks.")]
        public static int kMaxSpaceRockFoundBonus = 50;

        [Tunable, TunableComment("Range: 0 - 100. Bonus chance of alien activity for using a telescope.")]
        public static int kTelescopeUsedBonus = 25;

        // Alien Tuning
        [Tunable, TunableComment("Range: 0 - 10. Range of Advanced Technology skill levels for NPC aliens.")]
        public static int[] kAdvancedTechnologySkill = new int[] { 7, 10 };

        [Tunable, TunableComment("Range: 0 - 10. Range of Handiness skill levels for NPC aliens.")]
        public static int[] kHandinessSkill = new int[] { 5, 7 };

        [Tunable, TunableComment("Range: 0 - 10. Range of Logic skill levels for NPC aliens.")]
        public static int[] kLogicSkill = new int[] { 7, 10 };

        [Tunable, TunableComment("Whether to apply the Future Sim trait to NPC aliens.")]
        public static bool kFutureSim = true;

        // Alien Pregnancy Tuning
        [Tunable, TunableComment("Whether teens can be impregnated during an abduction.")]
        public static bool kAllowTeens = false;

        [Tunable, TunableComment("Range: 0 - 100. Base chance of an abductee to be impregnated during an abduction.")]
        public static int kImpregnationChance = 33;

        [Tunable, TunableComment("Duration (in hours) of an alien pregnancy.")]
        public static int kHoursOfPregnancy = 76;  // PregnancyLength * 24 + LaborLength

        [Tunable, TunableComment("Duration (in hours) over which Sim goes from not showing to fully pregnant")]
        public static int kHoursToShowPregnantMorph = 60; // (5/6) * PregnancyLength * 24

        [Tunable, TunableComment("Hour of alien pregnancy to start labor.")]
        public static int kHourToStartLabor = 72; // PregnancyLength * 24

        [Tunable, TunableComment("Hour of alien pregnancy to start pregnant walk.")]
        public static int kHourToStartPregnantWalk = 32;  // (4/9) * PregnancyLength * 24

        [Tunable, TunableComment("Minimum: 1. Duration (in days) of an alien pregnancy.")]
        public static int kPregnancyLength = 3;

        [Tunable, TunableComment("Minimum: 1. Duration (in hours) of an alien pregnnacy's labor.")]
        public static int kLaborLength = 4;

        [Tunable, TunableComment("Whether to use fertility factors when determining chance of impregnation.")]
        public static bool kUseFertility = false;
    }
}
