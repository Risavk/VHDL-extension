using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VHDL_Extension
{
    static class Helper
    {
        /// <summary>
        /// List of VHDL keywords
        /// </summary>
        public static List<string> Keywords;

        public static void LoadKeywoards()
        {
            //Load all keywords
            try
            {
                var path = Path.GetDirectoryName(typeof(VHDL_classifier).Assembly.Location);
                if (path == null)
                {
                    MessageBox.Show("Couldn't load keywords");
                    return;
                }
                path = Path.Combine(path, "keywords.csv");
                using (var reader = new StreamReader(path))
                {
                    Keywords = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            var values = line.Split(';');

                            Keywords.Add(values[0]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading keywords");
            }
        }

        public static void OpenErrorWindow()
        {
            IVsUIShell vsUIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
            Guid guid = new Guid(ToolWindowGuids80.ErrorList);
            IVsWindowFrame windowFrame;
            int result = vsUIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref guid, out windowFrame);   // Find MyToolWindow

            if (result != VSConstants.S_OK)
                result = vsUIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref guid, out windowFrame); // Crate MyToolWindow if not found

            if (result == VSConstants.S_OK)                                                                           // Show MyToolWindow
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
