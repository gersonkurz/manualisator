using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace manualisator.DBTools
{
    public class SQLiteDatabase : DBConnection
    {
        public SQLiteDatabase(string connectionString)
            :   base(new SQLiteConnection(connectionString))
        {
        }

        public override DbTransaction CreateTransaction()
        {
            return Connection.BeginTransaction();
        }

        public override DbCommand CreateCommand(string statement)
        {
            return new SQLiteCommand(statement, this.Connection as SQLiteConnection);
        }

        public override DbParameter CreateParameter()
        {
            return new SQLiteParameter();
        }

        public override string BuildNamedParameter(string parameterName)
        {
            return "?";
        }

        /// <summary>
        /// Check if a table exists. SQLite does this by querying the master table for the table name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override bool DoesTableExist(string name)
        {
            return !IsQueryEmpty(string.Format("select name from sqlite_master where lower(name)='{0}'", name.ToLower()));
        }

        public override string GetForeignKeyClause(ColumnSchema column)
        {
            return string.Format("foreign key({0}) references {1}({2})",
                column.DatabaseFieldName,
                column.ReferencedTable,
                column.ReferencedColumn);
        }

        public override string GetNativeSqlType(ColumnType type)
        {
            switch (type)
            {
                case ColumnType.PrimaryKey:
                    return "integer primary key autoincrement";

                case ColumnType.ForeignKey:
                    return "integer";

                case ColumnType.Integer:
                    return "integer";

                case ColumnType.String:
                    return "text";

                case ColumnType.Bool:
                    return "bool";

                case ColumnType.Timestamp:
                    return "timestamp";

                case ColumnType.Date:
                    return "date";

                case ColumnType.Double:
                    return "double";
                
                default:
                    return null;
            }
        }
    }
}
