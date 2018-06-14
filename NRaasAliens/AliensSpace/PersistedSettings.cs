using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI.Hud;
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
        public int[] mScienceSkill = AliensTuning.kScienceSkill;
        public int[] mFairyMagicSkill = AliensTuning.kFairyMagicSkill;
        public int[] mLycanthropySkill = AliensTuning.kLycanthropySkill;
        public int[] mSpellcraftSkill = AliensTuning.kSpellcraftSkill;
        public int[] mSpellcastingSkill = AliensTuning.kSpellcastingSkill;
        public int[] mGardeningSkill = AliensTuning.kGardeningSkill;
        public bool mAlienScience = AliensTuning.kAlienScience;
        public bool mAllowOccultAliens = AliensTuning.kAllowOccultAliens;
        public int mMaxAlienOccults = AliensTuning.kMaxAlienOccults;
        public int mOccultAlienChance = AliensTuning.kOccultAlienChance;
        public List<CASAgeGenderFlags> mValidAlienAges = new List<CASAgeGenderFlags>()
            {
                CASAgeGenderFlags.Teen,
                CASAgeGenderFlags.YoungAdult,
                CASAgeGenderFlags.Adult,
                CASAgeGenderFlags.Elder
            };
        public List<OccultTypes> mValidAlienOccults = OccultTypeHelper.CreateListOfMissingOccults(
            new List<OccultTypes>()
            {
                OccultTypes.Frankenstein,
                OccultTypes.Ghost,
                OccultTypes.Mummy,
                OccultTypes.Robot,
                OccultTypes.Unicorn
            }, false);

        // Alien Activity Settings
        public int mEarliestHour = AliensTuning.kEarliestHour;
        public int mActivityWindow = AliensTuning.kActivityWindow;
        public int mActivityCooldown = AliensTuning.kActivityCooldown;
        public int mBaseActivityChance = AliensTuning.kBaseActivityChance;
        public int mBaseAbductionChance = AliensTuning.kBaseAbductionChance;
        public int mAbductionLength = AliensTuning.kAbductionLength;
        public int mBaseVisitChance = AliensTuning.kBaseVisitChance;

        // Alien Activity Bonus Settings
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
        public int mBackacheChance = AliensTuning.kBackacheChance;
        public int mNumPuddles = AliensTuning.kNumPuddles;
        public bool mAllowOccultBabies = AliensTuning.kAllowOccultBabies;
        public int mMaxBabyOccults = AliensTuning.kMaxBabyOccults;
        // Only occults shared between abductee and alien yes/no

        
        // Derived Pregnancy Settings (in hours)
        public int mPregnancyShow = 10;
        public int mStartLabor = 82;
        public int mPregnancyDuration = 90;
        public int mPregnancyMorph = 60;
        public int mStartWalk = 42;
        public int mForeignShowTNS = 22;
        public int mForeignLeaves = 28;

        public PersistedSettings()
        {
            Debugging = mDebugging;
            UpdatePregnancyTuning();
        }

        public bool Debugging
        {
            get => mDebugging;

            set
            {
                Common.kDebugging = value;
                mDebugging = value;
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
