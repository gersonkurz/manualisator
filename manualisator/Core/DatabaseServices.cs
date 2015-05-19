using System;
using System.Collections.Generic;
using manualisator.DBTools;
using manualisator.DBSchema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace manualisator.Core
{
    public class DatabaseServices
    {
        public readonly SQLiteDatabase Database;

        public DatabaseServices(string filename)
        {
            string connectionString = string.Format("Data Source={0}", filename);
            Trace.TraceInformation("connectionString: {0}", connectionString);
            Database = new SQLiteDatabase(connectionString);

            ListOfSchemas = new TableSchema[] {
                new Manual().Schema,
                new Template().Schema,
                new ManualContents().Schema,
            };

            Database.ExecuteBootstrapCode(ListOfSchemas);
        }

        public static TableSchema[] ListOfSchemas = null;

        public void Insert(TableRow tableRow)
        {
            using(InsertStatement ins = new InsertStatement(Database, tableRow))
            {
                ins.Execute();
                tableRow.SetPrimaryKey(ins.LastRowID);
            }
        }

        public void Update(TableRow tableRow)
        {
            using (UpdateStatement ins = new UpdateStatement(Database, tableRow))
            {
                ins.Execute();
            }
        }

        public void Delete<T>(T instance)
            where T: TableRow
        {
            Database.ExecuteNonQuery(instance.CreateDeleteStatement());
        }

        public void ExecuteNonQuery(string expression)
        {
            Database.ExecuteNonQuery(expression);
        }


        public void DeleteAll<T>()
            where T : TableRow, new()
        {
            T instance = new T();
            Database.ExecuteNonQuery(string.Format("delete from {0};", instance.Schema.Name));
            instance = null;
        }

        public List<T> SelectAll<T>(
                string whereClause = null,
                IEnumerable<DbParameter> parameters = null)
            where T : TableRow, new()
        {
            List<T> result = new List<T>();

            T tableRow = null;
            using (SelectStatement ss = SelectStatement.Select<T>(Database, whereClause, parameters))
            {
                while(ss.Next())
                {
                    if( tableRow == null )
                    {
                        tableRow = new T();
                    }
                    ss.ReadTableRow(tableRow);
                    result.Add(tableRow);
                    tableRow = null;
                }
            }
            return result;
        }


        public List<DERIVED> SelectAll<T, DERIVED>(
                string whereClause = null,
                IEnumerable<DbParameter> parameters = null) 
            where T : TableRow, new() 
            where DERIVED: T, new()
        {
            List<DERIVED> result = new List<DERIVED>();

            DERIVED tableRow = null;
            using (SelectStatement ss = SelectStatement.Select<T>(Database, whereClause, parameters))
            {
                while (ss.Next())
                {
                    if (tableRow == null)
                    {
                        tableRow = new DERIVED();
                    }
                    ss.ReadTableRow(tableRow);
                    result.Add(tableRow);
                    tableRow = null;
                }
            }
            return result;
        }

        public void SelectAllOnExistingList<T>(
                IList<T> existingList,
                string whereClause = null,
                IEnumerable<DbParameter> parameters = null)
            where T : TableRow, new()
        {
            Dictionary<long, T> lookup = new Dictionary<long, T>();
            foreach (T instance in existingList)
            {
                lookup[instance.GetUniqueID()] = instance;
            }

            T tableRow = null;
            using (SelectStatement ss = SelectStatement.Select<T>(Database, whereClause, parameters))
            {
                while (ss.Next())
                {
                    if (tableRow == null)
                    {
                        tableRow = new T();
                    }
                    ss.ReadTableRow(tableRow);
                    long id = tableRow.GetUniqueID();
                    if (lookup.ContainsKey(id))
                    {
                        lookup[id].UpdateFrom(tableRow);
                    }
                    else
                    {
                        existingList.Add(tableRow);
                    }
                    tableRow = null;
                }
            }
        }

        public void SelectAllOnExistingList<T, DERIVED>(
                IList<DERIVED> existingList,
                string whereClause = null,
                IEnumerable<DbParameter> parameters = null)
            where T : TableRow, new()
            where DERIVED : T, new()
        {
            Dictionary<long, DERIVED> lookup = new Dictionary<long, DERIVED>();
            foreach (DERIVED instance in existingList)
            {
                lookup[instance.GetUniqueID()] = instance;
            }

            DERIVED tableRow = null;
            using (SelectStatement ss = SelectStatement.Select<T>(Database, whereClause, parameters))
            {
                while (ss.Next())
                {
                    if (tableRow == null)
                    {
                        tableRow = new DERIVED();
                    }
                    ss.ReadTableRow(tableRow);
                    long id = tableRow.GetUniqueID();
                    if( lookup.ContainsKey(id) )
                    {
                        lookup[id].UpdateFrom(tableRow);
                    }
                    else
                    {
                        existingList.Add(tableRow);
                    }
                    tableRow = null;
                }
            }
        }

        public bool IsEmpty(string table)
        {
            using (SelectStatement ss = new SelectStatement(Database, string.Format("select * from {0}", table), null))
            {
                while (ss.Next())
                {
                    return false;
                }
            }
            return true;
        }


        public void SelectAllFromStatement<T>(
                IList<T> existingList,
                string statement,
                IEnumerable<DbParameter> parameters = null)
            where T : TableRow, new()
        {
            // create a lookup dictionary. When we later query the database we can decide
            // whether it is feasible to update an existing object, or we need to create a new one
            Dictionary<long, T> lookup = new Dictionary<long, T>();
            foreach (T instance in existingList)
            {
                lookup[instance.GetUniqueID()] = instance;
            }

            T tableRow = null;
            using (SelectStatement ss = new SelectStatement(Database, statement, parameters))
            {
                while (ss.Next())
                {
                    if (tableRow == null)
                    {
                        tableRow = new T();
                    }
                    
                    // has to handle subclasses as well
                    ss.ReadTableRow(tableRow);

                    // now that the data has been read, verify if we can use update or must use insert
                    long id = tableRow.GetUniqueID();
                    if (lookup.ContainsKey(id))
                    {
                        lookup[id].UpdateFrom(tableRow);
                    }
                    else
                    {
                        existingList.Add(tableRow);
                    }
                    tableRow = null;
                }
            }
        }

        public T Select<T>(int id) where T: TableRow, new()
        {
            T result = new T();

            using(SelectStatement ss = SelectStatement.Select<T>(Database, string.Format("where {0} = {1}", result.Schema.PrimaryKey.DatabaseFieldName, id)))
            {
                while (ss.Next())
                {
                    ss.ReadTableRow(result);
                    return result;
                }
            }
            return null;
        }

    }
}
