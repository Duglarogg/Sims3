using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg
{
    public class AbductorTuning
    {
        // General
        [Tunable, TunableComment("Whether to allow logging and debug interactions")]
        public static bool kDebugging = true;


        // Alien Activity
        [Tunable, TunableComment("Base chance of alien activity on a given day.")]
        public static int kBaseActivityChance = 20;

        [Tunable, TunableComment("Base chance that alien activity is abduction.")]
        public static int kBaseAbductionChance = 25;

        [Tunable, TunableComment("Base chance that alien activity is visit.")]
        public static int kBaseVisitChance = 50;

        [Tunable, TunableComment("Base chance that active household is abduction target.")]
        public static int kActiveAbductionChance = 50;

        [Tunable, TunableComment("Base chance that active household is visit target.")]
        public static int kActiveVisitChance = 20;

        [Tunable, TunableComment("Earliest hour of day that alien activity can occur.")]
        public static int kHourToStartActivity = 22;

        [Tunable, TunableComment("Span of time (in hours) during which alien activity can occur.")]
        public static int kHoursOfActivity = 6;

        [Tunable, TunableComment("Bonus chance of alien activity per space rock found.")]
        public static int kSpaceRockBonus = 10;

        [Tunable, TunableComment("Max bonus chance of alien activity for finding space rocks.")]
        public static int kMaxSpaceRockBonus = 50;

        [Tunable, TunableComment("Bonus chance of alien activity for using a telescope.")]
        public static int kTelescopeUsedBonus = 25;

        [Tunable, TunableComment("Min LTR liking to get the high LTR bonus to visit chance.")]
        public static int kHighLTRThreshold = 40;

        [Tunable, TunableComment("Bonus chance that active household is targeted for a visit due to high LTR with alien.")]
        public static int kHighLTRBonus = 8;

        [Tunable, TunableComment("Bonus chance that active household is targeted for a visit due to being offspring of alien.")]
        public static int kOffspringBonus = 10;

        [Tunable, TunableComment("Min space rocks on lot to get bonus chance to visit.")]
        public static int kSpaceRockThreshold = 5;

        [Tunable, TunableComment("Bonus chance that active household is targeted for a visit due to space rocks on lot.")]
        public static int kSpaceRockVisitBonus = 15;


        // Aliens
        [Tunable, TunableComment("Advanced Tech skill level range for NPC aliens.")]
        public static int[] kAdvancedTechSkill = new int[] { 8, 10 };

        [Tunable, TunableComment("Handiness skill level range for NPC aliens.")]
        public static int[] kHandinessSkill = new int[] { 5, 7 };

        [Tunable, TunableComment("Logic skill level range for NPC aliens.")]
        public static int[] kLogicSkill = new int[] { 8, 10 };

        [Tunable, TunableComment("Whether to apply Future Sim trait to aliens.")]
        public static bool kFutureSim = true;


        // Alien Pregnancy
        [Tunable, TunableComment("Whether teens can be impregnated during abductions.")]
        public static bool kAllowTeens = true;

        [Tunable, TunableComment("Base chance of impregnation during abductions.")]
        public static int kImpregnationChance = 33;

        [Tunable, TunableComment("Duration (in days) of pregnancy phase.")]
        public static int kPregnancyLength = 3;

        [Tunable, TunableComment("Duration (in hours) of labor phase.")]
        public static int kLaborLength = 4;

        [Tunable, TunableComment("Total duration (in hours) of alien pregnancy.")]
        public static int kHoursOfPregnancy = 76;           // (kPregnancyLength * 24) + kLaborLength

        [Tunable, TunableComment("Span of time (in hours) to apply the pregnancy morph.")]
        public static int kHoursToShowPregnantMorph = 60;   // (5 / 6) * (kPregnancyLength * 24)

        [Tunable, TunableComment("Hour of alien pregnancy to start labor phase.")]
        public static int kHourToStartLabor = 72;           // (kPregnancyLength * 24)

        [Tunable, TunableComment("Hour of alien pregnancy to start pregnant walk.")]
        public static int kHourToStartPregnantWalk = 32;    // (4 / 9) * (kPregnancyLength * 24)

        [Tunable, TunableComment("Whether to apply fertility factors to chance of impregnation during abduction.")]
        public static bool kUseFertiltiy = false;

        // Volunter For Examination
        [Tunable, TunableComment("Duration (in hours) of examinations at the science lab")]
        public static int kExamDuration = 2;

        [Tunable, TunableComment("Base compensation rate for volunteers")]
        public static int kBaseCompensation = 500;

        [Tunable, TunableComment("Max compensation multiplier")]
        public static int kMaxMultiplier = 6;
    }
}
