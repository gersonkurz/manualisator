using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Data.Common;

namespace manualisator.DBTools
{
    public class StatementBuilder : IDisposable
    {
        public readonly string TableName;
        private readonly string SkipTableHeader;
        public readonly ParameterList Data;
        public readonly ParameterList Parameters;
        public bool Quiet = false;
        protected readonly DBConnection Database;
        protected string IDClause;

        public StatementBuilder(DBConnection c, string tableName)
        {
            Database = c;
            Data = new ParameterList(c);
            Parameters = new ParameterList(c);
            TableName = tableName;
            SkipTableHeader = string.Format("{0}.", TableName);
        }

        public void Dispose()
        {
        }

        public StatementBuilder(DBConnection database, TableRow row)
            :   this(database, row.Schema.Name)
        {
            foreach(ColumnSchema columnSchema in row.Schema.Columns)
            {
                switch(columnSchema.Type)
                {
                    case ColumnType.PrimaryKey:
                        IDClause = string.Format("{0}_id={1}",
                            row.Schema.Name.ToLower(),
                            row.GetType().GetProperty(columnSchema.InstanceVariableName).GetValue(row));
                        break;

                    case ColumnType.Integer:
                    case ColumnType.String:
                    case ColumnType.Date:
                    case ColumnType.Timestamp:
                    case ColumnType.Bool:
                        Data.Add(columnSchema.DatabaseFieldName, row.GetType().GetProperty(columnSchema.InstanceVariableName).GetValue(row));
                        break;

                    case ColumnType.ForeignKey:
                        {
                            int value = (int) row.GetType().GetProperty(columnSchema.InstanceVariableName).GetValue(row);
                            if( value != 0 )
                            {
                                Data.Add(columnSchema.DatabaseFieldName, value);
                            }
                        }
                        break;

                    default:
                        throw new NotImplementedException(string.Format("Error, unable to build statement for {0}", columnSchema.Type));
                }
            }
        }

        public bool FilterField(ref string fieldName)
        {
            int k = fieldName.IndexOf('.');
            if (k < 0)
                return false;

            if (fieldName.StartsWith(SkipTableHeader))
            {
                fieldName = fieldName.Substring(k + 1);
                return false;
            }
            return true;
        }
        
        public bool Execute()
        {
            ParameterList JoinedList = ParameterList.Merge(Data, Parameters);
            if (JoinedList.Count == 0)
            {
                Trace.TraceInformation("ERROR, no parameters in sql: {0}", ToString());
                return false;
            }
            return Database.ExecuteNonQuery(ToString(), JoinedList, Quiet);
        }

        public long LastRowID
        {
            get
            {
                return (long)Database.ExecuteScalar("select last_insert_rowid()", null);
            }
        }
    }

}
