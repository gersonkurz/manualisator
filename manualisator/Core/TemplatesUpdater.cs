using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using manualisator.DBSchema;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace manualisator.Core
{
    public class TemplatesUpdater : LongRunningTask
    {
        private readonly string DatabaseFilename;
        private DatabaseServices Database = null;
        private int CurrentStep = 1;
        private Word._Application Word = null;
        private Excel._Application Excel = null;
        private Dictionary<string, TemplateInfo> KnownTemplates = new Dictionary<string, TemplateInfo>();
        private long ManualID = 1;
        private long ContentID = 1;
        private int CurrentLinesInserted = 0;

        public static bool UseOnlyOneManual = false;

        public TemplatesUpdater()
        {
            DatabaseFilename = Settings.ManualsDatabasePathname;
        }

        public override bool Initialize(IDisplayCallback displayCallback)
        {
            base.Initialize(displayCallback);

            if (File.Exists(DatabaseFilename))
            {
                DisplayCallback.AddInformation(Strings.OpeningExistingDatabase, DatabaseFilename);
            }
            else
            {
                DisplayCallback.AddInformation(Strings.CreatingNewDatabase, DatabaseFilename);
            }

            Database = new manualisator.Core.DatabaseServices(DatabaseFilename);
            return true;
        }

        public override void Run()
        {
            ConvertDocFiles();
            if (!IsCancelFlagSet())
            {
                UpdateTemplates();
                if (!IsCancelFlagSet())
                {
                    UpdateExcelSheets();
                    if (!IsCancelFlagSet())
                    {
                        DetermineWhichManualsAreNeeded();
                    }
                }
            }
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

        private bool DetermineWhichManualsAreNeeded()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.DetermineWhichTemplatesAreNeeded, CurrentStep++);

            DateTime now = DateTime.Now;

            // select all manuals that need updating
            List<Manual> manuals = Database.SelectAll<Manual>();
            DisplayCallback.AddInformation(Strings.NumberOfManualsThatNeedUpdating, manuals.Count);

            if (manuals.Count > 0)
            {
                // create lookup: manual ID -> manual
                Dictionary<long, Manual> manualByID = new Dictionary<long, Manual>();
                foreach (Manual manual in manuals)
                {
                    manualByID[manual.ID] = manual;
                }

                // read templates 
                List<Template> templates = Database.SelectAll<Template>();
                Dictionary<long, Template> templateByID = new Dictionary<long, Template>();
                foreach (Template t in templates)
                {
                    templateByID[t.ID] = t;
                }

                // select all templates
                List<ManualContents> contents = Database.SelectAll<ManualContents>();
                Dictionary<long, List<long>> templatesForManual = new Dictionary<long, List<long>>();
                HashSet<long> templatesNeeded = new HashSet<long>();

                foreach (ManualContents mc in contents)
                {
                    if (!templatesForManual.ContainsKey(mc.ManualID))
                        templatesForManual[mc.ManualID] = new List<long>();

                    templatesForManual[mc.ManualID].Add(mc.TemplateID);
                    templatesNeeded.Add(mc.TemplateID);
                }

                double percentage = templatesNeeded.Count / ((templateByID.Count / 100.0));

                DisplayCallback.AddInformation(
                    Strings.NumberOfTemplatesNeeded,
                    templatesNeeded.Count,
                    templateByID.Count,
                    percentage,
                    DateTime.Now - now);
                return true;
            }
            else
            {
                DisplayCallback.AddInformation(Strings.NoTemplatesNeeded, DateTime.Now - now);
                return false;
            }
        }

        private void ConvertDocFiles()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.StepConvertingDocToDocx, CurrentStep++);

            DateTime now = DateTime.Now;

            List<string> filesToRemove = new List<string>();
            int nFilesConverted = 0;
            foreach (string filename in Settings.EnumerateDocuments())
            {
                if (filename.ToLower().EndsWith(".doc") && !
                    filename.StartsWith("~")) // skip hidden files
                {
                    if( ConvertDocFile(filename) )
                    {
                        ++nFilesConverted;
                        filesToRemove.Add(filename);
                    }
                }
                if (IsCancelFlagSet())
                    return;
            }
            if( nFilesConverted > 0 )
            {
                foreach (string filename in filesToRemove)
                {
                    DisplayCallback.AddInformation(Strings.AboutToRemoveFile, filename);
                    File.Delete(filename);
                }
                DisplayCallback.AddInformation(Strings.ConvertedDocFilesInDirectory, nFilesConverted, DateTime.Now - now);
            }
            else
            {
                DisplayCallback.AddInformation(Strings.NoFilesToConvert);
            }
        }

        private bool ConvertDocFile(string filename)
        {
            DisplayCallback.AddInformation(Strings.FileMustBeConverted, filename);
            if( Word == null )
            {
                DisplayCallback.AddInformation(Strings.CreatingInstanceOfWordAutomation);
                Word = new Word.Application();
                Word.Visible = false;
            }
            try
            {
                Word._Document document = Word.Documents.Open(filename);
                try
                {
                    if (Word.ActiveWindow.View.Type == Microsoft.Office.Interop.Word.WdViewType.wdReadingView)
                        Word.ActiveWindow.View.Type = Microsoft.Office.Interop.Word.WdViewType.wdPrintView;
                    string newFilename = filename + "x";
                    DisplayCallback.AddInformation(Strings.AboutToSaveAsDocx, newFilename);
                    document.Convert();
                    document.SaveAs2(newFilename);
                }
                finally
                {
                    document.Close();
                    Marshal.ReleaseComObject(document);
                }
            }
            catch(Exception e)
            {
                DumpException(e, Strings.ExceptionWhileConvertingDocFiles, filename);
                return false;
            }
            return true;
        }

        private void ReadExistingTemplates()
        {
            List<Template> existingTemplates = Database.SelectAll<Template>();
            KnownTemplates.Clear();
            foreach (Template existingTemplate in existingTemplates)
            {
                string key = Tools.KeyFromFilename(existingTemplate.Name);
                KnownTemplates[key] = new TemplateInfo(existingTemplate.ID, existingTemplate.Name);
            }
            if (existingTemplates.Count == 0)
            {
                DisplayCallback.AddInformation(Strings.DatabaseHasNoExistingTemplates);
            }
            else
            {
                DisplayCallback.AddInformation(Strings.NumberOfTemplatesInDatabase, KnownTemplates.Count);
            }
        }

        private void UpdateTemplates()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.StepUpdatingTemplates, CurrentStep++);

            DateTime now = DateTime.Now;
            Database.DeleteAll<Template>();
            ReadExistingTemplates();
            int nTemplatesInserted = 0;
            CurrentLinesInserted = 0;
            foreach (string filename in Settings.EnumerateDocuments())
            {
                string filename_only = Path.GetFileName(filename);
                string key = filename_only.ToLower();
                if(key.EndsWith(".docx") || key.EndsWith(".doc") )
                {
                    if (IsCancelFlagSet())
                        return;

                    if (!KnownTemplates.ContainsKey(key))
                    {
                        Template t = new Template();

                        t.Name = filename_only;
                        t.LastUpdated = File.GetLastWriteTime(filename);
                        if (CurrentLinesInserted++ == 0)
                        {
                            DisplayCallback.AddInformation(Strings.CreatingNewTemplateForName, t.Name);
                        }
                        else
                        {
                            DisplayCallback.AddInformation("^" + Strings.CreatingNewTemplateForName, t.Name);
                        }
                        Database.Insert(t);
                        ++nTemplatesInserted;
                    }
                }
            }
            if (nTemplatesInserted > 0)
            {
                DisplayCallback.AddInformation(Strings.InsertedTotalOfNewTemplates, nTemplatesInserted, DateTime.Now - now);
                ReadExistingTemplates();   
            }
            else
            {
                DisplayCallback.AddInformation(Strings.NoTemplatesToInsert);
            }
        }

        private void UpdateExcelSheets()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.StepImportingManuals, CurrentStep++);

            Database.DeleteAll<ManualContents>();
            Database.DeleteAll<Manual>();
            

            DateTime now = DateTime.Now;
            int nExcelSheetsImported = 0;
            int nExcelSheetsNotImported = 0;

            if (!UseOnlyOneManual)
            {
                foreach (string filename in Directory.GetFiles(Settings.BaseDirectory))
                {
                    if (filename.ToLower().EndsWith(".xls") || filename.ToLower().EndsWith(".xlsx"))
                    {
                        if (IsCancelFlagSet())
                            return;

                        string language = Strings.Language_DE;
                        if (filename.ToLower().Contains("_e_"))
                            language = Strings.Language_EN;
                        if (ImportManualFromExcelSheetWithLanguage(filename, language))
                        {
                            ++nExcelSheetsImported;
                        }
                        else
                        {
                            ++nExcelSheetsNotImported;

                            // mark this as a manual to be removed

                            Database.ExecuteNonQuery(string.Format("delete from manual where manual_id={0}", ManualID - 1));
                            Database.ExecuteNonQuery(string.Format("delete from manualcontents where manualcontents_manualid={0}", ManualID - 1));

                        }
                    }
                }
            }

            foreach (string base_directory in Directory.GetDirectories(Settings.BaseDirectory))
            {
                if (Path.GetFileName(base_directory).ToUpper().StartsWith("C") ||
                    Path.GetFileName(base_directory).ToUpper().StartsWith("T"))
                {
                    if (UseOnlyOneManual)
                    {
                        if (!Path.GetFileName(base_directory).ToUpper().Contains("C2590"))
                            continue;
                    }
                    foreach (string filename in Directory.GetFiles(base_directory))
                    {
                        if (filename.ToLower().EndsWith(".xls") || filename.ToLower().EndsWith(".xlsx"))
                        {
                            if (IsCancelFlagSet())
                                return;


                            string language = Strings.Language_DE;
                            if (filename.ToLower().Contains("_e_"))
                                language = Strings.Language_EN;
                            if (ImportManualFromExcelSheetWithLanguage(filename, language))
                            {
                                ++nExcelSheetsImported;
                            }
                            else
                            {
                                ++nExcelSheetsNotImported;

                                // mark this as a manual to be removed

                                Database.ExecuteNonQuery(string.Format("delete from manual where manual_id={0}", ManualID - 1));
                                Database.ExecuteNonQuery(string.Format("delete from manualcontents where manualcontents_manualid={0}", ManualID - 1));

                            }
                        }
                    }
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
            if( nExcelSheetsNotImported > 0)
            {
                DisplayCallback.AddWarning(Strings.NumberOfManualsNotImported, nExcelSheetsNotImported);
            }

        }

        private bool ImportManualFromExcelSheetWithLanguage(string excelsheet_filename, string language)
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation(Strings.ImportingManualInLanguage, excelsheet_filename, language);

            try
            {
                PartialManualContent first = ReadPartialManualContent(excelsheet_filename, ref language);
                if( first == null )
                {
                    DisplayCallback.AddError(Strings.ErrorUnableToReadFile, excelsheet_filename);
                    return false;
                }

                Trace.Assert((language == Strings.Language_EN) || (language == Strings.Language_DE));

                DisplayCallback.AddInformation(Strings.ManualHasSoManyParts, first.Filenames.Count);

                Manual m = new Manual();
                m.ID = ManualID++;
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

                Database.Insert(m);

                foreach (string filename in first.Filenames)
                {
                    string key = Tools.KeyFromFilename(filename);

                    long id = 0;
                    try
                    {
                        id = KnownTemplates[key].ID;
                    }
                    catch (KeyNotFoundException)
                    {
                        DisplayCallback.AddError(Strings.ErrorKeyNotFoundInKnownTemplates, key);
                        return false;
                    }


                    ManualContents mc = new ManualContents();
                    mc.ID = ContentID++;
                    mc.ManualID = m.ID;
                    mc.TemplateID = id;
                    Database.Insert(mc);
                }
                return true;
            }
            catch (Exception e)
            {
                DumpException(e, Strings.ExceptionWhileImportingManual, excelsheet_filename);
                return false;
            }
        }

        private PartialManualContent ReadPartialManualContent(string filename, ref string expected_language)
        {
            DisplayCallback.AddInformation(Strings.ReadingExcelFile, filename);
            if( Excel == null )
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
                    if (!manual.ConstructFromExcel(filename, valueArray, KnownTemplates, ref expected_language))
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
            catch(Exception e)
            {
                DumpException(e, Strings.ExceptionWhileImportingManual, filename);
            }
            return manual;
      
        }
    }
}
