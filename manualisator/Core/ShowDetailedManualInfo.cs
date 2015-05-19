using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using manualisator.DBSchema;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace manualisator.Core
{
    public class ShowDetailedManualInfo : LongRunningTask
    {
        // TODO: should be reusing the RebuildLookupDocument container instead ....
        private DatabaseServices Database;
        private int CurrentStep = 1;
        public readonly Dictionary<long, Manual> ManualByID = new Dictionary<long, Manual>();
        private readonly Dictionary<long, Template> TemplateByID = new Dictionary<long, Template>();
        private readonly Dictionary<long, List<long>> TemplatesForManual = new Dictionary<long,List<long>>();
        private readonly Dictionary<long, long> TemplatesNeeded = new Dictionary<long, long>();

        public readonly List<Manual> ManualsToGenerate = new List<Manual>();

        public ShowDetailedManualInfo()
        {
        }

        public bool PreInitialize(IDisplayCallback displayCallback)
        {
            Trace.Assert(Database == null);
            DisplayCallback = displayCallback;

            string databaseFilename = Settings.ManualsDatabasePathname;
            if (File.Exists(databaseFilename))
            {
                DisplayCallback.AddInformation(Strings.OpeningExistingDatabase, databaseFilename);
            }
            else
            {
                DisplayCallback.AddInformation(Strings.CreatingNewDatabase, databaseFilename);
            }

            Database = new DatabaseServices(databaseFilename);

            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.DetermineWhichTemplatesAreNeeded, CurrentStep++);

            DateTime now = DateTime.Now;

            // select all manuals that need updating
            var manuals = Database.SelectAll<Manual>();

            // create lookup: manual ID -> manual
            foreach (Manual manual in manuals)
            {
                ManualByID[manual.ID] = manual;
            }

            // create lookup: template ID -> template 
            foreach (Template t in Database.SelectAll<Template>())
            {
                TemplateByID[t.ID] = t;
            }

            // create lookup: manual ID -> list of templates used by this ID 
            foreach (ManualContents mc in Database.SelectAll<ManualContents>())
            {
                if (ManualByID.ContainsKey(mc.ManualID))
                {
                    if (!TemplatesForManual.ContainsKey(mc.ManualID))
                        TemplatesForManual[mc.ManualID] = new List<long>();

                    TemplatesForManual[mc.ManualID].Add(mc.TemplateID);
                    if (TemplatesNeeded.ContainsKey(mc.TemplateID))
                    {
                        TemplatesNeeded[mc.TemplateID] += 1;
                    }
                    else
                    {
                        TemplatesNeeded[mc.TemplateID] = 1;
                    }
                }
            }

            double percentage = TemplatesNeeded.Count / ((TemplateByID.Count / 100.0));

            DisplayCallback.AddInformation(
                Strings.NumberOfTemplatesNeeded, 
                TemplatesNeeded.Count,
                TemplateByID.Count, 
                percentage,
                DateTime.Now - now);

            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation("{0} nicht benötigte Templates:", TemplateByID.Count - TemplatesNeeded.Count);
            foreach (long key in TemplateByID.Keys)
            {
                if (!TemplatesNeeded.ContainsKey(key))
                {
                    Template t = TemplateByID[key];
                    DisplayCallback.AddInformation("- {0} (id: {1})", t.Name, t.ID);
                }
            }
            DisplayCallback.AddInformation("");

            bool first = true;
            foreach (long key in TemplatesNeeded.Keys)
            {
                long value = TemplatesNeeded[key];
                if( value > 1 )
                {
                    if( first )
                    {
                        DisplayCallback.AddInformation("Templates, die mehr als einmal benötigt werden:", TemplateByID.Count - TemplatesNeeded.Count);
                        first = false;
                    }
                    Template t = TemplateByID[key];
                    DisplayCallback.AddInformation("- {0} (id: {1}, Anzahl: {2})", t.Name, t.ID, value);
                }
            }
            if( first )
            {
                DisplayCallback.AddInformation("Alle Templates werden maximal einmal eingebunden.");
            }
            DisplayCallback.AddInformation("");
            return true;
        }

        public override bool Initialize(IDisplayCallback displayCallback)
        {
            DisplayCallback = displayCallback;
            return true;
        }

        public override void Run()
        {
        }

        public override void Dispose()
        {
            if( Database != null )
            {
                Database.Database.Cleanup();
            }
        }
    }
}
