using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using manualisator.DBSchema;
using Word = Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace manualisator.Core
{
    public class RebuildLookupDocument : LongRunningTask
    {
        private DatabaseServices Database;
        private int CurrentStep = 1;
        private Word._Application Word;
        public readonly Dictionary<long, Manual> ManualByID = new Dictionary<long, Manual>();
        private readonly Dictionary<long, Template> TemplateByID = new Dictionary<long, Template>();
        private readonly Dictionary<long, List<long>> TemplatesForManual = new Dictionary<long,List<long>>();
        private readonly HashSet<long> TemplatesNeeded = new HashSet<long>();
        private Word._Document LookupDocument;

        public RebuildLookupDocument()
        {
        }

        public override bool Initialize(IDisplayCallback displayCallback)
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
                    TemplatesNeeded.Add(mc.TemplateID);
                }
            }

            double percentage = TemplatesNeeded.Count / ((TemplateByID.Count / 100.0));

            DisplayCallback.AddInformation(
                Strings.NumberOfTemplatesNeeded,
                TemplatesNeeded.Count,
                TemplateByID.Count,
                percentage,
                DateTime.Now - now);

            return true;
        }

        public override void Run()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.StepCreateLookupDocument, CurrentStep++);

            DateTime now = DateTime.Now;

            string targetFilename = Settings.LookupDocumentPathname;
            if (File.Exists(targetFilename))
            {
                try
                {
                    File.Delete(targetFilename);
                }
                catch(Exception e)
                {
                    DumpException(e, Strings.ErrorUnableToCreateLookupDocument);
                    return;
                }
            }
            if (Word == null)
            {
                DisplayCallback.AddInformation(Strings.NeedToCreateNewInstanceOfWord);
                Word = new Word.Application();
                Word.Visible = false;
            }
            bool failed = false;
            try
            {
                LookupDocument = new Word.Document();
                try
                {
                    long count = 1;
                    foreach (long templateKey in TemplatesNeeded)
                    {
                        Template t = TemplateByID[templateKey];
                        DisplayCallback.AddInformation(Strings.CreatingLookupDocument,
                            t.Name,
                            count++,
                            TemplatesNeeded.Count,
                            DateTime.Now - now);


                        if(IsCancelFlagSet())
                        {
                            failed = true;
                            break;
                        }

                        if (!AddTemplateContentsToLookupDocument(LookupDocument, t))
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (!failed)
                    {
                        LookupDocument.SaveAs2(targetFilename);
                    }
                }
                finally
                {
                    if( failed )
                    {
                        object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                        LookupDocument.Close(so);
                        Marshal.ReleaseComObject(LookupDocument);
                        LookupDocument = null;
                    }
                }
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ErrorUnableToCreateLookupDocument);
                failed = true;
            }

            if (failed)
            {
                DisplayCallback.AddInformation(Strings.FailedAfterTime, DateTime.Now - now);
            }
            else
            {
                DisplayCallback.AddInformation(Strings.SucceededAfterTime, DateTime.Now - now);
            }
        }

        private bool AddTemplateContentsToLookupDocument(Word._Document lookupDocument, Template t)
        {
            if( Tools.IsSpecialTemplate(t.Name) )
            {
                DisplayCallback.AddInformation(Strings.SpecialHandlingForTemplateAddDirectly, t.Name);
                return true;
            }

            if(IsCancelFlagSet())
            {
                return false;
            }

            try
            {
                string pathname = Settings.GetDocumentFilename(t.Name);
                Word._Document currentTemplateDocument = Word.Documents.Open(pathname);
                try
                {
                    currentTemplateDocument.Activate();
                    Word.Range rng;
                    DisplayCallback.AddInformation(Strings.FilesHasSoManyBookmarks, t.Name, currentTemplateDocument.Bookmarks.Count);
                    DisplayCallback.AddInformation("");
                    for (int i = 1, imax = currentTemplateDocument.Bookmarks.Count; i <= imax; ++i)
                    {
                        Word.Bookmark bm = currentTemplateDocument.Bookmarks[i];
                        if (!bm.Name.StartsWith("TEMPLATE_"))
                        {
                            string newName = string.Format("TEMPLATE_{0}_{1}", t.ID, bm.Name);
                            object oldRange = bm.Range;
                            currentTemplateDocument.Bookmarks.Add(newName, ref oldRange);
                            DisplayCallback.AddInformation("^- {0} [{1}..{2}]", newName, bm.Range.Start, bm.Range.End);
                        }
                    }

                    // copy the whole cover letter - including all bookmarks in it
                    object start = currentTemplateDocument.Content.Start;
                    object end = currentTemplateDocument.Content.End;
                    rng = currentTemplateDocument.Range(ref start, ref end);
                    rng.Copy();

                    lookupDocument.Activate();
                    start = lookupDocument.Content.End - 1;
                    end = lookupDocument.Content.End;
                    rng = lookupDocument.Range(ref start, ref end);
                    rng.Select();
                    rng.Paste();    

                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    currentTemplateDocument.Close(so);
                    Marshal.ReleaseComObject(currentTemplateDocument);
                }
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ErrorUnableToAddTemplateContentsToLookupDocument);
                return false;
            }
            return true;
        }

        public override void Dispose()
        {
            if(LookupDocument != null)
            {
                object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                LookupDocument.Close(so);
                Marshal.ReleaseComObject(LookupDocument);
            }
 	        if( Word != null )
            {
                Word.Quit();
                Word = null;
            }
            if( Database != null )
            {
                Database.Database.Cleanup();
            }
        }
    }
}
