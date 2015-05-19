using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace manualisator
{
    static class Program
    {
        internal static manualisator.Properties.Settings Settings;
        internal static PersistentSettings PersistentSettings;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Settings = new manualisator.Properties.Settings();
            PersistentSettings = new PersistentSettings(Settings, "p-nand-q.com\\manualisator");
            PersistentSettings.Load();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
