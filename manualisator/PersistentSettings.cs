﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using log4net;
using System.Reflection;
using Microsoft.Win32;

namespace manualisator
{
    public class PersistentSettings
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal abstract class SettingsIO
        {
            protected readonly string KeyName;

            public SettingsIO(string keyName)
            {
                KeyName = keyName;
            }

            public virtual bool Begin()
            {
                return true;
            }

            public virtual void End()
            {
            }

            public abstract void Process(object settings, PropertyInfo pi);
        }

        internal abstract class RegistrySettingsIO : SettingsIO
        {
            protected RegistryKey Key = null;
            private readonly bool Writeable;

            public RegistrySettingsIO(string keyName, bool writeable)
                :   base(keyName)
            {
                Writeable = writeable;
            }

            public override bool Begin()
            {
                try
                {
                    if (Writeable )
                    {
                        Key = Registry.CurrentUser.CreateSubKey("Software\\" + KeyName);
                    }
                    else
                    {
                        Key = Registry.CurrentUser.OpenSubKey("Software\\" + KeyName, Writeable);
                    }
                    
                    return Key != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public override void End()
            {
                Key.Close();
            }


            protected void SetValue(object settings, PropertyInfo pi, object value)
            {
                if(value != null) 
                {
                    if(pi.PropertyType == typeof(bool))
                    {
                        value = ((int)value) != 0;
                    }
                }
                pi.SetValue(settings, value, null);
            }

            protected object GetValue(object settings, PropertyInfo pi)
            {
                object result = pi.GetValue(settings, null);
                if (pi.PropertyType == typeof(bool))
                {
                    return ((bool)result) ? 1 : 0;
                }
                return result;
            }

        }

        internal class LoadSettingsFromRegistry : RegistrySettingsIO
        {
            public LoadSettingsFromRegistry(string keyName)
                : base(keyName, false)
            {
            }

            public override void Process(object settings, PropertyInfo pi)
            {
                try
                {
                    object existing = Key.GetValue(pi.Name);
                    if (existing != null)
                    {
                        // else: use default
                        SetValue(settings, pi, existing);
                    }
                }
                catch(Exception e)
                {
                    manualisator.Core.Tools.DumpException(e, "LoadSettingsFromRegistry caught an exception");
                }
            }
        }

        internal class SaveSettingsToRegistry : RegistrySettingsIO
        {
            public SaveSettingsToRegistry(string keyName)
                : base(keyName, true)
            {
            }

            public override void Process(object settings, PropertyInfo pi)
            {
                try
                {
                    object value = GetValue(settings, pi);
                    if( value != null )
                    {
                        Key.SetValue(pi.Name, value);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private readonly object Settings;
        private readonly string KeyName;
        private readonly SettingsIO SaveSettings; 
        private readonly SettingsIO LoadSettings;

        public PersistentSettings(object settings, string keyName)
        {
            Settings = settings;
            KeyName = keyName;
            SaveSettings = new SaveSettingsToRegistry(KeyName);
            LoadSettings = new LoadSettingsFromRegistry(KeyName);
        }

        public void Load()
        {
            ApplySettings(Settings, LoadSettings);
        }
        
        public void Save()
        {
            ApplySettings(Settings, SaveSettings);
        }
        
        private void ApplySettings(object settings, SettingsIO settingsIO)
        {
            if(settingsIO.Begin())
            {
                Type classType = settings.GetType();
                PropertyInfo[] fi = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo info in fi)
                {
                    object[] customAttributes = info.GetCustomAttributes(typeof(UserScopedSettingAttribute), false);
                    if (customAttributes.Length > 0)
                    {
                        settingsIO.Process(settings, info);
                    }
                }
                settingsIO.End();
            }
        }

    }
}
