﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace Shared.Database
{
    public class MySQLObjectDatabase : ObjectDatabase
    {
        public MySQLObjectDatabase(DataConnection connection)
            : base(connection)
        {
        }

        #region SQL implementation

        // Ajoute un nouvel objet a la DB
        protected override bool AddObjectImpl(DataObject dataObject)
        {
            try
            {
                string tableName = dataObject.TableName;

                if (dataObject.ObjectId == null)
                {
                    dataObject.ObjectId = IDGenerator.GenerateID();
                }

                var columns = new StringBuilder();
                var values = new StringBuilder();

                MemberInfo[] objMembers = dataObject.GetType().GetMembers();
                bool hasRelations = false;
                string dateFormat = Connection.GetDBDateFormat();

                columns.Append("`" + tableName + "_ID`");
                values.Append("'" + Escape(dataObject.ObjectId) + "'");

                for (int i = 0; i < objMembers.Length; i++)
                {
                    if (!hasRelations)
                    {
                        object[] relAttrib = GetRelationAttributes(objMembers[i]);
                        hasRelations = relAttrib.Length > 0;
                    }
                    object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
                    object[] attrib = objMembers[i].GetCustomAttributes(typeof(DataElement), true);
                    if (attrib.Length > 0 || keyAttrib.Length > 0)
                    {
                        object val = null;
                        if (objMembers[i] is PropertyInfo)
                        {
                            val = ((PropertyInfo)objMembers[i]).GetValue(dataObject, null);
                        }
                        else if (objMembers[i] is FieldInfo)
                        {
                            val = ((FieldInfo)objMembers[i]).GetValue(dataObject);
                        }

                        columns.Append(", ");
                        values.Append(", ");
                        columns.Append("`" + objMembers[i].Name + "`");

                        if (val is bool)
                        {
                            val = ((bool)val) ? (byte)1 : (byte)0;
                        }
                        else if (val is DateTime)
                        {
                            val = ((DateTime)val).ToString(dateFormat);
                        }
                        else if (val is float)
                        {
                            val = ((float)val).ToString(Nfi);
                        }
                        else if (val is double)
                        {
                            val = ((double)val).ToString(Nfi);
                        }
                        else if (val is string)
                        {
                            val = Escape(val.ToString());
                        }
                        else if (val is List<byte>)
                        {
                            List<byte> bytes = val as List<byte>;
                            string Result="";
                            foreach (byte b in bytes)
                                Result += b.ToString("X2") + " ";
                            val = Result;
                        }
                        else if (val is List<UInt32>)
                        {
                            List<UInt32> bytes = val as List<UInt32>;
                            string Result = "";
                            foreach (UInt32 b in bytes)
                                Result += b.ToString("X4") + " ";
                            val = Result;
                        }
                        else if (val is List<long>)
                        {
                            List<long> bytes = val as List<long>;
                            string Result = "";
                            foreach (long b in bytes)
                                Result += b.ToString("X8") + " ";
                            val = Result;
                        }

                        values.Append('\'');
                        values.Append(val);
                        values.Append('\'');
                    }
                }

                string sql = "INSERT INTO `" + tableName + "` (" + columns + ") VALUES (" + values + ")";

                Log.Debug("MysqlObject",sql);

                int res = Connection.ExecuteNonQuery(sql);
                if (res == 0)
                {
                    Log.Error("MysqlObject", "Add Error : " + dataObject.TableName + " ID=" + dataObject.ObjectId + "Query = " + sql);
                    return false;
                }

                if (hasRelations)
                {
                    SaveObjectRelations(dataObject);
                }

                dataObject.Dirty = false;
                dataObject.IsValid = true;
                dataObject.IsDeleted = false;

                return true;
            }
            catch (Exception e)
            {
                Log.Error("MysqlObject", "Add Error : " + dataObject.TableName + " " + dataObject.ObjectId + e.ToString());
            }

            return false;
        }

        // Persiste l'objet dans la DB
        protected override void SaveObjectImpl(DataObject dataObject)
        {
            try
            {
                
                string tableName = dataObject.TableName;

                var sb = new StringBuilder("UPDATE `" + tableName + "` SET ");

                BindingInfo[] bindingInfo = GetBindingInfo(dataObject.GetType());
                bool hasRelations = false;
                bool first = true;
                string dateFormat = Connection.GetDBDateFormat();

                for (int i = 0; i < bindingInfo.Length; i++)
                {
                    BindingInfo bind = bindingInfo[i];

                    if (bind.ReadOnly)
                    {
                        continue;
                    }

                    if (!hasRelations)
                    {
                        hasRelations = bind.HasRelation;
                    }
                    if (!bind.HasRelation)
                    {
                        object val = null;
                        if (bind.Member is PropertyInfo)
                        {
                            val = ((PropertyInfo)bind.Member).GetValue(dataObject, null);
                        }
                        else if (bind.Member is FieldInfo)
                        {
                            val = ((FieldInfo)bind.Member).GetValue(dataObject);
                        }
                        else
                        {
                            continue;
                        }

                        if (!first)
                        {
                            sb.Append(", ");
                        }
                        else
                        {
                            first = false;
                        }

                        if (val is bool)
                        {
                            val = ((bool)val) ? (byte)1 : (byte)0;
                        }
                        else if (val is DateTime)
                        {
                            val = ((DateTime)val).ToString(dateFormat);
                        }
                        else if (val is float)
                        {
                            val = ((float)val).ToString(Nfi);
                        }
                        else if (val is double)
                        {
                            val = ((double)val).ToString(Nfi);
                        }
                        else if (val is string)
                        {
                            val = Escape(val.ToString());
                        }
                        else if (val is List<byte>)
                        {
                            List<byte> bytes = val as List<byte>;
                            string Result = "";
                            foreach (byte b in bytes)
                                Result += b.ToString("X2") + " ";
                            val = Result;
                        }
                        else if (val is List<UInt32>)
                        {
                            List<UInt32> bytes = val as List<UInt32>;
                            string Result = "";
                            foreach (UInt32 b in bytes)
                                Result += b.ToString("X4") + " ";
                            val = Result;
                        }
                        else if (val is List<long>)
                        {
                            List<long> bytes = val as List<long>;
                            string Result = "";
                            foreach (long b in bytes)
                                Result += b.ToString("X8") + " ";
                            val = Result;
                        }

                        sb.Append("`" + bind.Member.Name + "` = ");
                        sb.Append('\'');
                        sb.Append(val);
                        sb.Append('\'');
                    }
                }

                sb.Append(" WHERE `" + tableName + "_ID` = '" + Escape(dataObject.ObjectId) + "'");

                string sql = sb.ToString();
                Log.Debug("MysqlObject", sql);

                int res = Connection.ExecuteNonQuery(sql);
                if (res == 0)
                {
                    Log.Error("MysqlObject", "Modify error : " + dataObject.TableName + " ID=" + dataObject.ObjectId + " --- keyvalue changed? " + sql + " " + Environment.StackTrace);
                    return;
                }

                if (hasRelations)
                {
                    SaveObjectRelations(dataObject);
                }

                dataObject.Dirty = false;
                dataObject.IsValid = true;
            }
            catch (Exception e)
            {
                Log.Error("MysqlObject", "Modify error : " + dataObject.TableName + " " + dataObject.ObjectId + e.ToString() );
            }
        }

        // Supprime un objet de la DB
        protected override void DeleteObjectImpl(DataObject dataObject)
        {
            try
            {
                string sql = "DELETE FROM `" + dataObject.TableName + "` WHERE `" + dataObject.TableName + "_ID` = '" +
                             Escape(dataObject.ObjectId) + "'";

                Log.Debug("MysqlObject", sql);

                int res = Connection.ExecuteNonQuery(sql);
                if (res == 0)
                {
                    Log.Error("MysqlObject", "Delete Object : " + dataObject.TableName + " failed! ID=" + dataObject.ObjectId + " " + Environment.StackTrace);
                }

                dataObject.IsValid = false;

                DeleteFromCache(dataObject.TableName, dataObject);
                DeleteObjectRelations(dataObject);

                dataObject.IsDeleted = true;
            }
            catch (Exception e)
            {
                throw new DatabaseException("Delete Databaseobject failed!", e);
            }
        }


        protected override DataObject FindObjectByKeyImpl(Type objectType, object key)
        {
            MemberInfo[] members = objectType.GetMembers();
            var ret = Activator.CreateInstance(objectType) as DataObject;

            string tableName = ret.TableName;
            DataTableHandler dth = TableDatasets[tableName];
            string whereClause = null;

            if (dth.UsesPreCaching)
            {
                DataObject obj = dth.GetPreCachedObject(key);
                if (obj != null)
                    return obj;
            }

            // Escape PK value
            key = Escape(key.ToString());

            for (int i = 0; i < members.Length; i++)
            {
                object[] keyAttrib = members[i].GetCustomAttributes(typeof(PrimaryKey), true);
                if (keyAttrib.Length > 0)
                {
                    whereClause = "`" + members[i].Name + "` = '" + key + "'";
                    break;
                }
            }

            if (whereClause == null)
            {
                whereClause = "`" + ret.TableName + "_ID` = '" + key + "'";
            }

            var objs = SelectObjectsImpl(objectType, whereClause, IsolationLevel.DEFAULT);
            if (objs.Length > 0)
            {
                dth.SetPreCachedObject(key, objs[0]);
                return objs[0];
            }

            return null;
        }

        // Retourne l'objet a partir de sa primary key
        protected override TObject FindObjectByKeyImpl<TObject>(object key)
        {
            MemberInfo[] members = typeof(TObject).GetMembers();
            var ret = (TObject)Activator.CreateInstance(typeof(TObject));

            string tableName = ret.TableName;
            DataTableHandler dth = TableDatasets[tableName];
            string whereClause = null;

            if (dth.UsesPreCaching)
            {
                DataObject obj = dth.GetPreCachedObject(key);
                if (obj != null)
                    return obj as TObject;
            }

            // Escape PK value
            key = Escape(key.ToString());

            for (int i = 0; i < members.Length; i++)
            {
                object[] keyAttrib = members[i].GetCustomAttributes(typeof(PrimaryKey), true);
                if (keyAttrib.Length > 0)
                {
                    whereClause = "`" + members[i].Name + "` = '" + key + "'";
                    break;
                }
            }

            if (whereClause == null)
            {
                whereClause = "`" + ret.TableName + "_ID` = '" + key + "'";
            }

            var objs = SelectObjectsImpl<TObject>(whereClause, IsolationLevel.DEFAULT);
            if (objs.Count > 0)
            {
                dth.SetPreCachedObject(key, objs[0]);
                return objs[0];
            }

            return null;
        }

        // Sélectionne tous les objets d'une table
        protected override DataObject[] SelectObjectsImpl(Type objectType, string whereClause, IsolationLevel isolation)
        {
            string tableName = GetTableOrViewName(objectType);
            var dataObjects = new List<DataObject>(64);

            // build sql command
            var sb = new StringBuilder("SELECT `" + tableName + "_ID`, ");
            bool first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(objectType);
            for (int i = 0; i < bindingInfo.Length; i++)
            {
                if (!bindingInfo[i].HasRelation)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }
                    sb.Append("`" + bindingInfo[i].Member.Name + "`");
                }
            }

            sb.Append(" FROM `" + tableName + "`");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            string sql = sb.ToString();

            Log.Debug("MysqlObject", "DataObject[] SelectObjectsImpl: " + sql);

            int objCount = 0;

            // read data and fill objects
            Connection.ExecuteSelect(sql, delegate(MySqlDataReader reader)
            {
                var data = new object[reader.FieldCount];
                while (reader.Read())
                {
                    objCount++;

                    reader.GetValues(data);
                    var id = (string)data[0];

                    // fill new data object
                    var obj = Activator.CreateInstance(objectType) as DataObject;
                    obj.ObjectId = id;

                    bool hasRelations = false;
                    int field = 1;
                    // we can use hard index access because we iterate the same order here
                    for (int i = 0; i < bindingInfo.Length; i++)
                    {
                        BindingInfo bind = bindingInfo[i];
                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            object val = data[field++];
                            if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
                            {
                                if (bind.Member is PropertyInfo)
                                {
                                    Type type = ((PropertyInfo)bind.Member).PropertyType;

                                    try
                                    {
                                        if (type == typeof(bool))
                                        {
                                            // special handling for bool
                                            ((PropertyInfo)bind.Member).SetValue(obj,
                                                                                  (val.ToString() == "0") ? false : true,
                                                                                  null);
                                        }
                                        else if (type == typeof(DateTime))
                                        {
                                            // special handling for datetime
                                            if (val is MySqlDateTime)
                                            {
                                                ((PropertyInfo)bind.Member).SetValue(obj,
                                                                                      ((MySqlDateTime)val).GetDateTime(),
                                                                                      null);
                                            }
                                            else
                                            {
                                                ((PropertyInfo)bind.Member).SetValue(obj,
                                                                                      ((IConvertible)val).ToDateTime(null),
                                                                                      null);
                                            }
                                        }
                                        else if (type == typeof(List<byte>))
                                        {
                                            string sVal = val as string;
                                            List<byte> bytes = new List<byte>();
                                            foreach (string R in sVal.Split(' '))
                                                bytes.Add(byte.Parse(R));

                                            val = bytes;

                                        }
                                        else
                                        {
                                            ((PropertyInfo)bind.Member).SetValue(obj, val, null);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error("MysqlObject",
                                                tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName +
                                                " doesnt fit to " + bind.Member.DeclaringType.FullName + e.ToString() );
                                        continue;
                                    }
                                }
                                else if (bind.Member is FieldInfo)
                                {
                                    FieldInfo Info = (FieldInfo)bind.Member;
                                    if (Info.FieldType == typeof(List<byte>))
                                    {
                                        List<byte> bytes = new List<byte>();
                                        string sval = val.ToString();
                                        foreach (string R in sval.Split(' '))
                                            if (R.Length > 0) bytes.Add(byte.Parse(R));
                                        val = bytes;
                                    }
                                    else if (Info.FieldType == typeof(List<UInt32>))
                                    {
                                        List<UInt32> bytes = new List<UInt32>();
                                        string sval = val.ToString();
                                        foreach (string R in sval.Split(' '))
                                            if (R.Length > 0) bytes.Add(UInt32.Parse(R));
                                        val = bytes;
                                    }
                                    else if (Info.FieldType == typeof(List<long>))
                                    {
                                        List<long> bytes = new List<long>();
                                        string sval = val.ToString();
                                        foreach (string R in sval.Split(' '))
                                            if (R.Length > 0) bytes.Add(long.Parse(R));
                                        val = bytes;
                                    }

                                    ((FieldInfo)bind.Member).SetValue(obj, val);
                                }
                            }
                        }
                    }

                    dataObjects.Add(obj);
                    obj.Dirty = false;

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(obj, true);
                    }

                    obj.IsValid = true;
                }
            }
            , isolation);

            return dataObjects.ToArray();
        }

        // Sélectionne tous les objets d'une table
        protected override IList<TObject> SelectObjectsImpl<TObject>(string whereClause, IsolationLevel isolation)
        {
            string tableName = GetTableOrViewName(typeof(TObject));
            var dataObjects = new List<TObject>(64);

            // build sql command
            var sb = new StringBuilder("SELECT `" + tableName + "_ID`, ");
            bool first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(typeof(TObject));
            for (int i = 0; i < bindingInfo.Length; i++)
            {
                if (!bindingInfo[i].HasRelation)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }
                    sb.Append("`" + bindingInfo[i].Member.Name + "`");
                }
            }

            sb.Append(" FROM `" + tableName + "`");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            string sql = sb.ToString();

            Log.Debug("MysqlObject", "IList<TObject> SelectObjectsImpl: " + sql);

            // read data and fill objects
            Connection.ExecuteSelect(sql, delegate(MySqlDataReader reader)
            {
                var data = new object[reader.FieldCount];
                while (reader.Read())
                {
                    reader.GetValues(data);
                    var id = (string)data[0];

                    // fill new data object
                    var obj = Activator.CreateInstance(typeof(TObject)) as TObject;
                    obj.ObjectId = id;

                    bool hasRelations = false;
                    int field = 1;
                    // we can use hard index access because we iterate the same order here
                    for (int i = 0; i < bindingInfo.Length; i++)
                    {
                        BindingInfo bind = bindingInfo[i];
                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            object val = data[field++];
                            if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
                            {
                                if (bind.Member is PropertyInfo)
                                {
                                    Type type = ((PropertyInfo)bind.Member).PropertyType;

                                    try
                                    {
                                        if (type == typeof(bool))
                                        {
                                            // special handling for bool
                                            ((PropertyInfo)bind.Member).SetValue(obj,
                                                                                  (val.ToString() == "0") ? false : true,
                                                                                  null);
                                        }
                                        else if (type == typeof(DateTime))
                                        {
                                            // special handling for datetime
                                            if (val is MySqlDateTime)
                                            {
                                                ((PropertyInfo)bind.Member).SetValue(obj,
                                                                                      ((MySqlDateTime)val).GetDateTime(),
                                                                                      null);
                                            }
                                            else
                                            {
                                                ((PropertyInfo)bind.Member).SetValue(obj,
                                                                                      ((IConvertible)val).ToDateTime(null),
                                                                                      null);
                                            }
                                        }
                                        else
                                        {
                                            ((PropertyInfo)bind.Member).SetValue(obj, val, null);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error("MysqlObject",
                                                tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName +
                                                " doesnt fit to " + bind.Member.DeclaringType.FullName + e.ToString() );
                                        continue;
                                    }
                                }
                                else if (bind.Member is FieldInfo)
                                {
                                    FieldInfo Info = (FieldInfo)bind.Member;
                                    if (Info.FieldType == typeof(List<byte>))
                                    {
                                        List<byte> bytes = new List<byte>();
                                        string sval = val.ToString();
                                        foreach (string R in sval.Split(' '))
                                            if(R.Length > 0) bytes.Add(byte.Parse(R));
                                        val = bytes;
                                    }
                                    else if(Info.FieldType == typeof(List<UInt32>))
                                    {
                                        List<UInt32> bytes = new List<UInt32>();
                                        string sval = val.ToString();
                                        foreach (string R in sval.Split(' '))
                                            if (R.Length > 0) bytes.Add(UInt32.Parse(R));
                                        val = bytes;
                                    }
                                    else if (Info.FieldType == typeof(List<long>))
                                    {
                                        List<long> bytes = new List<long>();
                                        string sval = val.ToString();
                                        foreach (string R in sval.Split(' '))
                                            if (R.Length > 0) bytes.Add(long.Parse(R));
                                        val = bytes;
                                    }

                                    ((FieldInfo)bind.Member).SetValue(obj, val);
                                }
                            }
                        }
                    }

                    dataObjects.Add(obj);
                    obj.Dirty = false;

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(obj, true);
                    }

                    obj.IsValid = true;
                }
            }
                , isolation);

            return dataObjects.ToArray();
        }

        // Sélectionne tous les objets d'une table
        protected override IList<TObject> SelectAllObjectsImpl<TObject>(IsolationLevel isolation)
        {
            return SelectObjectsImpl<TObject>("", isolation);
        }

        // Retourne le nombre d'objet dans la db
        protected override int GetObjectCountImpl<TObject>(string where)
        {
            string tableName = GetTableOrViewName(typeof(TObject));

            if (Connection.IsSQLConnection)
            {
                string query = "SELECT COUNT(*) FROM " + tableName;
                if (where != "")
                    query += " WHERE " + where;

                object count = Connection.ExecuteScalar(query);
                if (count is long)
                    return (int)((long)count);

                return (int)count;
            }

            return 0;
        }

        // Execute une Requète non bloquante
        protected override bool ExecuteNonQueryImpl(string rawQuery)
        {
            try
            {
                Log.Debug("MysqlObject", rawQuery);

                int res = Connection.ExecuteNonQuery(rawQuery);
                if (res == 0)
                {
                    Log.Error("MysqlObject", "Execution error : " + rawQuery);

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error("MysqlObject", "Execution error : " + rawQuery + e.ToString() );
            }

            return false;
        }

        #endregion
    }
}
