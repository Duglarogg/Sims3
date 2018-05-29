using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace
{
    [Persistable]
    public class PersistedSettings
    {
        // General Settings
        public bool mDebugging = AbductorTuning.kDebugging;
        public bool mLinkToStoryProgression = AbductorTuning.kLinkToStoryProgression;
        public bool mStoryProgressionForUserDirected = AbductorTuning.kStoryProgressionForUserDirected;

        // Alien Activity Settings
        public int mBaseActivityChance = AbductorTuning.kBaseActivityChance;
        public int mBaseAbductionChance = AbductorTuning.kBaseAbductionChance;
        public int mBaseVisitChance = AbductorTuning.kBaseVisitChance;
        public int mActiveAbductionChance = AbductorTuning.kActiveAbductionChance;
        public int mActiveVisitChance = AbductorTuning.kActiveVisitChance;
        public int mHourToStartActivity = AbductorTuning.kHourToStartActivity;
        public int mHoursOfActivity = AbductorTuning.kHoursOfActivity;
        public int mSpaceRockFoundBonus = AbductorTuning.kSpaceRockFoundBonus;
        public int mMaxSpaceRockFoundBonus = AbductorTuning.kMaxSpaceRockFoundBonus;
        public int mTelescopeUsedBonus = AbductorTuning.kTelescopeUsedBonus;

        // Alien Pregnancy Settings
        public bool mAllowTeens = AbductorTuning.kAllowTeens;
        public int mImpregnationChance = AbductorTuning.kImpregnationChance;
        public int mHoursOfPregnancy = AbductorTuning.kHoursOfPregnancy;
        public int mHoursToShowPregnantMorph = AbductorTuning.kHoursToShowPregnantMorph;
        public int mHourToStartLabor = AbductorTuning.kHourToStartLabor;
        public int mHourToStartPregnantWalk = AbductorTuning.kHourToStartPregnantWalk;
        public int mPregnancyLength = AbductorTuning.kPregnancyLength;
        public int mLaborLength = AbductorTuning.kLaborLength;
        public bool mUseFertility = AbductorTuning.kUseFertility;

        // Alien Settings
        public int[] mAdvancedTechnologySkill = AbductorTuning.kAdvancedTechnologySkill;
        public int[] mHandinessSkill = AbductorTuning.kHandinessSkill;
        public int[] mLogicSkill = AbductorTuning.kLogicSkill;
        public bool mFutureSim = AbductorTuning.kFutureSim;

        public void UpdatePregnancySettings()
        {
            mHourToStartPregnantWalk = (int)Math.Round((4d / 9d) * mPregnancyLength * 24);
            mHoursToShowPregnantMorph = (int)Math.Round((5d / 6d) * mPregnancyLength * 24);
            mHourToStartLabor = mPregnancyLength * 24;
            mHoursOfPregnancy = mPregnancyLength * 24 + mLaborLength;
        }
    }
}
