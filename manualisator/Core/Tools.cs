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
    }
}
