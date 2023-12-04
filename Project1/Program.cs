using System;
using System.Diagnostics;
using System.IO;

// Author: Luke Ward

namespace Project1
{
    class Runner
    {
        /// <summary>
        /// Takes arguments from command line and plugs into proper methods
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var timer = new Stopwatch();
            if (args.Length < 2)
            {
                help();
            }
            else
            {
                switch (args[0])
                {
                    case "-s":
                        timer.Start();
                        long[] temp = SingleThread(new DirectoryInfo(args[1]));
                        timer.Stop();
                        Console.WriteLine("Directory {0}\n",args[1]);
                        Console.WriteLine("Sequential Calculated in: {0}s",timer.Elapsed.TotalSeconds);
                        Console.WriteLine("{0} folders, {1} files, {2} bytes",temp[2].ToString("n0"),temp[1].ToString("n0"),temp[0].ToString("n0"));
                        break;
                    case "-p":
                        timer.Start();
                        long[] temp2 = Multithread(new DirectoryInfo(args[1]));
                        timer.Stop();
                        Console.WriteLine("Directory {0}\n",args[1]);
                        Console.WriteLine("Parallel Calculated in: {0}s",timer.Elapsed.TotalSeconds);
                        Console.WriteLine("{0} folders, {1} files, {2} bytes",temp2[2].ToString("n0"),temp2[1].ToString("n0"),temp2[0].ToString("n0"));
                        break;
                    case "-b":
                        Console.WriteLine("Directory {0}\n",args[1]);
                        timer.Start();
                        long[] temp4 = Multithread(new DirectoryInfo(args[1]));
                        timer.Stop();
                        Console.WriteLine("Parallel Calculated in: {0}s",timer.Elapsed.TotalSeconds);
                        Console.WriteLine("{0} folders, {1} files, {2} bytes\n",temp4[2].ToString("n0"),temp4[1].ToString("n0"),temp4[0].ToString("n0"));
                        timer.Reset();
                        timer.Start();
                        long[] temp3 = SingleThread(new DirectoryInfo(args[1]));
                        timer.Stop();
                        Console.WriteLine("Sequential Calculated in: {0}s",timer.Elapsed.TotalSeconds);
                        Console.WriteLine("{0} folders, {1} files, {2} bytes\n",temp3[2].ToString("n0"),temp3[1].ToString("n0"),temp3[0].ToString("n0"));
                        break;
                    case "help":
                        help();
                        break;
                    default:
                        help();
                        break;
                }
            }
           
            //var dirinfo = new DirectoryInfo("C:/Users/Duke4/CA123/GradingScripts");
        }
        
        /// <summary>
        /// Static method to display help text
        /// </summary>
        static void help()
        {
            Console.WriteLine("Usage: du [-s] [--p] [-b] <path>");
            Console.WriteLine("Summarize the disk usage of the set of FILES, recursively for directories.\nYou MUST specify one of the parameters, -s, --p, or -b");
            Console.WriteLine("-s\tRun in single threaded mode.\n-p\tRun in parallel mode (uses all available processors)\n-b\tRun in both parallel and single threaded mode.\n\tRun parallel followed by sequential mode");
        }
        
        /// <summary>
        /// Sequential read of the data inside a directory.  Displays
        /// folder count, file count, and size of all contents in bytes.
        /// </summary>
        /// <param name="dirinfo"></param>
        /// <returns></returns>
        static long[] SingleThread(DirectoryInfo dirinfo)
        {
       
            long[] info = new long[3];
            var di = dirinfo;
            
            //Permissions exception handling
            FileInfo[] fis;
            try
            {
                fis = di.GetFiles();
            }
            catch
            {
                fis = Array.Empty<FileInfo>();;
            }
            foreach (FileInfo fi in fis)
            {
                info[0] += fi.Length;
            }

            info[1] += fis.Length;
            
            //Permissions exception handling
            DirectoryInfo[] dis;
            try
            {
                dis = di.GetDirectories();
            }
            catch 
            {dis = Array.Empty<DirectoryInfo>();;
            }
            
            info[2] += dis.Length;
            foreach (DirectoryInfo d in dis)
            {        
                long[] temp = SingleThread(d);
                info[0] += temp[0];
                info[1] += temp[1];
                info[2] += temp[2];
            }

            return info;
        }
        
        /// <summary>
        /// Parallel read of the data inside a directory.  Displays
        /// folder count, file count, and size of all contents in bytes.
        /// </summary>
        /// <param name="dirinfo"></param>
        /// <returns></returns>
        static long[] Multithread(DirectoryInfo dirinfo)
        {
            long[] info = new long[3];
            var di = dirinfo;
            
            //Permissions exception handling
            FileInfo[] fis;
            try
            {
                fis = di.GetFiles();
            }
            catch 
            {
                fis = Array.Empty<FileInfo>();
            }
            
            info[0] = fis.Sum(file => file.Length);
            info[1] += fis.Length;
        
            //Permissions exception handling
            DirectoryInfo[] dis;
            try
            {
                dis = di.GetDirectories();
            }
            catch 
            {
                dis = Array.Empty<DirectoryInfo>();
            }
        
            info[2] += dis.Length;
        
            Parallel.ForEach(dis, d =>
            {
            
                long[] temp = Multithread(d);
                //Dont want to lock on the recurse because it'll wait for each one
                lock (info)
                {
                    info[0] += temp[0];
                    info[1] += temp[1];
                    info[2] += temp[2];
                }
                
            });

            return info;
        }       
    }
}
