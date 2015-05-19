using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;

namespace manualisator.DBTools
{
    /// <summary>
    /// This class defines all the tables used in ProAKT
    /// </summary>
    public class TableRow : INotifyPropertyChanged
    {
        private TableSchema _TableSchema;

        public event PropertyChangedEventHandler PropertyChanged; //What i have to do with this?

        public void NotifyPropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(str));
            }
        }

        public string this[string key]
        {
            get
            {
                return GetType().GetProperty(key).GetValue(this).ToString();
            }
        }

        public virtual bool MatchesLowerCaseText(string textToFind)
        {
            foreach (ColumnSchema column in Schema.Columns)
            {
                PropertyInfo fieldInfo = GetType().GetProperty(column.InstanceVariableName);
                object fieldValue = fieldInfo.GetValue(this);
                string fieldValueAsString = fieldValue as string;
                if (fieldValueAsString == null)
                {
                    if( fieldValue != null )
                    {
                        fieldValueAsString = fieldValue.ToString();
                    }
                }
                if (fieldValueAsString != null)
                {
                    if( fieldValueAsString.ToLower().Contains(textToFind) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetPrimaryKey(long key)
        {
            foreach (ColumnSchema column in Schema.Columns)
            {
                if(column.Type == ColumnType.PrimaryKey)
                {
                    PropertyInfo fieldInfo = GetType().GetProperty(column.InstanceVariableName);
                    fieldInfo.SetValue(this, key);
                }
            }
        }

        public void UpdateFrom(TableRow otherRow)
        {
            foreach (ColumnSchema column in Schema.Columns)
            {
                PropertyInfo fieldInfo = GetType().GetProperty(column.InstanceVariableName);
                object thisValue = fieldInfo.GetValue(this);
                object otherValue = fieldInfo.GetValue(otherRow);

                if( (thisValue == null) || !thisValue.Equals(otherValue))
                {
                    Trace.TraceInformation("{0}: changed {1} from {2} to {3}",
                        this,
                        column.InstanceVariableName,
                        thisValue,
                        otherValue);

                    fieldInfo.SetValue(this, otherValue);
                    NotifyPropertyChanged(column.InstanceVariableName);
                }
            }
        }

        public virtual long GetUniqueID()
        {
            foreach (ColumnSchema column in Schema.Columns)
            {
                if( column.Type == ColumnType.PrimaryKey)
                {
                    return (long)GetType().GetProperty(column.InstanceVariableName).GetValue(this);
                }
            }
            Trace.Assert(false);
            return 0;
        }

        public string CreateDeleteStatement()
        {
            return string.Format("delete from {0} where {1}={2}", Schema.Name, Schema.PrimaryKey.DatabaseFieldName, GetUniqueID());
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendFormat("<{0}: ", Schema.Name);
            bool first = true;
            foreach(ColumnSchema column in Schema.Columns)
            {
                if (first)
                    first = false;
                else
                    output.Append(", ");

                object instanceValue = GetType().GetProperty(column.InstanceVariableName).GetValue(this);

                output.Append(string.Format("{0}: {1}",
                    column.InstanceVariableName,
                    instanceValue));
            }

            output.Append(">");

            return output.ToString();
        }

        public TableSchema Schema
        {
            get
            {
                if( _TableSchema == null )
                {
                    _TableSchema = TableSchema.FromClassType(GetType());
                }
                return _TableSchema;
            }
        }
    }
}
