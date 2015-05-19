using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Globalization;
using System.Xml;
using System.IO;

namespace manualisator.Core
{
    public static class Tools
    {
        public static bool IsSpecialTemplate(string name)
        {
            return name.StartsWith("TOC_", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("TOF_", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("UMSCH_", StringComparison.OrdinalIgnoreCase);
        }

        public static string KeyFromFilename(string filename)
        {
            string key = filename.ToLower();
            int k = key.LastIndexOf('.');
            if( k > 0)
                key = key.Substring(0, k-1);
            return key;
        }


        public static string DumpException(Exception e, string format, params object[] args)
        {
            StringBuilder output = new StringBuilder();
            output.AppendFormat("--- EXCEPTION CAUGHT: {0} ---\r\n", e.Message);
            output.AppendFormat("CONTEXT: {0}\r\n", string.Format(format, args));
            output.AppendFormat("SOURCE: {0}\r\n", e.Source);
            output.AppendLine(e.StackTrace);
            string text = output.ToString();
            Trace.TraceError(text);
            return text;
        }

        public static string Soundex(string word)
        {
            const string soundexAlphabet = "0123012#02245501262301#202";
            string soundexString = "";
            char lastSoundexChar = '?';
            word = word.ToUpper();

            foreach (var c in from ch in word
                              where ch >= 'A' &&
                                    ch <= 'Z' &&
                                    soundexString.Length < 4
                              select ch)
            {
                char thisSoundexChar = soundexAlphabet[c - 'A'];

                if (soundexString.Length == 0)
                    soundexString += c;
                else if (thisSoundexChar == '#')
                    continue;
                else if (thisSoundexChar != '0' &&
                         thisSoundexChar != lastSoundexChar)
                    soundexString += thisSoundexChar;

                lastSoundexChar = thisSoundexChar;
            }

            return soundexString.PadRight(4, '0');
        }

        public static IEnumerable<string> EnumerateDocuments()
        {
            string[] folders = Program.Settings.FilesDirectory.Split(';', ',');
            string baseDirectory = Program.Settings.BaseDirectory;

            Trace.TraceInformation("EnumerateDocuments(baseDirectory: {0}, filesDirectory: {1})", baseDirectory, Program.Settings.FilesDirectory);
            foreach (string folderName in folders)
            {
                string folderPath = Path.Combine(baseDirectory, folderName);
                if (Directory.Exists(folderPath))
                {
                    foreach (string filename in Directory.GetFiles(folderPath))
                    {
                        Trace.TraceInformation("EnumerateDocuments() yields '{0}'", filename);
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
                if (File.Exists(pathName))
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
            if(Program.Settings.UseFilenameTemplate)
            {
                return Path.Combine(
                    Program.Settings.BaseDirectory,
                    Program.Settings.ManualsDirectory,
                    string.Format(Program.Settings.FilenameTemplate, m.Name, m.Device, m.Language));
            }
            string targetFilename = m.TargetFilename;
            if(targetFilename.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
            {
                targetFilename += "x";
            }
            else if(!targetFilename.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                targetFilename += ".docx";
            }

            return Path.Combine(
                Program.Settings.BaseDirectory,
                Program.Settings.ManualsDirectory,
                targetFilename);
                
        }
    }
}
