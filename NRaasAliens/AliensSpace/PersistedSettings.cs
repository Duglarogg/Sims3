using NRaas.AliensSpace.Buffs;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace
{
    [Persistable]
    public class PersistedSettings
    {
        // General Settings
        private bool mDebugging = AliensTuning.kDebugging;
        private bool mLinkToStoryProgression = AliensTuning.kLinkToStoryProgression;
        private bool mStoryProgressionForUserDirected = AliensTuning.kStoryProgressionForUserDirected;

        // Alien Settings
        public bool mFutureSim = AliensTuning.kFutureSim;
        public Pair<int, int> mLogicSkill = new Pair<int, int>(AliensTuning.kLogicSkill[0], AliensTuning.kLogicSkill[1]);
        public Pair<int, int> mHandinessSkill = new Pair<int, int>(AliensTuning.kHandinessSkill[0], AliensTuning.kHandinessSkill[1]);
        public Pair<int, int> mFutureSkill = new Pair<int, int>(AliensTuning.kFutureSkill[0], AliensTuning.kFutureSkill[1]);

        // Alien Activity Settings
        public int mEarliestHour = AliensTuning.kEarliestHour;
        public int mActivityWindow = AliensTuning.kActivityWindow;
        public int mActivityCooldown = AliensTuning.kActivityCooldown;

        public int mBaseActivityChance = AliensTuning.kBaseActivityChance;
        public int mBaseAbductionChance = AliensTuning.kBaseAbductionChance;
        public int mAbductionLength = AliensTuning.kAbductionLength;
        public int mBaseVisitChance = AliensTuning.kBaseVisitChance;
        public int mHighLTRThreshold = AliensTuning.kHightLTRThreshold;
        public int mSpaceRockThreshold = AliensTuning.kSpaceRockThreshold;

        public int mTelescopeBonus = AliensTuning.kTelescopeBonus;
        public int mSpaceRockFoundBonus = AliensTuning.kSpaceRockFoundBonus;
        public int mMaxSpaceRockBonus = AliensTuning.kMaxSpaceRockBonus;
        public int mHighLTRBonus = AliensTuning.kHighLTRBonus;
        public int mOffspringBonus = AliensTuning.kOffspringBonus;
        public int mSpaceRockBonus = AliensTuning.kSpaceRockBonus;

        // Alien Pregnancy Settings
        public int mPregnancyChance = AliensTuning.kPregnancyChance;
        public bool mAllowTeens = AliensTuning.kAllowTeens;
        public bool mUseFertility = AliensTuning.kUseFertility;
        public int mPregnancyLength = AliensTuning.kPregnancyLength;
        public int mLaborLength = AliensTuning.kLaborLength;
        public int mPregnancyShow = 10; // Hours
        public int mBackacheChance = AliensTuning.kBackacheChance;
        public int mNumPuddles = AliensTuning.kNumPuddles;

        // Derived Pregnancy Settings
        public int mPregnancyDuration;  // In Hours
        public int mPregnancyMorph;     // In Hours
        public int mStartWalk;          // In Hours
        public int mStartLabor;         // In Hours
        public int mForeignShowTNS;     // In Hours
        public int mForeignLeaves;      // In Hours

        public PersistedSettings()
        {
            UpdatePregnancyTuning();
        }

        public bool Debugging
        {
            get { return mDebugging; }

            set
            {
                mDebugging = value;
                Common.kDebugging = value;
            }
        }

        public bool LinkToStoryProgression(bool autonomous)
        {
            if (!mLinkToStoryProgression)
                return false;

            if (!autonomous)
                if (!mStoryProgressionForUserDirected)
                    return false;

            return true;
        }

        public void UpdatePregnancyTuning()
        {
            int pregnancyLength = mPregnancyLength * 24;

            mStartLabor = mPregnancyShow + pregnancyLength;                                     // Default: 82 hours
            mPregnancyDuration = mPregnancyShow + pregnancyLength + mLaborLength;               // Default: 86 hours
            mForeignShowTNS = mPregnancyShow + (int)Math.Round((1f / 6f) * pregnancyLength);    // Default: 22 hours
            mForeignLeaves = mPregnancyShow + (int)Math.Round((1f / 4f) * pregnancyLength);     // Default: 28 hours
            mStartWalk = mPregnancyShow + (int)Math.Round((4f / 9f) * pregnancyLength);         // Default: 42 hours
            mPregnancyMorph = (int)Math.Round((5f / 6f) * pregnancyLength);                     // Default: 60 hours
        }
    }
}
