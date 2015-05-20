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
    public class CreateManualsDirectly : LongRunningTask
    {
        private readonly List<string> ExcelSheetsToGenerate;
        private int CurrentStep = 1;
        private Word._Application Word;
        private Excel._Application Excel;
        private readonly Dictionary<string, string> TemplateLookup = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> BookmarkToFilenames = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> FilenameToBookmarks = new Dictionary<string, List<string>>();

        public CreateManualsDirectly(List<string> excelSheetsToGenerate)
        {
            ExcelSheetsToGenerate = excelSheetsToGenerate;
        }

        public override bool Initialize(IDisplayCallback displayCallback)
        {
            DisplayCallback = displayCallback;
            return true;
        }

        public override void Run()
        {
            if (Word == null)
            {
                DisplayCallback.AddInformation(Strings.NeedToCreateNewInstanceOfWord);
                Word = new Word.Application();
                Word.Visible = false;
            }
            ReadTemplateLookup();
            ReadExcelSheets();
        }

        private void ReadTemplateLookup()
        {
            DisplayCallback.AddInformation(Strings.StepCreatingTemplateLookup, CurrentStep++);
            foreach (string filename in Tools.EnumerateDocuments())
            {
                TemplateLookup[Tools.KeyFromFilename(Path.GetFileName(filename))] = filename;
            }
        }

        private bool ReadExcelSheets()
        {
            DateTime now = DateTime.Now;
            int nExcelSheetsImported = 0;
            int nExcelSheetsNotImported = 0;

            foreach (string filename in ExcelSheetsToGenerate)
            {
                if (IsCancelFlagSet())
                    return false;

                DisplayCallback.AddInformation(Strings.StepReadingExcelSheets, filename);
                if (CreateManual(filename))
                {
                    ++nExcelSheetsImported;
                }
                else
                {
                    ++nExcelSheetsNotImported;
                }
            }

            if (nExcelSheetsImported > 0)
            {
                DisplayCallback.AddInformation(Strings.NumberOfManualsInDatabase, nExcelSheetsImported, DateTime.Now - now);
            }
            else
            {
                DisplayCallback.AddInformation(Strings.NoManualsToImportInDatabase, DateTime.Now - now);
            }
            if (nExcelSheetsNotImported > 0)
            {
                DisplayCallback.AddWarning(Strings.NumberOfManualsNotImported, nExcelSheetsNotImported);
            }
            return true;
        }

        private bool CreateManual(string excelsheet_filename)
        {
            if (!ImportManualFromExcelSheet(excelsheet_filename))
                return false;

            return true;
        }

        private bool ImportManualFromExcelSheet(string excelsheet_filename)
        {          
            try
            {
                string language;
                PartialManualContent pmc = ReadPartialManualContent(excelsheet_filename, out language);
                if (pmc == null)
                {
                    DisplayCallback.AddError(Strings.ErrorUnableToReadFile, excelsheet_filename);
                    return false;
                }

                DisplayCallback.AddInformation(Strings.ManualHasSoManyParts, pmc.Filenames.Count);

                Manual m = new Manual();
                m.Device = pmc.Title1;
                m.Name = "Manual";
                m.Language = language;
                m.LastUpdated = DateTime.Now;
                m.LastGenerated = new DateTime(2015, 1, 1);
                m.Title1 = pmc.Title1;
                m.Title2 = pmc.Title2;
                m.Title3 = pmc.Title3;
                m.Version = pmc.Version;
                m.TypeOfManual = pmc.TypeOfManual;
                m.Template = pmc.Template;
                m.TargetFilename = pmc.TargetFilename;
                Trace.TraceInformation("m.TargetFilename: {0}", m.TargetFilename);

                if (Program.Settings.UseBookmarksFromExcelSheet)
                {
                    if (!CreateBookmarksLookup(m, pmc))
                        return false;
                }

                return CreateManual(m, pmc);
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ExceptionWhileImportingManual, excelsheet_filename);
                return false;
            }
        }
        
        private bool CreateBookmarksLookup(Manual m, PartialManualContent pmc)
        {
            DisplayCallback.AddInformation(Strings.StepLookingUpBookmarksInDocuments, CurrentStep++,
                pmc.Bookmarks.Count,
                pmc.Filenames.Count);
            DisplayCallback.AddInformation("");
            DateTime now = DateTime.Now;

            foreach (string bookmarkName in pmc.Bookmarks)
            {
                BookmarkToFilenames[bookmarkName.ToLower()] = new List<string>();
            }

            bool failed = false;
            try
            {
                int filenameIndex = 1;
                foreach (string filename in pmc.Filenames)
                {

                    if (IsCancelFlagSet())
                    {
                        failed = true;
                        break;
                    }
                    string key = Tools.KeyFromFilename(filename);
                    if (!Program.Settings.CreateDocumentSortOrderFromBookmarks)
                    {
                        FilenameToBookmarks[key] = new List<string>();
                    }
                    if (!TemplateLookup.ContainsKey(key))
                    {
                        DisplayCallback.AddError(Strings.ErrorKeyNotFoundInKnownTemplates, key);
                        failed = true;
                        continue;
                    }
                    double percentage = filenameIndex / ((pmc.Filenames.Count / 100.0));
                    DisplayCallback.AddInformation("^{0}/{1} = {2:##.##}%: '{3}'", filenameIndex++, pmc.Filenames.Count, percentage, TemplateLookup[key]);

                    if( !CreateBookmarksLookupForFile(pmc, TemplateLookup[key], key) )
                    {
                        failed = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ErrorExceptionWhileCreatingDocument, m.Name);
                failed = true;
            }
            if(!failed)
            {
                if (Program.Settings.CreateDocumentSortOrderFromBookmarks)
                {
                    foreach (string bookmarkName in pmc.Bookmarks)
                    {
                        if (BookmarkToFilenames[bookmarkName.ToLower()].Count == 0)
                        {
                            DisplayCallback.AddError(Strings.ErrorBookmarkDoesNotExistAnywhere, bookmarkName);
                            failed = true;
                        }
                    }
                }
                else
                {
                    foreach (string filename in pmc.Filenames)
                    {
                        if (!Tools.IsSpecialTemplate(filename))
                        {
                            string key = Tools.KeyFromFilename(filename);
                            if (FilenameToBookmarks[key].Count == 0)
                            {
                                DisplayCallback.AddError(Strings.ErrorFileHasNoReferencedBookmarks, TemplateLookup[key]);
                                failed = true;
                            }
                        }
                    }
                }

            }

            if (failed)
            {
                DisplayCallback.AddInformation(Strings.FailedAfterTime, DateTime.Now - now);
                return false;
            }
            DisplayCallback.AddInformation(Strings.SucceededAfterTime, DateTime.Now - now);
            return true;
        }

        private bool CreateBookmarksLookupForFile(PartialManualContent pmc, string filename, string filenameKey)
        {
            bool failed = false;
            string context = "";
            try
            {
                Word._Document doc = Word.Documents.Open(filename);
                try
                {
                    doc.Activate();

                    for (int i = 1, imax = doc.Bookmarks.Count; i <= imax; ++i)
                    {
                        if (failed || IsCancelFlagSet())
                            break;
                        Word.Bookmark bm = doc.Bookmarks[i];
                        context = bm.Name;

                        string bookmarkKey = context.ToLower();

                        if (BookmarkToFilenames.ContainsKey(bookmarkKey))
                        {
                            Trace.TraceInformation("- Lesezeichen '{0}' gefunden in '{1}'", context, filename);
                            if (Program.Settings.CreateDocumentSortOrderFromBookmarks)
                            {
                                BookmarkToFilenames[bookmarkKey].Add(filename);
                            }
                            else
                            {
                                FilenameToBookmarks[filenameKey].Add(bm.Name);
                            }
                        }
                        else
                        {
                            Trace.TraceInformation("- Lesezeichen '{0}' wird nicht benutzt.", context);
                        }
                    }
                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    doc.Close(so);
                    Marshal.ReleaseComObject(doc);
                }
            }
            catch (Exception e)
            {
                DisplayCallback.AddWarning("§Datei: '{0}' (Context: {1})", filename, context);
                DumpException(e, Strings.ErrorUnableToAddTemplateContentsToLookupDocument);
                return false;
            }
            return !failed;
        }

        private bool CreateManual(Manual m, PartialManualContent pmc)
        {
            DisplayCallback.AddInformation(Strings.StepCreatingManualForDeviceInLanguage, CurrentStep++,
                m.Name,
                m.Device,
                m.Language);

            DateTime now = DateTime.Now;
            DisplayCallback.AddInformation("");
            string targetFilename = Tools.GetTargetFilename(m);

            bool failed = false;
            try
            {
                string template = Program.Settings.TemplateFilename_DE;
                if (m.Language.Equals(Strings.Language_EN))
                    template = Program.Settings.TemplateFilename_EN;

                object templatePath = Path.Combine(Program.Settings.BaseDirectory, Program.Settings.TemplatesDirectory, template);
                Word._Document doc = Word.Documents.Add(ref templatePath);
                try
                {

                    if(Program.Settings.UseBookmarksFromExcelSheet)
                    {
                        if(Program.Settings.CreateDocumentSortOrderFromBookmarks)
                        {
                            if (!CreateManualFromDocumentSortOrderFromBookmarks(doc, m, pmc))
                                failed = true;
                        }
                        else
                        {
                            if (!CreateManualFromDocumentSortOrderFromExcel(doc, m, pmc))
                                failed = true;
                        }
                    }
                    else
                    {
                        if (!CreateManualFromLanguageSpecificBookmarks(doc, m, pmc))
                            failed = true;
                    }
                    if (!failed)
                    {
                        for (int s = 1, smax = doc.Sections.Count; s <= smax; ++s)
                        {
                            Word.Section section = doc.Sections[s];
                            Word.HeaderFooter hf = section.Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                            for (int w = 1, wmax = hf.Range.Words.Count; w <= wmax; ++w)
                            {
                                string mord = hf.Range.Words[w].Text;

                                if (mord.Equals("Monat"))
                                    hf.Range.Words[w].Text = ManualGenerator.GermanMonthNames[now.Month];
                                else if (mord.Equals("Month"))
                                    hf.Range.Words[w].Text = ManualGenerator.EnglishMonthNames[now.Month];
                                else if (mord.Equals("Year") || mord.Equals("Jahr"))
                                    hf.Range.Words[w].Text = now.Year.ToString();
                            }
                        }

                        doc.SaveAs2(targetFilename);
                        DisplayCallback.AddInformation("- '{0}'", targetFilename);
                    }
                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    doc.Close(so);
                    Marshal.ReleaseComObject(doc);
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

        private bool CreateManualFromDocumentSortOrderFromExcel(Word._Document doc, Manual m, PartialManualContent pmc)
        {
            int index = 1;

            bool failed = false;
            foreach (string filename in pmc.Filenames)
            {
                if(failed)
                    break;

                string pathname = TemplateLookup[Tools.KeyFromFilename(filename)];
                double percentage = index / ((pmc.Filenames.Count / 100.0));
                

                if (Tools.IsSpecialTemplate(filename))
                {
                    DisplayCallback.AddInformation("^{0}/{1} = {2:##.##}%: '{3}' komplett", index, pmc.Filenames.Count, percentage, pathname);
                    if ( !AddDocumentToDocument(doc, pathname, pageBreak: true, manual: m) )
                    {
                        failed = true;
                        break;
                    }
                }
                else
                {
                    foreach(string bookmarkName in FilenameToBookmarks[Tools.KeyFromFilename(filename)])
                    {
                        DisplayCallback.AddInformation("^{0}/{1} = {2:##.##}%: '{3}' für das Lesezeichen {4}", index, pmc.Filenames.Count, percentage, pathname, bookmarkName);
                        if (IsCancelFlagSet())
                        {
                            failed = true;
                            break;
                        }
                        string bookmarkKey = bookmarkName.ToLower();
                        if( !AddThisBookmarkToDocument(doc, pathname, bookmarkName) )
                        {
                            failed = true;
                            break;
                        }
                    }
                }
                ++index;
            }
            return !failed;
        }

        private bool CreateManualFromDocumentSortOrderFromBookmarks(Word._Document doc, Manual m, PartialManualContent pmc)
        {
            int index = 1;
            int totalFiles = 0;
            foreach (string bookmarkName in pmc.Bookmarks)
            {
                totalFiles += BookmarkToFilenames[bookmarkName.ToLower()].Count;
            }

            bool failed = false;
            foreach (string bookmarkName in pmc.Bookmarks)
            {
                string bookmarkKey = bookmarkName.ToLower();
                foreach (string filename in BookmarkToFilenames[bookmarkKey])
                {
                    if (IsCancelFlagSet())
                    {
                        failed = true;
                        break;
                    }

                    double percentage = index / ((totalFiles / 100.0));
                    DisplayCallback.AddInformation("^{0}/{1} = {2:##.##}%: '{3}' für das Lesezeichen {4}", index++, totalFiles, percentage, filename, bookmarkName);
                    if (Tools.IsSpecialTemplate(filename))
                    {
                        AddDocumentToDocument(doc, filename, pageBreak: true, manual: m);
                    }
                    else
                    {
                        AddThisBookmarkToDocument(doc, filename, bookmarkName);
                    }
                }
            }
            return !failed;
        }

        private bool CreateManualFromLanguageSpecificBookmarks(Word._Document doc, Manual m, PartialManualContent pmc)
        {
            int index = 1;
            bool failed = false;
            foreach (string filename in pmc.Filenames)
            {
                if (IsCancelFlagSet())
                {
                    failed = true;
                    break;
                }
                string key = Tools.KeyFromFilename(filename);
                if (!TemplateLookup.ContainsKey(key))
                {
                    DisplayCallback.AddError(Strings.ErrorKeyNotFoundInKnownTemplates, key);
                    failed = true;
                    continue;
                }
                double percentage = index / ((pmc.Filenames.Count / 100.0));
                DisplayCallback.AddInformation("^{0}/{1} = {2:##.##}%: '{3}'", index++, pmc.Filenames.Count, percentage, TemplateLookup[key]);
                if (Tools.IsSpecialTemplate(filename))
                {
                    AddDocumentToDocument(doc, TemplateLookup[key], pageBreak: true, manual: m);
                }
                else
                {
                    AddBookmarksFromExistingDocument(doc, TemplateLookup[key], m.Language);
                }
            }
            return !failed;
        }

        private bool AddThisBookmarkToDocument(Word._Document doc, string filename, string bookmarkName)
        {
            bool failed = false;
            string context = "";
            try
            {
                Word._Document content = Word.Documents.Open(filename);
                try
                {
                    content.Activate();

                    Word.Bookmark bm = content.Bookmarks[bookmarkName];
                    bm.Range.Copy();
                    object start = doc.Content.End - 1;
                    object end = doc.Content.End;
                    Word.Range rng = doc.Range(ref start, ref end);
                    rng.Select();
                    rng.Paste();
                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    content.Close(so);
                    Marshal.ReleaseComObject(content);
                }
            }
            catch (Exception e)
            {
                DisplayCallback.AddWarning("§Datei: '{0}' (Context: {1})", filename, context);
                DumpException(e, Strings.ErrorUnableToAddTemplateContentsToLookupDocument);
                return false;
            }
            return !failed;
        }

        private bool AddBookmarksFromExistingDocument(Word._Document lookupDocument, string filename, string language)
        {
            bool failed = false;
            string context = "";
            try
            {
                bool isEnglish = language.Equals(Strings.Language_EN, StringComparison.OrdinalIgnoreCase);
                Word._Document currentTemplateDocument = Word.Documents.Open(filename);
                try
                {
                    currentTemplateDocument.Activate();                    

                    for (int i = 1, imax = currentTemplateDocument.Bookmarks.Count; i <= imax; ++i)
                    {
                        if (failed || IsCancelFlagSet())
                            break;
                        Word.Bookmark bm = currentTemplateDocument.Bookmarks[i];
                        context = bm.Name;
                        if (!bm.Name.StartsWith("TEMPLATE_"))
                        {
                            if (isEnglish && !bm.Name.EndsWith("E"))
                            {
                                Trace.TraceWarning("Ignore bookmark '{0}' because language is '{1}'", bm.Name, language);
                                continue;
                            }
                            else if (!isEnglish && bm.Name.EndsWith("E"))
                            {
                                Trace.TraceWarning("Ignore bookmark '{0}' because language is '{1}'", bm.Name, language);
                                continue;
                            }

                            // ok, this bookmark needs to be copied
                            Word.Range completeRange = bm.Range;
                            for (int j = 1, jmax = completeRange.Paragraphs.Count; j <= jmax; ++j)
                            {
                                if(IsCancelFlagSet())
                                {
                                    failed = true;
                                    break;
                                }
                                string name = completeRange.Paragraphs[j].get_Style().NameLocal;
                                if (name.StartsWith("Überschrift 1;"))
                                {
                                    object start2 = lookupDocument.Content.End - 1;
                                    object end2 = lookupDocument.Content.End;
                                    Word.Range rng2 = lookupDocument.Range(ref start2, ref end2);
                                    rng2.InsertBreak(Microsoft.Office.Interop.Word.WdBreakType.wdPageBreak);
                                    break;
                                }
                            }
                            bm.Range.Copy();
                            object start = lookupDocument.Content.End - 1;
                            object end = lookupDocument.Content.End;
                            Word.Range rng = lookupDocument.Range(ref start, ref end);
                            rng.Select();
                            rng.Paste();
                        }
                        else
                        {
                            Trace.TraceWarning("Ignoring this bookmark: {0}", bm.Name);
                        }
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
                DisplayCallback.AddWarning("§Datei: '{0}' (Context: {1})", filename, context);
                DumpException(e, Strings.ErrorUnableToAddTemplateContentsToLookupDocument);
                return false;
            }
            return !failed;
        }

        private PartialManualContent ReadPartialManualContent(string filename, out string expected_language)
        {
            expected_language = "";
            DisplayCallback.AddInformation(Strings.ReadingExcelFile, filename);
            if (Excel == null)
            {
                Excel = new Excel.Application();
            }
            PartialManualContent manual = null;
            try
            {
                Excel.Workbook wb = Excel.Workbooks.Open(filename);
                try
                {
                    Excel.Worksheet ws = (Excel.Worksheet)wb.Sheets[1];

                    Excel.Range excelRange = ws.UsedRange;
                    object[,] valueArray = (object[,])excelRange.get_Value(
                        Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault);

                    manual = new PartialManualContent(DisplayCallback);
                    if (!manual.ConstructFromExcel(filename, valueArray, null, ref expected_language))
                    {
                        manual = null;
                    }
                }
                finally
                {
                    wb.Close(false, filename, null);
                    Marshal.ReleaseComObject(wb);
                }
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ExceptionWhileImportingManual, filename);
            }
            return manual;
        }

        private void ReplaceBookmark(Word._Document currentTemplateDocument, string bmName, string newContent)
        {
            if (!string.IsNullOrEmpty(newContent))
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
                string pathname = Tools.GetDocumentFilename(templateName);
                Word._Document currentTemplateDocument = Word.Documents.Open(pathname);
                try
                {
                    currentTemplateDocument.Activate();
                    Word.Range rng;
                    Trace.TraceInformation("- Document {0} has {1} bookmarks", pathname, currentTemplateDocument.Bookmarks.Count);

                    if (currentTemplateDocument.Bookmarks.Count > 0)
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

                    if (pageBreak)
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


        public override void Dispose()
        {
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
        }
    }
}
