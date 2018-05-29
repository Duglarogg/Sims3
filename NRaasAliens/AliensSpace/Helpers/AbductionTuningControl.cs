using Sims3.Gameplay.Autonomy;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Helpers
{
    public class AbductionTuningControl : IDisposable
    {
        InteractionTuning mTuning;
        bool mDisallowAutonomous;
        bool mDisallowUserDirected;

        public AbductionTuningControl(InteractionTuning tuning, bool allowTeen)
        {
            mTuning = tuning;

            try
            {
                if (mTuning != null)
                {
                    mTuning.Availability.Teens = allowTeen;
                    mTuning.Availability.Adults = true;
                    mTuning.Availability.Elders = true;
                    mDisallowAutonomous = mTuning.HasFlags(InteractionTuning.FlagField.DisallowAutonomous);
                    mDisallowUserDirected = mTuning.HasFlags(InteractionTuning.FlagField.DisallowUserDirected);
                }
            }
            catch (Exception e)
            {
                Common.Exception("AbductionTuningControl", e);
            }
        }

        public void Dispose()
        {
            ResetTuning(mTuning, mDisallowAutonomous, mDisallowUserDirected);
        }

        public static InteractionTuning ResetTuning(InteractionTuning tuning, bool disallowAutonomout, bool disallowUserDirected)
        {
            try
            {
                if (tuning != null)
                {
                    tuning.Availability.Teens = false;
                    tuning.Availability.Adults = true;
                    tuning.Availability.Elders = true;
                    tuning.SetFlags(InteractionTuning.FlagField.DisallowAutonomous, disallowAutonomout);
                    tuning.SetFlags(InteractionTuning.FlagField.DisallowUserDirected, disallowUserDirected);
                }
            }
            catch (Exception e)
            {
                Common.Exception("ResetTuning", e);
            }

            return tuning;
        }
    }
}
