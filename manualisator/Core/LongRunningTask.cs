using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manualisator.Core
{
    public abstract class LongRunningTask : IDisposable
    {
        protected IDisplayCallback DisplayCallback;
        private bool CancelFlag;

        protected bool IsCancelFlagSet()
        {
            lock(DisplayCallback)
            {
                if (!CancelFlag)
                    return false;

                DisplayCallback.AddWarning("Warning: operation cancelled.");
                DisplayCallback.HasBeenAborted();
                return true;
            }
        }

        public void SetCancelFlag()
        {
            lock(DisplayCallback)
            {
                CancelFlag = true;
            }
        }

        public LongRunningTask()
        {
            CancelFlag = false;
        }

        public virtual bool Initialize(IDisplayCallback displayCallback)
        {
            DisplayCallback = displayCallback;
            return true;
        }

        public abstract void Run();

        public string DumpException(Exception e, string format, params object[] args)
        {
            StringBuilder output = new StringBuilder();
            output.AppendFormat("--- EXCEPTION CAUGHT: {0} ---\r\n", e.Message);
            output.AppendFormat("CONTEXT: {0}\r\n", string.Format(format, args));
            output.AppendFormat("SOURCE: {0}\r\n", e.Source);
            output.AppendLine(e.StackTrace);
            string text = output.ToString();
            DisplayCallback.AddError(text);
            return text;
        }

        public abstract void Dispose();
    }
}
