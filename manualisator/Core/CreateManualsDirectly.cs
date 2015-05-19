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
            ReadTemplateLookup();
            ReadExcelSheets();
        }

        private void ReadTemplateLookup()
        {
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
                PartialManualContent first = ReadPartialManualContent(excelsheet_filename, out language);
                if (first == null)
                {
                    DisplayCallback.AddError(Strings.ErrorUnableToReadFile, excelsheet_filename);
                    return false;
                }

                DisplayCallback.AddInformation(Strings.ManualHasSoManyParts, first.Filenames.Count);

                Manual m = new Manual();
                m.Device = first.Title1;
                m.Name = "Manual";
                m.Language = language;
                m.LastUpdated = DateTime.Now;
                m.LastGenerated = new DateTime(2015, 1, 1);
                m.Title1 = first.Title1;
                m.Title2 = first.Title2;
                m.Title3 = first.Title3;
                m.Version = first.Version;
                m.TypeOfManual = first.TypeOfManual;
                m.Template = first.Template;
                m.TargetFilename = first.TargetFilename;
                Trace.TraceInformation("m.TargetFilename: {0}", m.TargetFilename);
                return CreateManual(m, first.Filenames);
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ExceptionWhileImportingManual, excelsheet_filename);
                return false;
            }
        }

        private bool CreateManual(Manual m, List<string> filenames)
        {
            DisplayCallback.AddInformation(Strings.StepCreatingManualForDeviceInLanguage, CurrentStep++,
                m.Name,
                m.Device,
                m.Language);

            DateTime now = DateTime.Now;

            string targetFilename = Tools.GetTargetFilename(m);

            if (Word == null)
            {
                DisplayCallback.AddInformation(Strings.NeedToCreateNewInstanceOfWord);
                Word = new Word.Application();
                Word.Visible = false;
            }
            DisplayCallback.AddInformation("");
            bool failed = false;
            try
            {
                string template = Program.Settings.TemplateFilename_DE;
                if (m.Language.Equals(Strings.Language_EN))
                    template = Program.Settings.TemplateFilename_EN;

                object templatePath = Path.Combine(Program.Settings.BaseDirectory, Program.Settings.TemplatesDirectory, template);
                Word._Document newDocument = Word.Documents.Add(ref templatePath);
                try
                {
                    int index = 1;

                    foreach (string filename in filenames)
                    {
                        if (IsCancelFlagSet())
                        {
                            failed = true;
                            break;
                        }
                        string key = Tools.KeyFromFilename(filename);
                        if(!TemplateLookup.ContainsKey(key))
                        {
                            DisplayCallback.AddError(Strings.ErrorKeyNotFoundInKnownTemplates, key);
                            failed = true;
                            continue;
                        }
                        double percentage = index / ((filenames.Count / 100.0));
                        DisplayCallback.AddInformation("^{0}/{1} = {2:##.##}%: '{3}'", index++, filenames.Count, percentage, TemplateLookup[key]);
                        if (Tools.IsSpecialTemplate(filename))
                        {
                            AddDocumentToDocument(newDocument, TemplateLookup[key], pageBreak: true, manual: m);
                        }
                        else if(Program.Settings.UseLanguageSpecificBookmarks) 
                        {
                            AddBookmarksFromExistingDocument(newDocument, TemplateLookup[key], m.Language);
                        }
                        else
                        {
                            AddDocumentToDocument(newDocument, TemplateLookup[key], pageBreak: false, manual: m);
                        }
                    }
                    if (!failed)
                    {
                        for (int s = 1, smax = newDocument.Sections.Count; s <= smax; ++s)
                        {
                            Word.Section section = newDocument.Sections[s];

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
