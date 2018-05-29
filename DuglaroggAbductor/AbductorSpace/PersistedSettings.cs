using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AbductorSpace
{
    [Persistable]
    public class PersistedSettings
    {
        // General
        private bool mDebugging = AbductorTuning.kDebugging;
        private bool mLinkToStoryProgression = AbductorTuning.kLinkToStoryProgression;
        private bool mStoryProgressionForUseDirected = AbductorTuning.kStoryProgressionForUserDirected;

        // Alien Activity
        public int mBaseActivityChance = AbductorTuning.kBaseActivityChance;
        public int mBaseAbductionChance = AbductorTuning.kBaseAbductionChance;
        public int mBaseVisitationChance = AbductorTuning.kBaseVisitationChance;
        public int mActiveAbductionChance = AbductorTuning.kActiveAbductionChance;
        public int mActiveVisitationChance = AbductorTuning.kActiveVisitationChance;
        public int mEarliestVisitHour = AbductorTuning.kEarliestVisitHour;
        public int mVisitWindow = AbductorTuning.kVisitWindow;
        public int mSpaceRockFoundBonus = AbductorTuning.kSpaceRockFoundBonus;
        public int mMaxSpaceRockFoundBonus = AbductorTuning.kMaxSpaceRockFoundBonus;
        public int mTelescopeUsedBonus = AbductorTuning.kTelescopeUsedBonus;

        // Alien Pregnancy
        public bool mAllowTeens = AbductorTuning.kAllowTeens;
        public int mBackacheChance = AbductorTuning.kBackacheChance;
        public int mImpregnationChance = AbductorTuning.kImpregnationChance;
        public int mPregnancyLength = AbductorTuning.kPregnancyLength;
        public bool mUseFertility = AbductorTuning.kUseFertitly;

        public int mHoursOfPregnancy = AbductorTuning.kHoursOfPregnancy;
        public int mHourToStartPregnantWalk = AbductorTuning.kHourToStartPregnantWalk;
        public int mHoursToShowPregnantMorph = AbductorTuning.kHoursToShowPregnantMorph;
        public int mHourToStartContractions = AbductorTuning.kHourToStartContractions;

        // Aliens
        public int[] mAdvancedTechSkill = AbductorTuning.kAdvancedTechSkill;
        public bool mFutureSim = AbductorTuning.kFutureSim;
        public int[] mHandinessSkill = AbductorTuning.kHandinessSkill;
        public int[] mLogicSkill = AbductorTuning.kLogicSkill;

        public bool LinkToStoryProgression(bool autonomous)
        {
            if (!mLinkToStoryProgression) return false;

            if (!autonomous)
            {
                if (!mStoryProgressionForUseDirected) return false;
            }

            return true;
        }

        public void UpdateAlienPregnancyTuning()
        {
            int pregnancyLength = Abductor.Settings.mPregnancyLength * 24;
            int laborLength = AbductorTuning.kLaborLength;

            mHoursOfPregnancy = pregnancyLength + laborLength;
            mHourToStartPregnantWalk = (int)Math.Round((4f / 9f) * pregnancyLength);
            mHoursToShowPregnantMorph = (int)Math.Round((5f / 6f) * pregnancyLength);
            mHourToStartContractions = pregnancyLength;
        }

        public bool Debugging
        {
            get
            {
                return mDebugging;
            }

            set
            {
                mDebugging = value;
                Common.kDebugging = value;
            }
        }
    }
}
