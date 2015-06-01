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

namespace manualisator.Core
{
    public class FindTextContent : LongRunningTask
    {
        // TODO: should be reusing the RebuildLookupDocument container instead ....
        private Word._Application Word;
        private readonly string FindText;
        
        public FindTextContent(string findText)
        {
            FindText = findText;
        }

        public override bool Initialize(IDisplayCallback displayCallback)
        {
            DisplayCallback = displayCallback;
            return true;
        }

        public override void Run()
        {
            DisplayCallback.AddInformation("");
            DisplayCallback.AddInformation("Gesucht wird der Text \"{0}\"", FindText);

            DateTime now = DateTime.Now;
            if (Word == null)
            {
                DisplayCallback.AddInformation(Strings.NeedToCreateNewInstanceOfWord);
                Word = new Word.Application();
                Word.Visible = false;
            }

            bool failed = false;


            int nLinesOfSkippedText = 0;

            foreach (string filename in Tools.EnumerateDocuments())
            {
                if (IsCancelFlagSet())
                {
                    failed = true;
                    break;
                }
                if (nLinesOfSkippedText++ == 0)
                {
                    DisplayCallback.AddInformation("- {0}", filename);
                }
                else
                {
                    DisplayCallback.AddInformation("^- {0}", filename);
                }

                if (TryToFindTextIn(filename))
                {
                    DisplayCallback.AddInformation("- Gefunden in: '{0}'", filename);
                    nLinesOfSkippedText = 0;
                }
            }

            if (failed)
            {
                DisplayCallback.AddInformation(Strings.FailedAfterTime, DateTime.Now - now);
            }
            else
            {
                DisplayCallback.AddInformation(Strings.SucceededAfterTime, DateTime.Now - now);
            }
        }

        private bool TryToFindTextIn(string filename)
        {
            bool result = false;
            try
            {
                Word._Document currentTemplateDocument = Word.Documents.Open(filename);
                try
                {
                    currentTemplateDocument.Activate();

                    // copy the whole cover letter - including all bookmarks in it
                    object start = currentTemplateDocument.Content.Start;
                    object end = currentTemplateDocument.Content.End;
                    Word.Range rng = currentTemplateDocument.Range(ref start, ref end);
                    if(rng.Find.Execute(FindText: this.FindText))
                    {
                        result = true;
                    }
                }
                finally
                {
                    object so = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    currentTemplateDocument.Close(so);
                    Marshal.ReleaseComObject(currentTemplateDocument);
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        public override void Dispose()
        {
 	        if( Word != null )
            {
                Word.Quit();
                Word = null;
            }
        }
    }
}
