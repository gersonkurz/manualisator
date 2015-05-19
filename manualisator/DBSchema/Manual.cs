using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using manualisator.DBTools;
using manualisator.Attributes;

namespace manualisator.DBSchema
{
    public class Manual : TableRow
    {
        [IsTableMemberAttribute, PrimaryKey]
        public long ID { get; set; }

        [IsTableMemberAttribute, NonNull]
        public string Name { get; set; }

        [IsTableMemberAttribute, NonNull]
        public string Device { get; set; }

        [IsTableMemberAttribute, NonNull]
        public string Language { get; set; }

        [IsTableMemberAttribute, NonNull]
        public DateTime LastUpdated { get; set; }

        [IsTableMemberAttribute, NonNull]
        public DateTime LastGenerated { get; set; }

        [IsTableMemberAttribute]
        public string Title1 { get; set; }

        [IsTableMemberAttribute]
        public string Title2 { get; set; }

        [IsTableMemberAttribute]
        public string Title3 { get; set; }

        [IsTableMemberAttribute]
        public string Version { get; set; }

        [IsTableMemberAttribute]
        public string TypeOfManual { get; set; }

        [IsTableMemberAttribute]
        public string OrderNr { get; set; }

        [IsTableMemberAttribute]
        public string Template { get; set; }

        [IsTableMemberAttribute]
        public string TargetFilename { get; set; }
    }
}
