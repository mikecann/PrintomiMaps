using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PrintomiImageCleaner
{
    class StitchUtils
    {
        public static string toolsDir;

        public static bool Crop(int w, int h, string inFile, string outFile)
        {
            return Exec("jpegtran.exe", string.Format("-crop {0}x{1} {2} {3}", w, h, inFile, outFile));
        }

        public static bool Drop(string fileToDrop, int x, int y, string fileToDropOn, string outFile)
        {
            return Exec("jpegtran.exe", string.Format("-drop +{0}+{1} {2} {3} {4}", x, y, fileToDrop, fileToDropOn, outFile));
        }

        public static bool Optimize(string inFile, string outFile)
        {
            return Exec("jpegtran.exe", string.Format("-optimize {0} {1}", inFile, outFile));
        }

        public static bool Pack(string inFile)
        {
            return Exec("packJPG.exe", string.Format("-np {0}", inFile));
        }

        public static bool WebP(string inFile, string outFile)
        {
            return Exec("cwebp.exe", string.Format("-quality 100 {0} -o {1}", inFile, outFile));
        }
        

        private static bool Exec(string lib, string args)
        {
            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = args;
            start.FileName = toolsDir + lib;
            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;
       
            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                string stdout = proc.StandardOutput.ReadToEnd();
                string errorout = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                if (proc.ExitCode == 0){}// Console.WriteLine(string.Format("Command success. Args: {0}, Result: {1}", args, stdout));
                else Console.WriteLine(string.Format("Command error! Args: {0}, Result: {1}", args, errorout));
                return proc.ExitCode == 0;
            }
        }
    }
}
