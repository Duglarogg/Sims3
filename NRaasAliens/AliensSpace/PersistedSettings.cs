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
        public int[] mLogicSkill = AliensTuning.kLogicSkill;
        public int[] mHandinessSkill = AliensTuning.kHandinessSkill;
        public int[] mFutureSkill = AliensTuning.kFutureSkill;

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
    }
}
