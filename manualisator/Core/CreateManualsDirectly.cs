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
using log4net;
using System.Reflection;

namespace manualisator.Core
{
    public class CreateManualsDirectly : LongRunningTask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType); 
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
            // Ensure Word and Excel are running
            Word = new Word.Application();
            Word.Visible = false;
            Excel = new Excel.Application();
            Excel.Visible = false;
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
                if (CreateManualFromDocumentStructure(filename))
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

        private bool CreateManualFromDocumentStructure(string excelSheetFilename)
        {          
            try
            {
                PartialManualContent pmc = ReadDocumentStructure(excelSheetFilename);
                if (pmc == null)
                {
                    DisplayCallback.AddError(Strings.ErrorUnableToReadFile, excelSheetFilename);
                    return false;
                }

                DisplayCallback.AddInformation(Strings.ManualHasSoManyParts, pmc.Filenames.Count);
                if (Program.Settings.UseBookmarksFromExcelSheet)
                {
                    if (!CreateBookmarksLookup(pmc))
                        return false;
                }

                return CreateManual(pmc);
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ExceptionWhileImportingManual, excelSheetFilename);
                return false;
            }
        }
        
        private bool CreateBookmarksLookup(PartialManualContent pmc)
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
                    if(Tools.IsSpecialTemplate(filename))
                    {
                        Log.WarnFormat("Ignoring this: {0}", filename);
                        continue;
                    }

                    string key = Tools.KeyFromFilename(filename);
                    if (!TemplateLookup.ContainsKey(key))
                    {
                        DisplayCallback.AddError(Strings.ErrorKeyNotFoundInKnownTemplates, key);
                        failed = true;
                        continue;
                    }
                    FilenameToBookmarks[key] = new List<string>();
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
                DumpException(e, Strings.ErrorExceptionWhileCreatingDocument, pmc.TargetFilename);
                failed = true;
            }
            if(!failed)
            {
                foreach (string bookmarkName in pmc.Bookmarks)
                {
                    if (BookmarkToFilenames[bookmarkName.ToLower()].Count == 0)
                    {
                        DisplayCallback.AddError(Strings.ErrorBookmarkDoesNotExistAnywhere, bookmarkName);
                        failed = true;
                    }
                }
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

            if (failed)
            {
                DisplayCallback.AddInformation(Strings.FailedAfterTime, DateTime.Now - now);
                return false;
            }
            DisplayCallback.AddInformation(Strings.SucceededAfterTime, DateTime.Now - now);
            return true;
        }


        private void ReplaceTextInFooter(Word.Section section, PartialManualContent pmc)
        {
            for (int footerIndex = 1; footerIndex <= section.Footers.Count; ++footerIndex)
            {
                var fi = (Microsoft.Office.Interop.Word.WdHeaderFooterIndex)footerIndex;
                Log.InfoFormat("Looking at this footer;: {0}", fi);
                Word.HeaderFooter hf = section.Footers[fi];
                DateTime now = DateTime.Now;

                for (int f = 1; f <= hf.Range.Fields.Count; ++f)
                {
                    var field = hf.Range.Fields[f];
                    Log.InfoFormat("field {0}: {1}", f, field.Type);
                    if (field.Type == Microsoft.Office.Interop.Word.WdFieldType.wdFieldKeyWord)
                    {
                        // presumably, they want to replace this by the text 
                        field.Select();
                        field.Delete();
                        Word.Selection.TypeText(pmc.OrderNr);
                    }
                }

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
                            Log.InfoFormat("- Lesezeichen '{0}' gefunden in '{1}'", context, filename);
                            BookmarkToFilenames[bookmarkKey].Add(filename);
                            FilenameToBookmarks[filenameKey].Add(bm.Name);
                        }
                        else
                        {
                            Log.InfoFormat("- Lesezeichen '{0}' wird nicht benutzt.", context);
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

        private bool CreateManual(PartialManualContent pmc)
        {
            DisplayCallback.AddInformation(Strings.StepCreatingManualForDeviceInLanguage, CurrentStep++,
                "",
                pmc.Title1,
                pmc.Language);

            DateTime now = DateTime.Now;
            DisplayCallback.AddInformation("");
            string targetFilename = Tools.GetTargetFilename(pmc);
            if (Program.Settings.WarnBeforeOverwriting)
            {
                if (File.Exists(targetFilename))
                {
                    // need to display warning, ask for overwrite
                    var box = new WarnBeforeOverwriteForm(targetFilename);
                    if( box.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return false;
                    }
                    targetFilename = box.NewFilename;
                }
            }

            bool failed = false;
            try
            {
                string template = Program.Settings.TemplateFilename_DE;
                if (pmc.Language.Equals(Strings.Language_EN))
                    template = Program.Settings.TemplateFilename_EN;

                object templatePath = Path.Combine(Program.Settings.BaseDirectory, Program.Settings.TemplatesDirectory, template);
                Word._Document doc = Word.Documents.Add(ref templatePath);
                try
                {

                    if(Program.Settings.UseBookmarksFromExcelSheet)
                    {
                        if(Program.Settings.CreateDocumentSortOrderFromBookmarks)
                        {
                            if (!CreateManualFromDocumentSortOrderFromBookmarks(doc, pmc))
                                failed = true;
                        }
                        else
                        {
                            if (!CreateManualFromDocumentSortOrderFromExcel(doc, pmc))
                                failed = true;
                        }
                    }
                    else
                    {
                        if (!CreateManualUsingAutomaticBookmarkSelection(doc, pmc))
                            failed = true;
                    }
                    if (!failed)
                    {
                        for (int s = 1, smax = doc.Sections.Count; s <= smax; ++s)
                        {
                            ReplaceTextInFooter(doc.Sections[s], pmc);
                        }

                        InsertBreaksBeforeHeaders(doc);
                        UpdateAllTablesOfContent(doc);
                        UpdateAllTablesOfFigures(doc);

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
                DumpException(e, Strings.ErrorExceptionWhileCreatingDocument, targetFilename);
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

        private bool CreateManualFromDocumentSortOrderFromExcel(Word._Document doc, PartialManualContent pmc)
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
                    if ( !InsertDocument(doc, pathname, pmc) )
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
                        if( !InsertBookmarkFromDocument(doc, pathname, bookmarkName) )
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

        private bool CreateManualFromDocumentSortOrderFromBookmarks(Word._Document doc, PartialManualContent pmc)
        {
            int index = 1;
            int totalFiles = 0;
            foreach (string bookmarkName in pmc.Bookmarks)
            {
                totalFiles += BookmarkToFilenames[bookmarkName.ToLower()].Count;
            }

            // insert leading bookmark templates 
            for (int i = 0, imax = pmc.Filenames.Count; i < imax; ++i )
            {
                string filename = pmc.Filenames[i];
                string pathname = TemplateLookup[Tools.KeyFromFilename(filename)];
                if (Tools.IsSpecialTemplate(filename))
                {
                    if (!InsertDocument(doc, pathname, pmc))
                        return false;
                }
                else break;
            }


            foreach (string bookmarkName in pmc.Bookmarks)
            {
                string bookmarkKey = bookmarkName.ToLower();
                foreach (string filename in BookmarkToFilenames[bookmarkKey])
                {
                    if (IsCancelFlagSet())
                        return false;

                    double percentage = index / ((totalFiles / 100.0));
                    DisplayCallback.AddInformation("^{0}/{1} = {2:##.##}%: '{3}' für das Lesezeichen {4}", index++, totalFiles, percentage, filename, bookmarkName);                   
                    if (!InsertBookmarkFromDocument(doc, filename, bookmarkName))
                        return false;
                }
            }

            // insert trailing bookmark templates 
            for (int i = pmc.Filenames.Count-1, imin = 0; i >= imin; --i)
            {
                string filename = pmc.Filenames[i];
                string pathname = TemplateLookup[Tools.KeyFromFilename(filename)];
                if (Tools.IsSpecialTemplate(filename))
                {
                    if (!InsertDocument(doc, pathname, pmc))
                        return false;
                }
                else break;
            }
            return true;
        }

        void UpdateAllTablesOfFigures(Word._Document doc)
        {
            doc.TablesOfFigures.Format = Microsoft.Office.Interop.Word.WdTofFormat.wdTOFClassic;
            foreach(Microsoft.Office.Interop.Word.TableOfFigures tof in doc.TablesOfFigures)
            {
                tof.Update();
            }
        }

        void UpdateAllTablesOfContent(Word._Document doc)
        {
            foreach(Microsoft.Office.Interop.Word.TableOfContents toc in doc.TablesOfContents)
            {
                toc.Update();
            }
        }

        bool InsertBreaksBeforeHeaders(Word._Document doc)
        {
            if (Program.Settings.InsertBeforeHeading1 == 0)
                return true;
            var breakType = 
                (Program.Settings.InsertBeforeHeading1 == 1) ?
                    Microsoft.Office.Interop.Word.WdBreakType.wdPageBreak :
                    Microsoft.Office.Interop.Word.WdBreakType.wdSectionBreakNextPage;

            Log.InfoFormat("InsertBreaksBeforeHeaders: {0}", breakType);

            Word.Range completeRange = doc.Range();

            object rngStart = completeRange.Start;
            object rngEnd = completeRange.End;

            while (true)
            {
                Log.InfoFormat("Search this range: {0}..{1}", rngStart, rngEnd);
                var rng = doc.Range(rngStart, rngEnd);

                rng.Find.ClearFormatting();
                rng.Find.Format = true;
                rng.Find.Forward = true;
                rng.Find.ParagraphFormat.OutlineLevel = Microsoft.Office.Interop.Word.WdOutlineLevel.wdOutlineLevel1;
                if (!rng.Find.Execute())
                    break;

                if (!rng.Find.Found)
                    break;

                Log.InfoFormat("Found h1 in {0}..{1}: '{2}'",
                    rng.Start,
                    rng.End,
                    rng.Text);

                object start2 = rng.Start;
                object end2 = rng.Start;
                rngStart = rng.End + 2;
                var oldRange = doc.Range(ref start2, ref end2);
                Log.InfoFormat("Old trace: {0}", oldRange.Text);
                oldRange.InsertBreak(breakType);
            }

            return true;
        }

        /// <summary>
        /// Copy bookmark from to target document
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="bm"></param>
        /// <returns></returns>
        private bool CopyBookmark(Word._Document doc, Word.Bookmark bm, Word._Document fileToInsert)
        {
            bm.Range.Copy();
            object start = doc.Content.End - 1;
            object end = doc.Content.End;
            Word.Range rng = doc.Range(ref start, ref end);
            rng.Select();
            rng.Paste();
            return true;
        }

        /// <summary>
        /// TODO: add cache for most recently used document ? in case of more than one bookmark needs to be inserted
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="nameOfFileToInsert"></param>
        /// <param name="bookmarkName"></param>
        /// <returns></returns>
        private bool InsertBookmarkFromDocument(Word._Document doc, string nameOfFileToInsert, string bookmarkName)
        {
            bool result = true;
            try
            {
                Word._Document fileToInsert = Word.Documents.Open(nameOfFileToInsert);
                try
                {
                    fileToInsert.Activate();
                    result = CopyBookmark(doc, fileToInsert.Bookmarks[bookmarkName], fileToInsert);
                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    fileToInsert.Close(so);
                    Marshal.ReleaseComObject(fileToInsert);
                }
            }
            catch (Exception e)
            {
                DisplayCallback.AddWarning("§Datei: '{0}'", nameOfFileToInsert);
                DumpException(e, Strings.ErrorUnableToAddTemplateContentsToLookupDocument);
                return false;
            }
            return result;
        }

        /// <summary>
        /// Insert those bookmarks choosen by automatic bookmark selection
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="nameOfFileToInsert"></param>
        /// <param name="documentStructure"></param>
        /// <returns></returns>
        private bool InsertDocumentContentUsingAutomaticBookmarkSelection(Word._Document doc, string nameOfFileToInsert, PartialManualContent documentStructure)
        {
            bool result = true;
            string context = "";
            try
            {
                bool isEnglish = documentStructure.Language.Equals(Strings.Language_EN, StringComparison.OrdinalIgnoreCase);
                Word._Document fileToInsert = Word.Documents.Open(nameOfFileToInsert);
                try
                {
                    fileToInsert.Activate();                    

                    for (int i = 1, imax = fileToInsert.Bookmarks.Count; i <= imax; ++i)
                    {
                        if (IsCancelFlagSet())
                            return false;
                        Word.Bookmark bm = fileToInsert.Bookmarks[i];
                        context = bm.Name;
                        if (!bm.Name.StartsWith("TEMPLATE_"))
                        {
                            if (isEnglish && !bm.Name.EndsWith("E"))
                            {
                                Log.WarnFormat("Ignore bookmark '{0}' because language is '{1}'", bm.Name, documentStructure.Language);
                                continue;
                            }
                            else if (!isEnglish && bm.Name.EndsWith("E"))
                            {
                                Log.WarnFormat("Ignore bookmark '{0}' because language is '{1}'", bm.Name, documentStructure.Language);
                                continue;
                            }

                            // ok, this bookmark needs to be copied
                            result = CopyBookmark(doc, bm, fileToInsert);
                        }
                        else
                        {
                            Log.WarnFormat("Ignoring this bookmark: {0}", bm.Name);
                        }
                    }
                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    fileToInsert.Close(so);
                    Marshal.ReleaseComObject(fileToInsert);
                }
            }
            catch (Exception e)
            {
                DisplayCallback.AddWarning("§Datei: '{0}' (Context: {1})", nameOfFileToInsert, context);
                DumpException(e, Strings.ErrorUnableToAddTemplateContentsToLookupDocument);
                return false;
            }
            return result;
        }

        /// <summary>
        /// Read document structure from excel sheet
        /// </summary>
        /// <param name="excelFilename"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private PartialManualContent ReadDocumentStructure(string excelFilename)
        {
            DisplayCallback.AddInformation(Strings.ReadingExcelFile, excelFilename);
            PartialManualContent manual = null;
            try
            {
                Excel.Workbook wb = Excel.Workbooks.Open(excelFilename);
                try
                {
                    Excel.Worksheet ws = (Excel.Worksheet)wb.Sheets[1];

                    Excel.Range excelRange = ws.UsedRange;
                    object[,] valueArray = (object[,])excelRange.get_Value(
                        Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault);

                    manual = new PartialManualContent(DisplayCallback);
                    string language = "";
                    if (!manual.ConstructFromExcel(excelFilename, valueArray, null, ref language))
                    {
                        manual = null;
                    }
                }
                finally
                {
                    wb.Close(false, excelFilename, null);
                    Marshal.ReleaseComObject(wb);
                }
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ExceptionWhileImportingManual, excelFilename);
                manual = null;
            }
            return manual;
        }

        /// <summary>
        /// In a given document, replace all bookmarks by name
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="replacementTable"></param>
        private void ReplaceBookmarks(Word._Document doc, Dictionary<string,string> replacementTable)
        {
            for (int i = 1; i <= doc.Bookmarks.Count; ++i)
            {
                Word.Bookmark bm = doc.Bookmarks[i];
                string key = bm.Name.ToLower();
                if (replacementTable.ContainsKey(key))
                {
                    string value = replacementTable[key];
                    if (string.IsNullOrEmpty(value))
                        value = "";

                    bm.Range.Text = value;
                    --i;
                }
                else Log.WarnFormat("Found unknown bookmark '{0}'", bm.Name);
            }
        }

        /// <summary>
        /// Add a complete document, specified by 'templateName', to the existing document, without selecting any special bookmarks.
        /// In this copy, replace standard variables.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="nameOfFileToInsert"></param>
        /// <param name="documentStructure"></param>
        /// <returns></returns>
        private bool InsertDocument(Word._Document doc, string nameOfFileToInsert, PartialManualContent documentStructure)
        {
            Log.InfoFormat("AddDocumentToDocument(nameOfFileToInsert:{0})", nameOfFileToInsert);
            try
            {
                string templatePath = Tools.GetDocumentFilename(nameOfFileToInsert);
                Log.InfoFormat("- templatePath: {0}", templatePath);
                Word._Document fileToInsert = Word.Documents.Open(templatePath);
                try
                {
                    fileToInsert.Activate();

                    Log.InfoFormat("- templateDocument.Bookmarks.Count: {0}", fileToInsert.Bookmarks.Count);
                    if (fileToInsert.Bookmarks.Count > 0)
                    {
                        var replacementTable = new Dictionary<string,string>();
                        replacementTable["titel_1"] = documentStructure.Title1;
                        replacementTable["titel_2"] = documentStructure.Title2;
                        replacementTable["titel_3"] = documentStructure.Title3;
                        if (Program.Settings.AutoGenerateCurrentDate)
                        {
                            DateTime now = DateTime.Now;

                            if (documentStructure.Language == "en")
                            {
                                replacementTable["mon_jahr"] = string.Format("{0} {1}", ManualGenerator.EnglishMonthNames[now.Month], now.Year);
                            }
                            else
                            {
                                replacementTable["mon_jahr"] = string.Format("{0} {1}", ManualGenerator.GermanMonthNames[now.Month], now.Year);
                            }
                        }
                        else
                        {
                            replacementTable["mon_jahr"] = documentStructure.OrderNr;
                        }
                        replacementTable["hb_typ"] = documentStructure.TypeOfManual;
                        replacementTable["version"] = documentStructure.Version;
                        ReplaceBookmarks(fileToInsert, replacementTable);
                    }

                    object start = fileToInsert.Content.Start;
                    object end = fileToInsert.Content.End;
                    Word.Range rng = fileToInsert.Range(ref start, ref end);
                    rng.Copy();

                    doc.Activate();
                    start = doc.Content.End - 1;
                    end = doc.Content.End;
                    rng = doc.Range(ref start, ref end);
                    rng.Select();
                    rng.Paste();

                    doc.Words.Last.InsertBreak(Microsoft.Office.Interop.Word.WdBreakType.wdPageBreak);
                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    fileToInsert.Close(so);
                    Marshal.ReleaseComObject(fileToInsert);
                }
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ErrorDocumentFailedToConvert, nameOfFileToInsert);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create manual using automatic bookmark selection
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="documentStructure"></param>
        /// <returns></returns>
        private bool CreateManualUsingAutomaticBookmarkSelection(Word._Document doc, PartialManualContent documentStructure)
        {
            int index = 1;
            bool result = true;
            foreach (string filename in documentStructure.Filenames)
            {
                if (IsCancelFlagSet())
                    return false;

                string key = Tools.KeyFromFilename(filename);
                if (!TemplateLookup.ContainsKey(key))
                {
                    DisplayCallback.AddError(Strings.ErrorKeyNotFoundInKnownTemplates, key);
                    result = false;
                    continue;
                }
                string templatePathname = TemplateLookup[key];
                double percentage = index / ((documentStructure.Filenames.Count / 100.0));

                DisplayCallback.AddInformation("^{0}/{1} = {2:##.##}%: '{3}'", index++, documentStructure.Filenames.Count, percentage, templatePathname);
                if (Tools.IsSpecialTemplate(filename))
                {
                    if (!InsertDocument(doc, templatePathname, documentStructure))
                        return false;
                }
                else
                {
                    if (!InsertDocumentContentUsingAutomaticBookmarkSelection(doc, templatePathname, documentStructure))
                        return false;
                }
            }
            return result;
        }

        /// <summary>
        /// Cleanup this instance
        /// </summary>
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
