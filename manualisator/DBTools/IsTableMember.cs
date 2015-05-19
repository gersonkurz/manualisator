using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manualisator.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    class IsTableMemberAttribute : Attribute
    {
        public IsTableMemberAttribute()
        {
        }
    }
}
