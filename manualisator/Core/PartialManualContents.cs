using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Reflection;

namespace manualisator.Core
{
    class PartialManualContent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType); 
        public string OriginalTemplateFilename;
        private readonly IDisplayCallback DisplayCallback;
        private string CurrentFilename;
        
        public DateTime Revision;        // Stand
        public string Title1;          // Titel 1
        public string Title2;          // Title 2
        public string Version;         // Version
        public string Title3;          // Title 3
        public string TypeOfManual;    // HB-Typ
        public string OrderNr;         // Bestell-Nr
        public string Language;        // Sprache
        public string Template;        // Vorlage
        public string TargetFilename;  // Zieldatei
        public string Special;         // Spezial
        public readonly List<string> Filenames = new List<string>();
        public readonly List<string> Bookmarks = new List<string>();

        public PartialManualContent(IDisplayCallback displayCallback)
        {
            DisplayCallback = displayCallback;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
                Revision, Title1, Title2, Version, Title3, TypeOfManual, OrderNr, Template, TargetFilename, Special);
            return result.ToString();
        }

        internal bool ConstructFromExcel(string filename, object[,] values, Dictionary<string, TemplateInfo> knownTemplates, ref string expectedLanguage)
        {
            CurrentFilename = filename;
            string[] tokens = filename.Split('\\');
            int k = tokens.Length;
            OriginalTemplateFilename = tokens[k - 1];
            string machine = tokens[k - 2];

            // check that the first row follows standard defintitons. if one of those values fails, 
            if (!ExpectValueIs(values, 1, 1, "OS") ||
                !ExpectValueIs(values, 1, 2, "Stand") ||
                !ExpectValueIs(values, 1, 3, "Titel 1") ||
                !ExpectValueIs(values, 1, 4, "Titel 2") ||
                !ExpectValueIs(values, 1, 5, "Version") ||
                !ExpectValueIs(values, 1, 6, "Titel 3") ||
                !ExpectValueIs(values, 1, 7, "HB-Typ") ||
                !ExpectValueIs(values, 1, 8, "Bestell-Nr") ||
                !ExpectValueIs(values, 1, 9, "Sprache") ||
                !ExpectValueIs(values, 1, 10, "Vorlage") ||
                !ExpectValueIs(values, 1, 11, "Zieldatei") ||
                !ExpectValueIs(values, 1, 12, "Spezial"))
            {
                return false;
            }

            if (!ReadRequiredValue(values, 2, 2, out Revision, "Revision") ||
                !ReadRequiredValue(values, 2, 3, out Title1, "Title1") ||
                !ReadOptionalValue(values, 2, 4, out Title2) ||
                !ReadOptionalValue(values, 2, 5, out Version) ||
                !ReadOptionalValue(values, 2, 6, out Title3) ||
                !ReadRequiredValue(values, 2, 7, out TypeOfManual, "TypeOfManual") ||
                !ReadRequiredValue(values, 2, 8, out OrderNr, "OrderNr") ||
                !ReadRequiredValue(values, 2, 9, out Language, "Language") ||
                !ReadRequiredValue(values, 2, 10, out Template, "Template") ||
                !ReadRequiredValue(values, 2, 11, out TargetFilename, "TargetFilename") ||
                !ReadOptionalValue(values, 2, 12, out Special))
            {
                return false;
            }

            for (int i = 2; i < 60; ++i )
            {
                string bookmarkName = "";
                if (!ReadStringValue(values, 3, i, ref bookmarkName))
                    break;

                Log.InfoFormat("bookmarkName: {0}", bookmarkName);
                Bookmarks.Add(bookmarkName);
            }

            if (Language.ToLower().Equals("deutsch"))
            {
                if ((expectedLanguage != Strings.Language_DE) && !string.IsNullOrEmpty(expectedLanguage))
                {
                    DisplayCallback.AddWarning(Strings.WarningLanguageDoesNotMatchExpectedLanguage,
                        filename, Language, expectedLanguage);
                }
                Language = Strings.Language_DE;
                expectedLanguage = Language;
                DisplayCallback.AddInformation(Strings.LanguageResetToThis, Language);
            }
            else if (Language.ToLower().Equals("englisch"))
            {
                if ((expectedLanguage != Strings.Language_EN) && !string.IsNullOrEmpty(expectedLanguage))
                {
                    DisplayCallback.AddWarning(Strings.WarningLanguageDoesNotMatchExpectedLanguage,
                        filename, Language, expectedLanguage);
                }
                Language = Strings.Language_EN;
                expectedLanguage = Strings.Language_EN;
                DisplayCallback.AddInformation(Strings.LanguageResetToThis, Language);
            }
            else
            {
                DisplayCallback.AddError(Strings.ErrorLanguageIsUnknown,
                    filename, Language);
                return false;
            }

            if (!ExpectValueIs(values, 6, 1, "Inaktiv (X)") ||
                !ExpectValueIs(values, 6, 2, "PFH"))
            {
                return false;
            }

            bool failed = false;
            for (int nRow = 8, nMaxRow = values.GetLength(0); nRow <= nMaxRow; ++nRow )
            {
                Log.InfoFormat("reading row {0}", nRow);
                string is_used = values[nRow, 1] as string;
                if (string.IsNullOrEmpty(is_used))
                {
                    object o = values[nRow, 2];
                    if( o != null )
                    {
                        string template_filename = o as string;

                        string key = Tools.KeyFromFilename(template_filename);

                        if (string.IsNullOrEmpty(template_filename))
                        {
                            DisplayCallback.AddError(Strings.ErrorTemplateSpecIsInvalid, filename, values[nRow, 2].GetType());
                            return false;
                        }
                        else if ((knownTemplates == null) || knownTemplates.ContainsKey(key))
                        {
                            Filenames.Add(template_filename);
                        }
                        else if (knownTemplates != null)
                        {
                            DisplayCallback.AddError(Strings.ErrorTemplateNameIsInvalid, filename, template_filename);

                            string key_sx = Tools.Soundex(key);
                            foreach(string existingKey in knownTemplates.Keys)
                            {
                                if(Tools.Soundex(existingKey) == key_sx)
                                {
                                    DisplayCallback.AddWarning(Strings.DidYouMean, knownTemplates[existingKey].Name);
                                }
                            }
                            failed = true;
                        }
                    }

                }
            }
            return !failed;
        }

        private bool ExpectValueIs(object[,] values, int i, int j, string expectedValue)
        {
            string actualValue = values[i,j] as string;
            if(actualValue!=expectedValue)
            {
                DisplayCallback.AddError(Strings.ErrorValueXYShouldBeZButIsNot,
                    CurrentFilename,
                    i, j, expectedValue, actualValue);
                return false;
            }
            return true;
        }

        private bool ReadRequiredValue(object[,] values, int i, int j, out string actualValue, string fieldName)
        {
            object o = values[i, j];
            actualValue = o as string;
            if (string.IsNullOrEmpty(actualValue))
            {
                DisplayCallback.AddError(Strings.ErrorRequiredValueXYMissing,
                    CurrentFilename,
                    i, j, (o == null) ? "null" : o.GetType().ToString(),
                    fieldName);
                return false;
            }
            return true;
        }

        private bool ReadStringValue(object[,] values, int i, int j, ref string actualValue)
        {
            object o = values[i, j];
            actualValue = o as string;
            if (string.IsNullOrEmpty(actualValue))
            {
                return false;
            }
            return true;
        }


        private bool ReadRequiredValue(object[,] values, int i, int j, out DateTime actualValue, string fieldName)
        {
            object o = values[i, j];

            bool success = false;
            actualValue = DateTime.Now;
            if( o is string )
            {
                string[] tokens = (o as string).Split('.');
                if( tokens.Length == 3 )
                {
                    actualValue = new DateTime(year: int.Parse(tokens[2]), month: int.Parse(tokens[1]), day: int.Parse(tokens[0]));
                    success = true;
                }
            }
            else if( o is DateTime )
            {
                actualValue = (DateTime)o;
                success = true;
            }
            
            if (!success)
            {
                DisplayCallback.AddError(Strings.ErrorRequiredValueXYMissing,
                    CurrentFilename,
                    i, j, (o == null) ? "null" : o.GetType().ToString(),
                    fieldName);
                return false;
            }
            return true;
        }

        private bool ReadOptionalValue(object[,] values, int i, int j, out string actualValue)
        {
            actualValue = values[i, j] as string;
            return true;
        }
    }
}
