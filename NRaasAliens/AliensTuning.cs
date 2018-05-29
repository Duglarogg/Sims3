using Sims3.Gameplay.Roles;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas
{
    public class AliensTuning
    {
        // General Settings
        [Tunable, TunableComment("Whether debug messages and interactions are enabled")]
        public static bool kDebugging = true;

        [Tunable, TunableComment("Whether or not to link alien activity to Story Progression options")]
        public static bool kLinkToStoryProgression = false;

        [Tunable, TunableComment("Whether or not to link user-directed actions to Story Progression options")]
        public static bool kStoryProgressionForUserDirected = false;

        // Alien Settings
        [Tunable, TunableComment("Whether or not aliens gain the Future Sim trait and Advanced Technology skill")]
        public static bool kFutureSim = false;

        [Tunable, TunableComment("Min and max Logic skill levels for aliens")]
        public static int[] kLogicSkill = new int[] { 8, 10 };

        [Tunable, TunableComment("Min and max Handiness skill levels for aliens")]
        public static int[] kHandinessSkill = new int[] { 7, 8 };

        [Tunable, TunableComment("Min and max Advanced Technology skill levels for aliens")]
        public static int[] kFutureSkill = new int[] { 10, 10 };

        // Alien Activity Settings
        [Tunable, TunableComment("Range(0 - 23): Earliest hour that alien activity can occur")]
        public static int kEarliestHour = 20;

        [Tunable, TunableComment("Range(1 - 24): Number of hours during which alien activity can occur")]
        public static int kActivityWindow = 8;

        [Tunable, TunableComment("Range(0 - 24): Number of hours that must pass before more alien activity can occur")]
        public static int kActivityCooldown = 4;

        [Tunable, TunableComment("Range(0 - 100): Base chance of alien activity happening")]
        public static int kBaseActivityChance = 10;

        [Tunable, TunableComment("Range(0 - 100): Base chance of an abduction happening")]
        public static int kBaseAbductionChance = 25;

        // BaseVisitationChance = 1 - BaseAbductionChance (Default = 75)

        // ActiveAbductionChance /* Base chance that active household is target of abduction */

        [Tunable, TunableComment("Range(0 - 100): Minimum LTR needed to earn the high LTR bonus")]
        public static int kHightLTRThreshold = 60;

        [Tunable, TunableComment("Range(0 - 100): Base chance of aliens visiting the active household")]
        public static int kBaseVisitChance = 25;

        [Tunable, TunableComment("Range(0 - 100): Min number of space rocks on lot to get additional visit bonus")]
        public static int kSpaceRockThreshold = 5;

        [Tunable, TunableComment("Range(0 - 100): Additional activity chance for using a telescope")]
        public static int kTelescopeBonus = 25;

        [Tunable, TunableComment("Range(0 - 100): Additional activity chance for each space rock found")]
        public static int kSpaceRockFoundBonus = 5;

        [Tunable, TunableComment("Range(0 - 100): Max additional activity chance for finding space rocks")]
        public static int kMaxSpaceRockBonus = 25;

        [Tunable, TunableComment("Range(0 - 100): Additional visit chance for high LTR")]
        public static int kHighLTRBonus = 25;

        [Tunable, TunableComment("Range(0 - 100): Additional visit chance for offspring")]
        public static int kOffspringBonus = 25;

        [Tunable, TunableComment("Range(0 - 100): Additional visit chance for space rocks on lot")]
        public static int kSpaceRockBonus = 33;

        // Alien Pregnancy Settings

    }
}
