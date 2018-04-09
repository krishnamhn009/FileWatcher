using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
//https://weblogs.asp.net/ashben/31773

namespace FileWatcher
{
    class Program
    {
        static Dictionary<string, FileDetails> fileDetails = new Dictionary<string, FileDetails>();
        static FileSystemWatcher watcher = new FileSystemWatcher();
        static int counter = 0;
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Directory path or file format is missing");
                Console.ReadLine();
            } 
            else
            {
                string path = args[0].Trim();
                string filePattern = args[1].Trim();
                Timer timer = new Timer(10000);
                timer.Elapsed +=  (sender, e) =>  HandleTimer(path,filePattern);
                timer.Start();

                //Watch(path, filePattern);

                Console.Write("Press any key to exit... \n");
                Console.ReadKey();
            }
        }

        private async static void HandleTimer(string path, string filePattern)
        {
            Console.WriteLine("Counter :" + ++counter);
            if (IsValidDirectory(path))
            {
                await Task.Run(() => WatchDirectory(path, filePattern));
            }
            else
            {
                Console.Write("Invalid Directory :" + path);
                Console.ReadLine();
            }
        }
        static bool IsValidDirectory(string path)
        {

            // get the file attributes for file or directory

            //if (!Directory.Exists(@"C:\folderName")) return;
            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
                return true;
            else
                return false;
        }

        public static async void WatchDirectory(string targetDirectory,string filePattern)
        {           
            await Task.Run(() => Watch(targetDirectory, filePattern));
        }

     
        private static void Watch(string path,string filePattern )
        {
            //FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = filePattern;
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.Attributes |
                                    NotifyFilters.CreationTime |
                                    NotifyFilters.FileName |
                                    //NotifyFilters.LastAccess |
                                    NotifyFilters.LastWrite |
                                    //NotifyFilters.Size |
                                    NotifyFilters.Security;
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            FileDetails fd = new FileDetails() {
                FileName = e.Name,
                FilePath = e.FullPath,
                TimeCreated = new FileInfo(e.FullPath).CreationTime,
                TimeModified = new FileInfo(e.FullPath).LastWriteTime,
            };
            StringBuilder sb = new StringBuilder();
            sb.Append(e.ChangeType.ToString());
            sb.Append(" : ");
            sb.Append(e.Name);
            sb.Append(" : ");

            int count = 0;
            using (StreamReader r = new StreamReader(e.FullPath))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    count++;
                }
            }
            fd.PreviousNumOfLines = fd.CurrentNumOfLines = count;
            sb.Append(count);
            fileDetails.Add(e.Name, fd);
            Console.WriteLine(sb);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            int count = 0;
            bool isFileAccessible = false;
            while (!isFileAccessible)
            {
                try
                {
                    using (StreamReader r = new StreamReader(e.FullPath))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            count++;
                        }
                    }
                    isFileAccessible = true;
                    fileDetails[e.Name].TimeModified = DateTime.Now;
                }
                catch
                {

                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(e.ChangeType.ToString());
            sb.Append(" : ");
            sb.Append(e.Name);
            sb.Append(" : ");

            fileDetails[e.Name].PreviousNumOfLines = fileDetails[e.Name].CurrentNumOfLines;
            fileDetails[e.Name].CurrentNumOfLines = count;
            sb.Append(fileDetails[e.Name].CurrentNumOfLines - fileDetails[e.Name].PreviousNumOfLines);
            Console.WriteLine(sb);
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            Console.WriteLine(e.ChangeType + " : " + e.Name);
            fileDetails.Remove(e.Name);
        }
    }
    class FileDetails
    {
        public string FileName { get; set; } = String.Empty;
        public string FilePath { get; set; } = String.Empty;
        public DateTime TimeCreated { get; set; }
        public DateTime TimeModified { get; set; }
        public int PreviousNumOfLines { get; set; } = 0;
        public int CurrentNumOfLines { get; set; } = 0;
    }
}
