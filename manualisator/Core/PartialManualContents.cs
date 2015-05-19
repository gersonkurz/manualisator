using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace manualisator.Core
{
    class PartialManualContent
    {
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

            if( Language.ToLower().Equals("deutsch") )
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
            else if( Language.ToLower().Equals("englisch") )
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

            if (!machine.Equals(TypeOfManual))
            {
                string temp = string.Format("CINEO {0}", machine);
                if (!temp.Equals(TypeOfManual))
                {
                    Dictionary<string, string> exceptions = new Dictionary<string,string>() {
                        { "C6040CPT", "CINEO C6040 Compact" },
                        { "C6040STD", "CINEO C6040 Standard" },
                    };

                    bool ignoreThis = false;
                    if( exceptions.ContainsKey(machine) )
                    {
                        if(TypeOfManual.Equals(exceptions[machine]))
                        {
                            ignoreThis = true;
                        }
                    }

                    if(!ignoreThis)
                    {
                        DisplayCallback.AddWarning(Strings.WarningMachineDoesNotMatchExpectedMachine,
                           filename, machine, TypeOfManual);
                    }
                }
            }

            if (!ExpectValueIs(values, 6, 1, "Inaktiv (X)") ||
                !ExpectValueIs(values, 6, 2, "PFH"))
            {
                return false;
            }

            bool failed = false;
            for (int nRow = 8, nMaxRow = values.GetLength(0); nRow <= nMaxRow; ++nRow )
            {
                Trace.TraceInformation("reading row {0}", nRow);
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

        private bool ReadRequiredValue(object[,] values, int i, int j, out DateTime actualValue, string fieldName)
        {
            object o = values[i, j];
            actualValue = (DateTime) o;
            if (actualValue == null)
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
