using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;
using manualisator.Attributes;
using log4net;

namespace manualisator.DBTools
{
    public class SelectStatement : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public DbDataReader DataReader;
        public readonly DBConnection Database;
        private int RowNumber;
        public static int DefaultInt = 0;
        public static long DefaultLong = 0;
        public static double DefaultDouble = 0;

        private Dictionary<string, int> _ColumnNameDictionary = null;

        public static SelectStatement Select<T>(
                DBConnection c, 
                string whereClause = null, 
                IEnumerable<DbParameter> parameters = null)
            where T: TableRow, new()
        {
            StringBuilder statement = new StringBuilder();
            statement.Append("select ");
            bool first = true;
            T row = new T();
            foreach(ColumnSchema cs in row.Schema.Columns)
            {
                if( first )
                {
                    first = false;
                }
                else
                {
                    statement.Append(",");
                }
                statement.Append(cs.DatabaseFieldName);
            }
            statement.Append(" from ");
            statement.Append(row.GetType().Name);
            if (!string.IsNullOrEmpty(whereClause) )
            {
                statement.Append(" ");
                statement.Append(whereClause);                
            }
            statement.Append(";");

            return new SelectStatement(c, statement.ToString(), parameters);
        }
        
        public SelectStatement(DBConnection c, string statement, IEnumerable<DbParameter> parameters)
        {
            Database = c;
            RowNumber = 0;
            Log.InfoFormat("SQL: {0}", statement);
            DbCommand command = c.CreateCommand(statement);
            if (parameters != null)
            {
                foreach (DbParameter p in parameters)
                {
                    command.Parameters.Add(p);
                }
            }
            DataReader = command.ExecuteReader();
        }
        
        public bool Next()
        {
            if (DataReader != null)
            {
                if (DataReader.Read())
                {
                    ++RowNumber;
                    return true;
                }
                DataReader.Close();
                DataReader = null;
            }
            return false;
        }

        public void Dispose()
        {
            if (DataReader != null)
            {
                DataReader.Close();
                DataReader = null;
            }
        }

        public object[] AllItemsAsObjects()
        {
            object[] result = new object[DataReader.FieldCount];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = GetValue(i);
            }
            return result;
        }


        public string[] AllItemsAsText()
        {
            string[] result = new string[DataReader.FieldCount];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = AsText(i);
            }
            return result;
        }

        public string[] AllItemsAsText(string[] AdditionalNames)
        {
            int n = DataReader.FieldCount, i;
            string[] result = new string[n + AdditionalNames.Length];
            for (i = 0; i < n; ++i)
            {
                result[i] = AsText(i);
            }
            while (i < result.Length)
            {
                result[i++] = "";
            }

            return result;
        }

        private Dictionary<string,int> ColumnNameDictionary
        {
            get
            {
                if( _ColumnNameDictionary == null )
                {
                    _ColumnNameDictionary = new Dictionary<string, int>();
                    for (int i = 0, n = DataReader.FieldCount; i < n; ++i)
                    {
                        _ColumnNameDictionary[DataReader.GetName(i)] = i;
                    }
                }
                return _ColumnNameDictionary;
            }
        }

        public object GetValue(int index)
        {
            object result = DataReader[index];
            if (result is DBNull)
                result = null;
            return result;
        }

        public static string ValueAsText(object value, DbType TypeHint)
        {
            try
            {
                if (value == null)
                    return "";

                if (value is DBNull)
                    return "";

                if (value is string)
                    return value as string;

                if (value is double)
                    return string.Format("{0:#,##0.00}", (double)value);

                if (value is int || value is long)
                    return string.Format("{0}", value);

                if (value is DateTime)
                {
                    DateTime v = (DateTime)value;

                    if (TypeHint == DbType.Time)
                        return v.ToLongTimeString();

                    return v.ToShortDateString();
                }

                return "";
            }
            catch (Exception e)
            {
                manualisator.Core.Tools.DumpException(e, "ValueAsText: {0} [{1}]", value, value.GetType());
                return "";
            }
        }

        public string AsText(int index)
        {
            return ValueAsText(DataReader[index], DbType.Object);
        }

        public string AsText(int index, DbType TypeHint)
        {
            return ValueAsText(DataReader[index], TypeHint);
        }

        public int AsInt(int index)
        {
            try
            {
                object value = DataReader[index];
                if ((value == null) || (value is DBNull))
                    return DefaultInt;
                if (value is long)
                    return (int)(long)value;
                return (int)value;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public long AsLong(int index)
        {
            try
            {
                object value = DataReader[index];
                if ((value == null) || (value is DBNull))
                    return DefaultLong;
                if (value is int)
                    return (long)(int)value;
                return (long)value;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public double AsDouble(int index)
        {
            try
            {
                object value = DataReader[index];
                if ((value == null) || (value is DBNull))
                    return DefaultDouble;
                if (value is int)
                    return (double)(int)value;
                if (value is long)
                    return (double)(long)value;
                return (double)value;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public DateTime AsDateTime(int index)
        {
            try
            {
                object value = DataReader[index];
                if (!(value == null) && (value is DateTime))
                    return (DateTime)value;
            }
            catch (Exception)
            {
            }
            return DateTime.Now;
        }


        public bool AsBool(int index, bool defaultValue)
        {
            try
            {
                object value = DataReader[index];
                if (!(value == null) && (value is bool))
                    return (bool)value;
            }
            catch (Exception)
            {
            }
            return defaultValue;
        }

        public bool IsNull(int index)
        {
            try
            {
                object value = DataReader[index];
                return (value is DBNull) || (value == null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public T Get<T>(int index, T defaultValue)
        {
            try
            {
                object value = DataReader[index];
                if (value is DBNull)
                    return defaultValue;
                return (T)value;
            }
            catch (Exception e)
            {
                manualisator.Core.Tools.DumpException(e, "Get: {0}, {1}", index, defaultValue.GetType());
                return defaultValue;
            }
        }

        public void ReadTableRow(TableRow row)
        {
            Type classType = row.GetType();
            // enumerate all properties in this item
            foreach (PropertyInfo info in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // check if this property is a table item. 
                IsTableMemberAttribute isTableMember = info.GetCustomAttribute<IsTableMemberAttribute>();
                if (isTableMember == null)
                {
                    // no, but maybe this is not a table member, but a table itself: if so, read recursive
                    if( info.PropertyType.IsSubclassOf(typeof(TableRow)))
                    {
                        object o = info.GetValue(row);
                        TableRow tr = o as TableRow;
                        ReadTableRow(tr);
                    }
                }
                else
                {
                    string fieldName = string.Format("{0}_{1}", classType.Name, info.Name).ToLower();
                    int fieldIndex = 0;
                    if ( ColumnNameDictionary.TryGetValue(fieldName, out fieldIndex) )
                    {
                        object item = DataReader[fieldIndex];

                        if( item == null )
                        {
                            // do nothing
                        }
                        else if (item is DBNull)
                        {
                            // do very little :)
                            item = null;
                        }
                        
                        info.SetValue(row, item);
                    }
                    SetPropertyValueOnTableRow(row, info);
                }
            }
        }

        private void SetPropertyValueOnTableRow(TableRow row, PropertyInfo info)
        {
            
            /*
        }
                   

            string fieldType = info.PropertyType.Name;
            if (cs.Type == ColumnType.PrimaryKey)
            {
                info.SetValue(row, AsInt(index));
            }
            else if (info.PropertyType.IsEnum)
            {
                info.SetValue(row, AsInt(index));
            }
            else if (info.PropertyType.Equals(typeof(int)))
            {
                info.SetValue(row, AsInt(index));
            }
            else if (info.PropertyType.Equals(typeof(long)))
            {
                info.SetValue(row, AsLong(index));
            }
            else if (info.PropertyType.Equals(typeof(string)))
            {
                info.SetValue(row, AsText(index));
            }
            else if (info.PropertyType.Equals(typeof(bool)))
            {
                info.SetValue(row, AsBool(index, false));
            }
            else if (info.PropertyType.Equals(typeof(DateTime)))
            {
                info.SetValue(row, AsDateTime(index));
            }
            else if (info.PropertyType.Equals(typeof(Date)))
            {
                info.SetValue(row, AsDate(index));
            }
            else if (info.PropertyType.Equals(typeof(Type)))
            {
                // ignore
            }
            else
            {
                Trace.TraceWarning("Don't know type {0} for {1}, unable to create TableSchema mapping for this...",
                    fieldType, info.Name);
            }
            ++index;
            }*/
        }

        public DbDataReader Row
        {
            get
            {
                return DataReader;
            }
        }
    }
}
