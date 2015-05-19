using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace manualisator.Core
{
    public static class Settings
    {
        public static bool UseLanguageSpecificBookmarks
        {
            get
            {
                string result = GetStringValue("UseLanguageSpecificBookmarks", "true");
                return result.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        result.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                        result.Equals("1", StringComparison.OrdinalIgnoreCase);
            }
        }


        public static string BaseDirectory
        {
            get
            {
                return GetStringValue("BaseDirectory", ".");
            }
        }

        public static string TemplateFilename_DE
        {
            get
            {
                return GetStringValue("TemplateFilename_DE", "template_de.dotx");
            }
        }

        public static string TemplateFilename_EN
        {
            get
            {
                return GetStringValue("TemplateFilename_EN", "template_en.dotx");
            }
        }

        public static string ManualsDirectory
        {
            get
            {
                return GetStringValue("ManualsDirectory", "Handbuecher");
            }
        }

        public static string TemplatesDirectory
        {
            get
            {
                return GetStringValue("TemplatesDirectory", "Vorlagen");
            }
        }
        
        public static IEnumerable<string> EnumerateDocuments()
        {
            string[] folders = GetStringValue("FilesDirectory", "Dateien").Split(';');
            string baseDirectory = Settings.BaseDirectory;

            Trace.TraceInformation("EnumerateDocuments(baseDirectory: {0}, filesDirectory: {1})");
            foreach(string folderName in folders)
            {
                foreach (string filename in Directory.GetFiles(Path.Combine(baseDirectory, folderName)))
                {
                    yield return filename;
                }
            }
        }
        
        public static string GetDocumentFilename(string fileName)
        {
            string[] folders = GetStringValue("FilesDirectory", "Dateien").Split(';');
            string baseDirectory = Settings.BaseDirectory;

            foreach (string folderName in folders)
            {
                string pathName = Path.Combine(baseDirectory, folderName, fileName);
                if(File.Exists(pathName))
                {
                    Trace.TraceInformation("{0} exists", pathName);
                    return pathName;
                }
                else
                {
                    Trace.TraceWarning("{0} does not exist", pathName);
                }
            }
            Trace.TraceError("Document named '{0}' not found in any of these folders: {1}", fileName, folders);
            return null;
        }
        
        public static string LookupDocumentPathname
        {
            get
            {
                return Path.Combine(
                    Settings.BaseDirectory, 
                    Settings.ManualsDirectory, 
                    GetStringValue("LookupDocumentFilename", "lookup.docx"));
            }
        }

        public static string ManualsDatabasePathname
        {
            get
            {
                return Path.Combine(
                    Settings.BaseDirectory, 
                    GetStringValue("ManualsDatabaseFilename", "manuals.db"));
            }
        }

        public static string GetTargetFilename(DBSchema.Manual m)
        {
            return Path.Combine(
                Settings.BaseDirectory, 
                Settings.ManualsDirectory, 
                string.Format(GetStringValue("FilenameTemplate", "{0}_{1}_{2}.docx"), m.Name, m.Device, m.Language));
        }

        private static string GetStringValue(string name, string defaultValue)
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey("Software\\p-nand-q.com\\WNIMAN", writable: false);
                try
                {
                    object existing = key.GetValue(name);
                    if (existing != null)
                        defaultValue = existing as string;
                }
                finally
                {
                    key.Close();
                }
            }
            catch(Exception)
            {
            }
            return defaultValue;
        }
    }
}
