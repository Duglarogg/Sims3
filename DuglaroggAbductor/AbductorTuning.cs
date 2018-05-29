using Sims3.Gameplay.Roles;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas
{
    public class AbductorTuning
    {
        [Tunable, TunableComment("Whether to display debugging messages.")]
        public static bool kDebugging = true;

        [Tunable, TunableComment("Whether to link alien activity to Story Progression options.")]
        public static bool kLinkToStoryProgression = false;

        [Tunable, TunableComment("Whether to link user-directed abductions to Story Progression options.")]
        public static bool kStoryProgressionForUserDirected = false;


        [Tunable, TunableComment("Range: 0 - 100. Base chance that there is alien activity.")]
        public static int kBaseActivityChance = 10;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that an abduction is attempted.")]
        public static int kBaseAbductionChance = 25;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that a visitation is attempted.")]
        public static int kBaseVisitationChance = 25;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that the active household will be targeted for alien abduction.")]
        public static int kActiveAbductionChance = 75;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that the active household will be targeted for alien visitation.")]
        public static int kActiveVisitationChance = 25;

        [Tunable, TunableComment("Range: 0 -24. The earliest hour that alien activity can occur.")]
        public static int kEarliestVisitHour = 20;

        [Tunable, TunableComment("Range: 0 - 24. The window of time (in hours) during which alien activity can occur.")]
        public static int kVisitWindow = 9;

        [Tunable, TunableComment("Range: 0 - 100. Additional chance of activity per space rock found.")]
        public static int kSpaceRockFoundBonus = 10;

        [Tunable, TunableComment("Range: 0 - 100. Maximum additional chance of activity for space rocks found.")]
        public static int kMaxSpaceRockFoundBonus = 50;

        [Tunable, TunableComment("Range: 0 - 100. Additional chance of activity for using telescope.")]
        public static int kTelescopeUsedBonus = 25;


        [Tunable, TunableComment("The range of skill levels for the Advanced Technology skill of a new alien.")]
        public static int[] kAdvancedTechSkill = new int[] { 8, 10 };

        [Tunable, TunableComment("The range of skill levels for the Handiness skill of a new alien.")]
        public static int[] kHandinessSkill = new int[] { 7, 8 };

        [Tunable, TunableComment("The range of skill levels for the Logic skill of a new alien.")]
        public static int[] kLogicSkill = new int[] { 10, 10 };

        [Tunable, TunableComment("Whether to applied the hidden Future Sim trait to a new alien.")]
        public static bool kFutureSim = false;


        [Tunable, TunableComment("Whether teenagers can be impregnated during an abduction.")]
        public static bool kAllowTeens = false;

        [Tunable, TunableComment("Range: 0 - 100. Chance of Sim getting a backache during alien pregnancy.")]
        public static int kBackacheChance = 25;

        [Tunable, TunableComment("Range: 0 - 100. Base chance that an abductee will be impregnated during an abduction.")]
        public static int kImpregnationChance = 33;

        [Tunable, TunableComment("Duration (in hours) of an alien pregnancy.")]
        public static int kHoursOfPregnancy = 76;  // (3 * 24) + 4

        [Tunable, TunableComment("Duration (in hours) over which Sim goes from not showing to fully pregnant.")]
        public static int kHoursToShowPregnantMorph = 60;  // (5/6) * (3 * 24)

        [Tunable, TunableComment("Hour of alien pregnancy to apply pregnancy buff.")]
        public static int kHourToShowPregnancy = 8;

        [Tunable, TunableComment("Hour of alien pregnancy to start labor.")]
        public static int kHourToStartContractions = 72;  // (3 * 24)

        [Tunable, TunableComment("Hour of alien pregnancy to start pregnant walk.")]
        public static int kHourToStartPregnantWalk = 32;  // ((4/9) * (3 * 24)

        [Tunable, TunableComment("Minimum: 1. Duration (in days) of an alien pregnancy.")]
        public static int kPregnancyLength = 3;

        [Tunable, TunableComment("Minimum: 1. Duration (in hours) of an alien pregnancy's labor phase.")]
        public static int kLaborLength = 4;

        [Tunable, TunableComment("How many puddles to spawn when labor starts.")]
        public static int kPuddlesForWaterBreak = 1;

        [Tunable, TunableComment("Whether to use additional fertility factors when calculating chance of alien pregnancy and multiple babies.")]
        public static bool kUseFertitly = false;
    }
}
