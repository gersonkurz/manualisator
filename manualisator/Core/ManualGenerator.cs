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
    public class ManualGenerator : LongRunningTask
    {
        // TODO: should be reusing the RebuildLookupDocument container instead ....
        private DatabaseServices Database;
        private int CurrentStep = 1;
        private Word._Application Word;
        private Excel._Application Excel;
        public readonly Dictionary<long, Manual> ManualByID = new Dictionary<long, Manual>();
        private readonly Dictionary<long, Template> TemplateByID = new Dictionary<long, Template>();
        private readonly Dictionary<long, List<long>> TemplatesForManual = new Dictionary<long,List<long>>();
        private readonly HashSet<long> TemplatesNeeded = new HashSet<long>();
        private Word._Document LookupDocument;

        public readonly List<Manual> ManualsToGenerate = new List<Manual>();

        private readonly Dictionary<long, string> DE_Lookup_Names = new Dictionary<long, string>();
        private readonly Dictionary<long, string> EN_Lookup_Names = new Dictionary<long, string>();

        public ManualGenerator()
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

        public override bool Initialize(IDisplayCallback displayCallback)
        {
            DisplayCallback = displayCallback;
            return true;
        }

        public override void Run()
        {
            if( OpenLookupDocument() )
            {
                if (ReadExistingLookupBookmarks())
                {
                    CreateAllManuals();
                }
            }
        }

        private void CreateAllManuals()
        {
            DateTime now = DateTime.Now;
            int nManualsCreated = 0;
            foreach(Manual m in ManualsToGenerate)
            {
                if (IsCancelFlagSet())
                    break;

                if( CreateManual(m) )
                {
                    ++nManualsCreated;
                }
            }

            DisplayCallback.AddInformation(Strings.TotalTimeToCreateAllManuals, nManualsCreated, DateTime.Now - now);
        }


        public static Dictionary<int, string> GermanMonthNames = new Dictionary<int, string>() {
            { 1, "Januar" },
            { 2, "Februar" },
            { 3, "März" },
            { 4, "April" },
            { 5, "Mai" },
            { 6, "Juni" },
            { 7, "Juli" },
            { 8, "August" },
            { 9, "September" },
            { 10, "October" },
            { 11, "November" },
            { 12, "Dezember" },
        };


        public static Dictionary<int, string> EnglishMonthNames = new Dictionary<int, string>() {
            { 1, "January" },
            { 2, "February" },
            { 3, "March" },
            { 4, "April" },
            { 5, "May" },
            { 6, "June" },
            { 7, "July" },
            { 8, "August" },
            { 9, "September" },
            { 10, "October" },
            { 11, "November" },
            { 12, "December" },
        };

        
        private bool CreateManual(Manual m)
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.StepCreatingManualForDeviceInLanguage, CurrentStep++,
                m.Name,
                m.Device,
                m.Language);

            DateTime now = DateTime.Now;

            string targetFilename = Settings.GetTargetFilename(m);

            Dictionary<long, string> lookup = m.Language.Equals(Strings.Language_DE, StringComparison.OrdinalIgnoreCase) ? DE_Lookup_Names : EN_Lookup_Names;
            Dictionary<long, string> otherLookup = m.Language.Equals(Strings.Language_DE, StringComparison.OrdinalIgnoreCase) ? EN_Lookup_Names : DE_Lookup_Names;

            Trace.Assert(Word != null);
            bool failed = false;
            try
            {

                string template = Settings.TemplateFilename_DE;
                if (m.Language.Equals(Strings.Language_EN))
                    template = Settings.TemplateFilename_EN;

                object templatePath = Path.Combine(Settings.BaseDirectory, Settings.TemplatesDirectory, template);
                Word._Document newDocument = Word.Documents.Add(ref templatePath);
                try
                {
                    foreach (long templateKey in TemplatesForManual[m.ID])
                    {
                        if (IsCancelFlagSet())
                        {
                            failed = true;
                            break;
                        }
                        Template t = TemplateByID[templateKey];
                        if (Tools.IsSpecialTemplate(t.Name))
                        {
                            AddDocumentToDocument(newDocument, t.Name, pageBreak: true, manual: m);
                        }
                        else if( lookup.ContainsKey(templateKey))
                        {
                            AddBookmarkToDocument(newDocument, lookup[templateKey]);
                        }
                        else if (otherLookup.ContainsKey(templateKey))
                        {
                            AddBookmarkToDocument(newDocument, otherLookup[templateKey]);
                        }
                        else
                        {
                            DisplayCallback.AddError(Strings.ErrorTemplateKeyNotFound, templateKey, t.Name);
                            failed = true;
                        }
                    }
                    if (!failed)
                    {
                        for( int s = 1, smax = newDocument.Sections.Count; s <= smax; ++s )
                        {
                            Word.Section section = newDocument.Sections[s];

                            Word.HeaderFooter hf = section.Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                            for( int w = 1, wmax = hf.Range.Words.Count; w <= wmax; ++w )
                            {
                                string mord = hf.Range.Words[w].Text;

                                if (mord.Equals("Monat"))
                                    hf.Range.Words[w].Text = GermanMonthNames[now.Month];
                                else if (mord.Equals("Month"))
                                    hf.Range.Words[w].Text = EnglishMonthNames[now.Month];
                                else if (mord.Equals("Year") || mord.Equals("Jahr"))
                                    hf.Range.Words[w].Text = now.Year.ToString();
                            }

                        }


                        newDocument.SaveAs2(targetFilename);
                        DisplayCallback.AddInformation("- '{0}'", targetFilename);
                    }
                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    newDocument.Close(so);
                    Marshal.ReleaseComObject(newDocument);
                }
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ErrorExceptionWhileCreatingDocument, m.Name);
                failed = true;
            }

            if (failed)
            {
                DisplayCallback.AddInformation(Strings.FailedAfterTime, DateTime.Now - now);
                return false;
            }
            DisplayCallback.AddInformation(Strings.SucceededAfterTime, DateTime.Now - now);
            return true;
        }

        private bool AddBookmarkToDocument(Word._Document newDocument, string bookmarkName)
        {
            try
            {
                Word.Bookmark bm = LookupDocument.Bookmarks[bookmarkName];
                // insert page headers
                Word.Range completeRange = bm.Range;
                for (int i = 1, imax = completeRange.Paragraphs.Count; i <= imax; ++i)
                {
                    string name = completeRange.Paragraphs[i].get_Style().NameLocal;
                    if (name.StartsWith("Überschrift 1;"))
                    {
                        object start2 = newDocument.Content.End - 1;
                        object end2 = newDocument.Content.End;
                        Word.Range rng2 = newDocument.Range(ref start2, ref end2);
                        rng2.InsertBreak(Microsoft.Office.Interop.Word.WdBreakType.wdPageBreak);
                        break;
                    }
                }
                bm.Range.Copy();
                object start = newDocument.Content.End - 1;
                object end = newDocument.Content.End;
                Word.Range rng = newDocument.Range(ref start, ref end);
                rng.Select();
                rng.Paste();
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ErrorUnableToLocateBookmark, bookmarkName);
                return false;
            }
            return true;
        }

        private void ReplaceBookmark(Word._Document currentTemplateDocument, string bmName, string newContent)
        {
            if( !string.IsNullOrEmpty(newContent))
            {
                for (int i = 1, imax = currentTemplateDocument.Bookmarks.Count; i <= imax; ++i)
                {
                    Word.Bookmark bm = currentTemplateDocument.Bookmarks[i];
                    if (bm.Name.Equals(bmName))
                    {                        
                        bm.Range.Text = newContent;
                        break;
                    }
                }
            }
        }

        private bool AddDocumentToDocument(Word._Document newDocument, string templateName, bool pageBreak = false, Manual manual = null)
        {
            Trace.TraceInformation("AddDocumentToDocument: {0}", templateName);
            try
            {
                string pathname = Settings.GetDocumentFilename(templateName);
                Word._Document currentTemplateDocument = Word.Documents.Open(pathname);
                try
                {
                    currentTemplateDocument.Activate();
                    Word.Range rng;
                    Trace.TraceInformation("- Document {0} has {1} bookmarks", pathname, currentTemplateDocument.Bookmarks.Count);

                    if (currentTemplateDocument.Bookmarks.Count > 0 )
                    {
                        ReplaceBookmark(currentTemplateDocument, "Titel_1", manual.Title1);
                        ReplaceBookmark(currentTemplateDocument, "Titel_2", manual.Title2);
                        ReplaceBookmark(currentTemplateDocument, "Version", manual.Version);
                    }

                    object start = currentTemplateDocument.Content.Start;
                    object end = currentTemplateDocument.Content.End;
                    rng = currentTemplateDocument.Range(ref start, ref end);
                    rng.Copy();

                    newDocument.Activate();
                    start = newDocument.Content.End - 1;
                    end = newDocument.Content.End;
                    rng = newDocument.Range(ref start, ref end);
                    rng.Select();
                    rng.Paste();

                    if( pageBreak )
                    {
                        newDocument.Words.Last.InsertBreak(Microsoft.Office.Interop.Word.WdBreakType.wdPageBreak);
                    }
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
                DumpException(e, Strings.ErrorDocumentFailedToConvert, templateName);
                return false;
            }
            return true;
        }

        private bool ReadExistingLookupBookmarks()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.StepParsingBookmarksInExistingDocument, CurrentStep++);

            DateTime now = DateTime.Now;

            try
            {
                LookupDocument.Activate();
                Microsoft.Office.Interop.Word.Bookmark bm;
                DisplayCallback.AddInformation("");
                for (int i = 1, imax = LookupDocument.Bookmarks.Count; i <= imax; ++i)
                {
                    bm = LookupDocument.Bookmarks[i];
                    string[] tokens = bm.Name.Split('_');
                    if (tokens[0].Equals("TEMPLATE"))
                    {
                        DisplayCallback.AddInformation("^- {0} [{1}..{2}]", bm.Name, bm.Start, bm.End);
                        long id = long.Parse(tokens[1]);
                        if (tokens[2].EndsWith("E"))
                        {
                            EN_Lookup_Names[id] = bm.Name;
                        }
                        else
                        {
                            DE_Lookup_Names[id] = bm.Name;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DumpException(e, "Exception caught");
                return false;
            }
            DisplayCallback.AddInformation(Strings.FoundBookmarksAfterSearching, 
                DE_Lookup_Names.Count,
                EN_Lookup_Names.Count,
                DateTime.Now - now);
            return true;
        }

        private bool OpenLookupDocument()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.StepOpenIntermediateDocument, CurrentStep++);

            DateTime now = DateTime.Now;

            string lookupDocumentFilename = Settings.LookupDocumentPathname;
            if( !File.Exists(lookupDocumentFilename))
            {
                return CreateLookupDocument();
            }

            if (Word == null)
            {
                DisplayCallback.AddInformation(Strings.NeedToCreateNewInstanceOfWord);
                Word = new Word.Application();
                Word.Visible = false;
            }
            try
            {
                LookupDocument = Word.Documents.Open(lookupDocumentFilename);
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ErrorUnableToReadLookupDocument);
                return false;
            }
            DisplayCallback.AddInformation(Strings.OpenedExistingLookupDocument, DateTime.Now - now);
            return true;
        }

        private bool CreateLookupDocument()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.StepCreateLookupDocument, CurrentStep++);

            DateTime now = DateTime.Now;

            string targetFilename = Settings.LookupDocumentPathname;
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

            if( failed )
            {
                DisplayCallback.AddInformation(Strings.FailedAfterTime, DateTime.Now - now);
                return false;
            }
            DisplayCallback.AddInformation(Strings.SucceededAfterTime, DateTime.Now - now);
            return true;
        }

        private bool AddTemplateContentsToLookupDocument(Word._Document lookupDocument, Template t)
        {
            if( Tools.IsSpecialTemplate(t.Name) )
            {
                DisplayCallback.AddInformation(Strings.SpecialHandlingForTemplateAddDirectly, t.Name);
                return true;
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
            if( Excel != null )
            {
                Excel.Quit();
                Excel = null;
            }
            if( Database != null )
            {
                Database.Database.Cleanup();
            }
        }
    }
}
