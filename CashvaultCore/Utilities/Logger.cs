using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CashvaultCore.Utilities
{
    public static class Logger
    {
        public static void logToFile(string dirr, string file, string content)
        {
            string dateFolder = DateTime.Now.ToString("dd-MM-yyyy");
            string dir = dirr +"\\" + dateFolder+"\\";
            if (Directory.Exists(dir))
            {
                File.AppendAllText(dir + file, Environment.NewLine + DateTime.Now.ToString() +
                Environment.NewLine + "####" +
                Environment.NewLine + content);
            }
            else
            {
                Directory.CreateDirectory(dir);
                File.AppendAllText(dir + file, Environment.NewLine + DateTime.Now.ToString() +
                Environment.NewLine + "####" +
                Environment.NewLine + content);
            }
        }

        public static void logToFile(Exception excpt, string DirPath)
        {
            string log = excpt.ToString()
                + " "
                + DateTime.Now.ToString()
                + Environment.NewLine
                + excpt.ToString()
                + Environment.NewLine
                + Environment.NewLine
                + Environment.NewLine;

            DateTime logTimeStamp = DateTime.Today;
            string thisYear = logTimeStamp.ToString("yyyy");
            string thisMonth = logTimeStamp.ToString("MMMM");
            string thisDate = logTimeStamp.ToString("dd-MM-yyyy");

            string exceptionErrorPath = string.Format(@"{0}\{1}-{2}", DirPath, thisMonth, thisYear);

            try
            {
                if (Directory.Exists(exceptionErrorPath))
                {
                    StreamWriter w = File.AppendText(string.Format("{0}\\{1}.txt", exceptionErrorPath, thisDate));
                    w.Write(string.Format(log));
                    w.Flush();
                    w.Close();
                }
                else
                {
                    DirectoryInfo dir = Directory.CreateDirectory(exceptionErrorPath);
                    StreamWriter w = File.AppendText(string.Format("{0}\\{1}.txt", exceptionErrorPath, thisDate));
                    w.Write(string.Format(log));
                    w.Flush();
                    w.Close();
                }
            }
            catch (Exception)
            {
                //We will have to ignore this for now...
            }
        }

        public static void logToFile(String eventLog, string DirPath)
        {
            string log =
                DateTime.Now.ToString()
                + Environment.NewLine
                + eventLog;

            DateTime logTimeStamp = DateTime.Today;
            string thisYear = logTimeStamp.ToString("yyyy");
            string thisMonth = logTimeStamp.ToString("MMMM");
            string thisDate = logTimeStamp.ToString("dd-MM-yyyy");

            string logDir = string.Format(@"{0}\{1}-{2}", DirPath, thisMonth, thisYear);

            try
            {
                if (Directory.Exists(logDir))
                {
                    StreamWriter w = File.AppendText(string.Format("{0}\\{1}.txt", logDir, thisDate));
                    w.Write(string.Format(log));
                    w.Flush();
                    w.Close();
                }
                else
                {
                    DirectoryInfo dir = Directory.CreateDirectory(logDir);
                    StreamWriter w = File.AppendText(string.Format("{0}\\{1}.txt", logDir, thisDate));
                    w.Write(string.Format(log));
                    w.Flush();
                    w.Close();
                }
            }
            catch (Exception)
            {
                //We will have to ignore this for now...
            }
        }

        public static void logToFile(String log, bool CreateNewFile, string DirPath)
        {
            log = log
                + " "
                + DateTime.Now.ToString()
                + Environment.NewLine
                + Environment.NewLine
                + Environment.NewLine;
            DateTime logTimeStamp = DateTime.Today;
            string thisYear = logTimeStamp.ToString("yyyy");
            string thisMonth = logTimeStamp.ToString("MMMM");

            string thisDate = CreateNewFile ? logTimeStamp.ToString("dd-MM-yyyy hhmmss") + "." + Guid.NewGuid().ToString() : logTimeStamp.ToString("dd-MM-yyyy");

            string LogPath = string.Format(@"{0}\{1}-{2}", DirPath, thisMonth, thisYear);

            if (Directory.Exists(LogPath))
            {
                StreamWriter w = File.AppendText(string.Format("{0}\\{1}.txt", LogPath, thisDate));
                w.Write(string.Format(log));
                w.Flush();
                w.Close();
            }
            else
            {
                DirectoryInfo dir = Directory.CreateDirectory(LogPath);
                StreamWriter w = File.AppendText(string.Format("{0}\\{1}.txt", LogPath, thisDate));
                w.Write(string.Format(log));
                w.Flush();
                w.Close();
            }
        }

        public static void logToFile(String log, string DirPath, bool CreateNewFile = false, string fileNamePrefix = "", string fileExtention = "txt", bool addTimeStamp = true, bool LogRequiresFormating = false)
        {
            log = addTimeStamp ? log + DateTime.Now.ToString() + Environment.NewLine :
            log = log + Environment.NewLine;

            DateTime logTimeStamp = DateTime.Today;
            string thisYear = logTimeStamp.ToString("yyyy");
            string thisMonth = logTimeStamp.ToString("MMMM");

            string thisDate = CreateNewFile ? DateTime.Now.ToString("yyyy-MM-dd-Hmmss") : DateTime.Now.ToString("yyyy-MM-dd");

            string exceptionErrorPath = string.Format(@"{0}\{1}-{2}", DirPath, thisMonth, thisYear);

            if (Directory.Exists(exceptionErrorPath))
            {
                StreamWriter w = File.AppendText(string.Format("{0}\\{1}.{2}", exceptionErrorPath, fileNamePrefix + "_" + thisDate, fileExtention));
                w.Write(LogRequiresFormating ? string.Format(log) : log);
                w.Flush();
                w.Close();
            }
            else
            {
                DirectoryInfo dir = Directory.CreateDirectory(exceptionErrorPath);
                StreamWriter w = File.AppendText(string.Format("{0}\\{1}.{2}", exceptionErrorPath, fileNamePrefix + "_" + thisDate, fileExtention));
                w.Write(string.Format(log));
                w.Flush();
                w.Close();
            }
        }
    }
}
