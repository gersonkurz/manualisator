using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Security.AccessControl;

namespace manualisator.Core
{
    /// <summary>
    /// Helper class for registry methods
    /// </summary>
    /// TODO: allow use of PROAKT_REGISTRY_PATH.INI
    public static class RegTools
    {
        /// <summary>
        /// Get a string value
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="value">value</param>
        /// <param name="defval">default value</param>
        /// <returns>string result</returns>
        static public string GetString(string key, string value, string defval = "")
        {
            try
            {
                string result = defval;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rkKey = rk.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ReadKey);
                if (rkKey != null)
                {
                    result = (string)rkKey.GetValue(value, defval);
                    rkKey.Close();
                }
                rk.Close();
                return result;
            }
            catch (Exception e)
            {
                Tools.DumpException(e, "GetString({0}, {1}, {2})", key, value, defval);
                return defval;
            }
        }

        /// <summary>
        /// Set a string value
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="name">name</param>
        /// <param name="value">value</param>
        /// <returns>true if the value could be set, or false otherwise</returns>
        static public bool SetString(string key, string name, string value)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rkKey = rk.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                if (rkKey == null)
                    rkKey = rk.CreateSubKey(key);
                if (rkKey != null)
                {
                    rkKey.SetValue(name, value);
                    rkKey.Close();
                    rk.Close();
                    return true;
                }
                rk.Close();
                return false;
            }
            catch (Exception e)
            {
                Tools.DumpException(e, "SetString({0}, {1}, {2})", key, name, value);
                return false;
            }
        }

        /// <summary>
        /// Get a bool value from a string
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="defval">default value</param>
        /// <returns>result value</returns>
        static public bool GetBool(string key, string value, bool defval)
        {
            try
            {
                bool result = defval;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rkKey = rk.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ReadKey);
                if (rkKey != null)
                {
                    result = ((int)rkKey.GetValue(value, defval ? 1 : 0) == 0) ? false : true;
                    rkKey.Close();
                }
                rk.Close();
                return result;
            }
            catch (Exception e)
            {
                Tools.DumpException(e, "GetBool({0}, {1}, {2})", key, value, defval);
                return false;
            }
        }

        /// <summary>
        /// Set a bool value to the registry
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="name">name</param>
        /// <param name="value">value</param>
        /// <returns>true if the value could be set, or false otherwise</returns>
        static public bool SetBool(string key, string name, bool value)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rkKey = rk.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                if (rkKey == null)
                    rkKey = rk.CreateSubKey(key);
                if (rkKey != null)
                {
                    rkKey.SetValue(name, value ? 1 : 0);
                    rkKey.Close();
                    rk.Close();
                    return true;
                }
                rk.Close();
                return false;
            }
            catch (Exception e)
            {
                Tools.DumpException(e, "SetBool({0}, {1}, {2})", key, name, value);
                return false;
            }
        }

        /// <summary>
        /// Get an integer value from the registry
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="defval">default value</param>
        /// <returns>actual result value</returns>
        static public int GetInt(string key, string value, int defval)
        {
            try
            {
                int result = defval;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rkKey = rk.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ReadKey);
                if (rkKey != null)
                {
                    result = (int)rkKey.GetValue(value, defval);
                    rkKey.Close();
                }
                rk.Close();
                return result;
            }
            catch (Exception e)
            {
                Tools.DumpException(e, "GetInt({0}, {1}, {2})", key, value, defval);
                return 0;
            }
        }

        /// <summary>
        /// Set an integer value to the registry
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="name">name</param>
        /// <param name="value">value to set</param>
        /// <returns>true if the value could be set, or false otherwise</returns>
        static public bool SetInt(string key, string name, int value)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rkKey = rk.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                if (rkKey == null)
                    rkKey = rk.CreateSubKey(key);

                if (rkKey != null)
                {
                    rkKey.SetValue(name, value);
                    rkKey.Close();
                    rk.Close();
                    return true;
                }
                rk.Close();
                return false;
            }
            catch (Exception e)
            {
                Tools.DumpException(e, "SetInt({0}, {1}, {2})", key, name, value);
                return false;
            }
        }

        /// <summary>
        /// Delete a key by name
        /// </summary>
        /// <param name="key"></param>
        static public void DeleteKey(string key)
        {
            RegistryKey rk = Registry.LocalMachine;
            rk.DeleteSubKeyTree(key);
            rk.Close();
        }

        /// <summary>
        /// Delete a value in a key
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="name">value name</param>
        static public void DeleteValue(string key, string name)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rkKey = rk.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                if (rkKey == null)
                    return;

                rkKey.DeleteValue(name);
                rkKey.Close();
                rk.Close();
            }
            catch (Exception e)
            {
                Tools.DumpException(e, "DeleteValue({0}, {1})", key, name);
            }
        }

        /// <summary>
        /// This is obsolete really and should be replaced by regis3.dll: export a registry key as a string
        /// </summary>
        /// <param name="rk">registry key</param>
        /// <param name="key">key name</param>
        /// <returns>string with key data</returns>
        static public string EnumerateKeyAsString(RegistryKey rk, string key)
        {
            StringBuilder traceString = new StringBuilder("");
            RegistryKey rkKey = rk.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ReadKey);
            string[] subKeys = rkKey.GetSubKeyNames();
            if (rkKey != null)
            {
                string[] values = rkKey.GetValueNames();
                if ((values != null) && values.Length > 0)
                {
                    traceString.AppendFormat("\r\n[{0}\\{1}]\r\n", rk, key);
                    Array.Sort(values);
                    foreach (string value in values)
                    {
                        traceString.AppendFormat("{0,-60} = '{1}'\r\n", value, rkKey.GetValue(value));
                    }
                }
                if ((subKeys != null) && subKeys.Length > 0)
                {
                    Array.Sort(subKeys);
                    foreach (string subKey in subKeys)
                    {
                        traceString.Append(EnumerateKeyAsString(rkKey, subKey));
                    }
                }
                rkKey.Close();
            }
            return traceString.ToString();
        }
    }
}
