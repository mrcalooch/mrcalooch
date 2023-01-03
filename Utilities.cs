using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Nanopath
{
    public static class Utilities
    {
        /// <summary>
        /// StringToLocation
        /// Converts a string to a stage location value (0.00 - 99.99)
        /// </summary>
        /// <param name="toParse"></param>
        /// <param name="coerced"></param>
        /// <returns></returns>
        public static double? StringToLocation(string toParse, out bool coerced)
        {
            coerced = false;
            string strLoc = Regex.Match(toParse, @"^[0-9]*\.{0,1}[0-9]{0,2}").Value;// See if there is an real number somewhere in the string
            if (strLoc.Trim().Length > 0) return double.Parse(strLoc).CoerceToLimits(0.0, 99.99, out coerced); ; // If found, parse it out as an int
            return null;                                                            // Otherwise, return null
        }

        const int ERROR_SHARING_VIOLATION = 32;
        const int ERROR_LOCK_VIOLATION = 33;
        public static bool IsFileLocked(string file)
        {
            //check that problem is not in destination file
            if (File.Exists(file) == true)
            {
                FileStream stream = null;
                try
                {
                    stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (Exception ex2)
                {
                    //_log.WriteLog(ex2, "Error in checking whether file is locked " + file);
                    int errorCode = Marshal.GetHRForException(ex2) & ((1 << 16) - 1);
                    if ((ex2 is IOException) && (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION))
                    {
                        return true;
                    }
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
            return false;
        }
    }
}
