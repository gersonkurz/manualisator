﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace manualisator.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("template_de.dotx")]
        public string TemplateFilename_DE {
            get {
                return ((string)(this["TemplateFilename_DE"]));
            }
            set {
                this["TemplateFilename_DE"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("template_en.dotx")]
        public string TemplateFilename_EN {
            get {
                return ((string)(this["TemplateFilename_EN"]));
            }
            set {
                this["TemplateFilename_EN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Handbuecher")]
        public string ManualsDirectory {
            get {
                return ((string)(this["ManualsDirectory"]));
            }
            set {
                this["ManualsDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Vorlagen")]
        public string TemplatesDirectory {
            get {
                return ((string)(this["TemplatesDirectory"]));
            }
            set {
                this["TemplatesDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Dateien")]
        public string FilesDirectory {
            get {
                return ((string)(this["FilesDirectory"]));
            }
            set {
                this["FilesDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("lookup.docx")]
        public string LookupDocumentFilename {
            get {
                return ((string)(this["LookupDocumentFilename"]));
            }
            set {
                this["LookupDocumentFilename"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("manuals.db")]
        public string ManualsDatabaseFilename {
            get {
                return ((string)(this["ManualsDatabaseFilename"]));
            }
            set {
                this["ManualsDatabaseFilename"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UseBookmarksFromExcelSheet {
            get {
                return ((bool)(this["UseBookmarksFromExcelSheet"]));
            }
            set {
                this["UseBookmarksFromExcelSheet"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Projects\\2015\\manualisator\\_DEF_FILES")]
        public string BaseDirectory {
            get {
                return ((string)(this["BaseDirectory"]));
            }
            set {
                this["BaseDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseFilenameTemplate {
            get {
                return ((bool)(this["UseFilenameTemplate"]));
            }
            set {
                this["UseFilenameTemplate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CreateDocumentSortOrderFromBookmarks {
            get {
                return ((bool)(this["CreateDocumentSortOrderFromBookmarks"]));
            }
            set {
                this["CreateDocumentSortOrderFromBookmarks"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Product_Description_%DEVICE%_%LANGUAGE%.docx")]
        public string FilenameTemplate {
            get {
                return ((string)(this["FilenameTemplate"]));
            }
            set {
                this["FilenameTemplate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int InsertBeforeHeading1 {
            get {
                return ((int)(this["InsertBeforeHeading1"]));
            }
            set {
                this["InsertBeforeHeading1"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool WarnBeforeOverwriting {
            get {
                return ((bool)(this["WarnBeforeOverwriting"]));
            }
            set {
                this["WarnBeforeOverwriting"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutoGenerateCurrentDate {
            get {
                return ((bool)(this["AutoGenerateCurrentDate"]));
            }
            set {
                this["AutoGenerateCurrentDate"] = value;
            }
        }
    }
}
