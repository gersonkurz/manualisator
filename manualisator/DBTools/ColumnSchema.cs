using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using manualisator.Attributes;

namespace manualisator.DBTools
{
    /// <summary>
    /// A column in a database schema is described by instances of this class. A column has a name and a type. 
    /// </summary>
    public class ColumnSchema
    {
        /// <summary>
        /// Name of the column
        /// </summary>
        public readonly string DatabaseFieldName;

        /// <summary>
        /// Type of the column
        /// </summary>
        public readonly ColumnType Type;

        /// <summary>
        /// Reference (if Type is ColumnType.ForeignKey);
        /// </summary>
        public readonly string ReferencedTable;
        public readonly string ReferencedColumn;

        /// <summary>
        /// Name of the column
        /// </summary>
        public readonly string InstanceVariableName;

        /// <summary>
        /// Indicates if this column is allowed to be null
        /// </summary>
        public readonly bool IsNullable;

        /// <summary>
        /// Explicit constructor
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <param name="type">Type of the column</param>
        public ColumnSchema(string name, ColumnType type, string instanceVariableName, bool isNullable)
        {
            DatabaseFieldName = name;
            Type = type;
            IsNullable = isNullable;
            InstanceVariableName = instanceVariableName;
        }

        /// <summary>
        /// Explicit constructor for a foreign key
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <param name="type">Type of the column</param>
        public ColumnSchema(string name, string instanceVariableName, Type referredTable, bool isNullable)
        {
            DatabaseFieldName = name;
            IsNullable = isNullable;
            Type = ColumnType.ForeignKey;

            foreach (PropertyInfo info in referredTable.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                PrimaryKeyAttribute isPrimaryKey = info.GetCustomAttribute<PrimaryKeyAttribute>();
                if (isPrimaryKey != null)
                {
                    ReferencedTable = referredTable.Name.ToLower();
                    ReferencedColumn = string.Format("{0}_{1}", ReferencedTable, info.Name.ToLower());
                    break;
                }
            }
            InstanceVariableName = instanceVariableName;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="objectSrc"></param>
        public ColumnSchema(ColumnSchema objectSrc)
        {
            DatabaseFieldName = objectSrc.DatabaseFieldName;
            Type = objectSrc.Type;
            IsNullable = objectSrc.IsNullable;
            ReferencedTable = objectSrc.ReferencedTable;
            ReferencedColumn = objectSrc.ReferencedColumn;
            InstanceVariableName = objectSrc.InstanceVariableName;
        }

        public override string ToString()
        {
            if(ColumnType.ForeignKey == Type)
            {
                return string.Format("<DBField {0} is {1} referencing {2}.{3} (Variable: {4})/>", DatabaseFieldName, Type, ReferencedTable, ReferencedColumn, InstanceVariableName);
            }
            else
            {
                return string.Format("<DBField {0} is {1} (Variable: {2})/>", DatabaseFieldName, Type, InstanceVariableName);
            }
        }
    }
}
