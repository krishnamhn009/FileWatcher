using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FileWatcher
{
    class Program
    {
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
                Console.Write("Press any key to exit... ");
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
            await Task.Run(() => watch(targetDirectory, filePattern));
        }

     
        private static void watch(string path,string filePattern )
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = filePattern;
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("OnCreated Event Trigger");
            StringBuilder sb = new StringBuilder();
            sb.Append("Name: "+e.Name);           
            sb.Append(" ");
            sb.Append("Event Type: "+e.ChangeType.ToString());
            sb.Append("    ");
            sb.Append("TimeStamp: "+DateTime.Now.ToString());
            Console.WriteLine(sb);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {            
            StringBuilder sb = new StringBuilder();
            sb.Append("Name: " + e.Name);          
            sb.Append(" ");
            sb.Append("Event Type: " + e.ChangeType.ToString());
            sb.Append("    ");
            sb.Append("TimeStamp: " + DateTime.Now.ToString());
            Console.WriteLine(sb);
        }

    }
}
