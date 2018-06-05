using NRaas.CommonSpace.Booters;
using Sims3.Gameplay.ActorSystems;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace.Booters
{
    public class TraitBooter : BooterHelper.ListingBooter
    {
        public TraitBooter()
            : this(VersionStamp.sNameSpace + ".Traits", true)
        { }

        public TraitBooter(string reference, bool testDirect)
            : base("TraitFile", "File", reference, testDirect)
        { }

        public override BooterHelper.BootFile GetBootFile(string reference, string name, bool primary)
        {
            return new BooterHelper.DataBootFile(reference, name, primary);
        }

        protected override void PerformFile(BooterHelper.BootFile file)
        {
            BooterHelper.DataBootFile dataFile = file as BooterHelper.DataBootFile;

            if (dataFile == null)
            {
                BooterLogger.AddError("TraitBooter.PerformFile: Data file is null");
                return;
            }

            if (dataFile.GetTable("TraitList") == null && dataFile.GetTable("Traits") == null)
            {
                BooterLogger.AddError("TraitBooter.PerformFile: TraitList and Traits tables are null");
                return;
            }

            if (dataFile.GetTable("TraitList") == null)
                BooterLogger.AddError("TraitBooter.PerformFile: TraitList table is null");

            if (dataFile.GetTable("Traits") == null)
                BooterLogger.AddError("TraitBooter.PerformFile: Traits table is null");

            try
            {
                TraitManager.ParseTraitData(dataFile.Data, true);
                BooterLogger.AddTrace(file + ": ParseTraitData Success");
            }
            catch (Exception e)
            {
                Common.Exception(file.ToString(), e);
                BooterLogger.AddError(file + ": ParseTraitData Error");
            }
        }
    }
}
