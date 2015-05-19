using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using manualisator.DBTools;
using manualisator.Attributes;

namespace manualisator.DBSchema
{
    public class ManualContents : TableRow
    {
        [IsTableMemberAttribute, PrimaryKey]
        public long ID { get; set; }

        [IsTableMemberAttribute, ForeignKey(typeof(Manual))]
        public long ManualID { get; set; }

        [IsTableMemberAttribute, ForeignKey(typeof(Template))]
        public long TemplateID { get; set; }
    }
}
