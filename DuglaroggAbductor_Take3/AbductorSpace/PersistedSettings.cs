using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duglarogg.AbductorSpace
{
    [Persistable]
    public class PersistedSettings
    {
        public bool mDebugging = AbductorTuning.kDebugging;


        public int mBaseActivityChance = AbductorTuning.kBaseActivityChance;
        public int mBaseAbductionChance = AbductorTuning.kBaseAbductionChance;
        public int mBaseVisitChance = AbductorTuning.kBaseVisitChance;

        public int mActiveAbductionChance = AbductorTuning.kActiveAbductionChance;
        public int mActiveVisitChance = AbductorTuning.kActiveVisitChance;
        public int mSpaceRockBonus = AbductorTuning.kSpaceRockBonus;
        public int mMaxSpaceRockBonus = AbductorTuning.kMaxSpaceRockBonus;
        public int mTelescopeUsedBonus = AbductorTuning.kTelescopeUsedBonus;
        public int mHighLTRThreshold = AbductorTuning.kHighLTRThreshold;
        public int mHighLTRBonus = AbductorTuning.kHighLTRBonus;
        public int mOffspringBonus = AbductorTuning.kOffspringBonus;
        public int mSpaceRockThreshold = AbductorTuning.kSpaceRockThreshold;
        public int mSpaceRockVisitBonus = AbductorTuning.kSpaceRockVisitBonus;

        public int mHourToStartActivity = AbductorTuning.kHourToStartActivity;
        public int mHoursOfActivity = AbductorTuning.kHoursOfActivity;


        public int[] mAdvancedTechSkill = AbductorTuning.kAdvancedTechSkill;
        public int[] mHandinessSkill = AbductorTuning.kHandinessSkill;
        public int[] mLogicSkill = AbductorTuning.kLogicSkill;
        public bool mFutureSim = AbductorTuning.kFutureSim;


        public bool mAllowTeens = AbductorTuning.kAllowTeens;
        public int mImpregnantionChance = AbductorTuning.kImpregnationChance;
        public int mPregnancyLength = AbductorTuning.kPregnancyLength;
        public int mLaborLength = AbductorTuning.kLaborLength;
        public int mHoursOfPregnancy = AbductorTuning.kHoursOfPregnancy;
        public int mHoursToShowPregnantMorph = AbductorTuning.kHoursToShowPregnantMorph;
        public int mHourToStartLabor = AbductorTuning.kHourToStartLabor;
        public int mHourToStartPregnantWalk = AbductorTuning.kHourToStartPregnantWalk;
        public bool mUseFertility = AbductorTuning.kUseFertiltiy;


        public int mExamDuration = AbductorTuning.kExamDuration;
        public int mBaseCompensation = AbductorTuning.kBaseCompensation;
        public int mMaxMultiplier = AbductorTuning.kMaxMultiplier;
    }
}
