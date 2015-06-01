using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using log4net;
using System.Reflection;

namespace manualisator.DBTools
{
    public class ParameterList : List<DbParameter>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public readonly DBConnection Database;

        public ParameterList(DBConnection database)
        {
            Database = database;
        }

        public static ParameterList Create(DBConnection c, params object[] array)
        {
            ParameterList result = new ParameterList(c);
            for (int i = 0; i < array.Length/2; ++i)
            {
                string name = array[i * 2] as string;
                object value = array[i * 2 + 1];
                result.Add(name, value);
            }
            return result;
        }

        public static ParameterList Merge(ParameterList A, ParameterList B)
        {
            ParameterList result = new ParameterList(A.Database);
            result.AddItemsFrom(A);
            result.AddItemsFrom(B);
            return result;
        }

        public void AddItemsFrom(IEnumerable<DbParameter> source)
        {
            foreach (DbParameter p in source)
            {
                Add(p);
            }
        }

        public bool Add(string name, object value)
        {
            return Add(name, value, null);
        }

        public bool Add(string name, object value, object defaultValue)
        {
            DbParameter np = Database.CreateParameter();
            np.ParameterName = name;
            np.SourceColumn = name;
            np.Value = value;

            if ((value == null) || (value is DBNull))
                value = defaultValue;

            if (value is Double)
            {
                np.DbType = DbType.Double;
            }
            else if (value is long)
            {
                np.DbType = DbType.Int64;
            }
            else if (value is int)
            {
                np.DbType = DbType.Int32;
            }
            else if (value is bool)
            {
                np.DbType = DbType.Boolean;
            }
            else if (value is DateTime)
            {
                np.DbType = DbType.DateTime;
            }
            else if (value is string)
            {
                np.DbType = DbType.String;
            }
            else if(value is Enum)
            {
                np.DbType = DbType.Int32;
                np.Value = (int)value;
            }
            else if (value != null)
            {
                Log.InfoFormat("ERROR in ParameterList.Add({0}, {1}): unknown type mapping", name, value);
                return false;
            }
            Add(np);
            return true;
        }        
    }

}
