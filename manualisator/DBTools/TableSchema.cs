using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using manualisator.Attributes;
using log4net;

namespace manualisator.DBTools
{
    public class TableSchema
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public readonly string Name;
        public readonly ColumnSchema[] Columns;
        public readonly ColumnSchema PrimaryKey;

        public TableSchema(string name, ColumnSchema primaryKey, params ColumnSchema[] columns)
        {
            Name = name;
            PrimaryKey = primaryKey;
            Columns = columns;
        }

        public static TableSchema FromClassType(Type classType)
        {
            List<ColumnSchema> columns = new List<ColumnSchema>();
            ColumnSchema primaryKey = null;

            PropertyInfo[] fi = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            while( true )
            {
                IsDatabaseViewAttribute isDatabaseView = classType.GetCustomAttribute<IsDatabaseViewAttribute>();
                if (isDatabaseView != null)
                {
                    classType = classType.BaseType;
                }
                else break;
            }

            foreach (PropertyInfo info in fi)
            {
                IsTableMemberAttribute isTableMember = info.GetCustomAttribute<IsTableMemberAttribute>();
                if (isTableMember == null)
                    continue;

                string fieldName = string.Format("{0}_{1}", classType.Name, info.Name).ToLower();

                PrimaryKeyAttribute isPrimaryKey = info.GetCustomAttribute<PrimaryKeyAttribute>();

                bool isNullable = true;
                NonNullAttribute nonNull = info.GetCustomAttribute<NonNullAttribute>();
                if(nonNull != null)
                {
                    isNullable = false;
                }

                ForeignKeyAttribute foreignKey = info.GetCustomAttribute<ForeignKeyAttribute>();
                if( isPrimaryKey != null )
                {
                    primaryKey = new ColumnSchema(fieldName, ColumnType.PrimaryKey, info.Name, isNullable);
                    columns.Add(primaryKey);
                }
                else if( info.PropertyType.IsEnum )
                {
                    columns.Add(new ColumnSchema(fieldName, ColumnType.Integer, info.Name, isNullable));
                }
                else if (info.PropertyType.Equals(typeof(int)))
                {
                    if (foreignKey != null)
                    {
                        columns.Add(new ColumnSchema(fieldName, info.Name, foreignKey.ReferredTable, isNullable));
                    }
                    else
                    {
                        columns.Add(new ColumnSchema(fieldName, ColumnType.Integer, info.Name, isNullable));
                    }
                }
                else if (info.PropertyType.Equals(typeof(long)))
                {
                    columns.Add(new ColumnSchema(fieldName, ColumnType.Integer, info.Name, isNullable));
                }
                else if (info.PropertyType.Equals(typeof(string)))
                {
                    columns.Add(new ColumnSchema(fieldName, ColumnType.String, info.Name, isNullable));
                }
                else if (info.PropertyType.Equals(typeof(bool)))
                {
                    columns.Add(new ColumnSchema(fieldName, ColumnType.Bool, info.Name, isNullable));
                }
                else if (info.PropertyType.Equals(typeof(DateTime)))
                {
                    columns.Add(new ColumnSchema(fieldName, ColumnType.Timestamp, info.Name, isNullable));
                }
                else if (info.PropertyType.Equals(typeof(Type)))
                {
                    // ignore
                }
                else
                {
                    Log.WarnFormat("Don't know type {0} for {1}, unable to create TableSchema mapping for this...",
                        info.PropertyType, info.Name);
                }
            }

            return new TableSchema(classType.Name.ToLower(), primaryKey, columns.ToArray());
        }

        public static TableSchema ByName(TableSchema[] Schemas, string name)
        {
            foreach (TableSchema ts in Schemas)
            {
                if (ts.Name.Equals(name))
                {
                    return ts;
                }
            }
            return null;
        }

        public string CreateExplicitSelectStatement(string where_clause)
        {
            StringBuilder result = new StringBuilder();
            result.Append("SELECT ");
            bool first = true;
            foreach (ColumnSchema cs in Columns)
            {
                if (first)
                    first = false;
                else
                    result.Append(",");
                result.Append(cs.DatabaseFieldName);
            }
            result.Append(" FROM ");
            result.Append(Name);
            if (!string.IsNullOrEmpty(where_clause))
            {
                result.Append(" ");
                result.Append(where_clause);
            }
            result.Append(";");
            return result.ToString();
        }

    }
}
