using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using manualisator.DBTools;
using manualisator.Attributes;

namespace manualisator.DBSchema
{
    public class Template : TableRow
    {
        [IsTableMemberAttribute, PrimaryKey]
        public long ID { get; set; }

        [IsTableMemberAttribute]
        public string Name { get; set; }

        [IsTableMemberAttribute, NonNull]
        public DateTime LastUpdated { get; set; }
    }
}
