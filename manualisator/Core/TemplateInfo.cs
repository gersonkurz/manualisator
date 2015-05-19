using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manualisator.Core
{
    public class TemplateInfo
    {
        public readonly long ID;
        public readonly string Name;

        public TemplateInfo(long id, string name)
        {
            Name = name;
            ID = id;
        }
    }
}
