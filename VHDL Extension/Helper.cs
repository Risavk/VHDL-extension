using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VHDL_Extension
{
    static class Helper
    {
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
    }
}
