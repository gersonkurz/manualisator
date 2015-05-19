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
                string result = Program.Settings.UseLanguageSpecificBookmarks;
                return result.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        result.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                        result.Equals("1", StringComparison.OrdinalIgnoreCase);
            }
        }
                
        public static IEnumerable<string> EnumerateDocuments()
        {
            string[] folders = Program.Settings.FilesDirectory.Split(';', ',');
            string baseDirectory = Program.Settings.BaseDirectory;

            Trace.TraceInformation("EnumerateDocuments(baseDirectory: {0}, filesDirectory: {1})", baseDirectory, Program.Settings.FilesDirectory);
            foreach(string folderName in folders)
            {
                string folderPath = Path.Combine(baseDirectory, folderName);
                if (Directory.Exists(folderPath))
                {
                    foreach (string filename in Directory.GetFiles(folderPath))
                    {
                        Trace.TraceInformation("EnumerateDocuments() yields '%s'", filename);
                        yield return filename;
                    }
                }
            }
        }
        
        public static string GetDocumentFilename(string fileName)
        {
            string[] folders = Program.Settings.FilesDirectory.Split(';', ',');
            string baseDirectory = Program.Settings.BaseDirectory;

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
                    Program.Settings.BaseDirectory,
                    Program.Settings.ManualsDirectory, 
                    Program.Settings.LookupDocumentFilename);
            }
        }

        public static string ManualsDatabasePathname
        {
            get
            {
                return Path.Combine(
                    Program.Settings.BaseDirectory, 
                    Program.Settings.ManualsDatabaseFilename);
            }
        }

        public static string GetTargetFilename(DBSchema.Manual m)
        {
            return Path.Combine(
                Program.Settings.BaseDirectory,
                Program.Settings.ManualsDirectory, 
                string.Format(Program.Settings.FilenameTemplate, m.Name, m.Device, m.Language));
        }

    }
}
