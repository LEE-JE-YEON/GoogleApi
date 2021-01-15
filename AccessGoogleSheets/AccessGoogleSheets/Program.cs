using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AccessGoogleSheets
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveAssembly);
            ApplicationStart();

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }

        private static void ApplicationStart()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            //Assembly thisAssembly = Assembly.GetExecutingAssembly();
            //var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
            //foreach (var r in thisAssembly.GetManifestResourceNames())
            //{
            //    if (r.EndsWith(name))
            //    {
            //        using (Stream stream = thisAssembly.GetManifestResourceStream(r))
            //        {
            //            if (stream != null)
            //            {
            //                byte[] assembly = new byte[stream.Length];
            //                stream.Read(assembly, 0, assembly.Length);
            //                Console.WriteLine("Dll file load : " + r);
            //                return Assembly.Load(assembly);
            //            }
            //        }
            //    }
            //}
            //return null;

            //Lamda
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
            var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(name));

            if (resources.Count() > 0)
            {
                string resourceName = resources.First();
                using (Stream stream = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] assembly = new byte[stream.Length];
                        stream.Read(assembly, 0, assembly.Length);
                        Console.WriteLine("Dll file load : " + resourceName);
                        return Assembly.Load(assembly);
                    }
                }
            }
            return null;

        }
    }
}
