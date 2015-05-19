using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manualisator.Core
{
    public interface IDisplayCallback
    {
        void AddInformation(string msg, params object[] args);
        void AddWarning(string msg, params object[] args);
        void AddError(string msg, params object[] args);
        void AddInformation(string msg);
        void AddWarning(string msg);
        void AddError(string msg);
        void HasBeenAborted();
    }
}
