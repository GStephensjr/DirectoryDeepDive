using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryDeepDive
{
    internal class Program
    {
        static ConsoleProgressBar progress = new ConsoleProgressBar();
        static List<FileCopyData> files = new List<FileCopyData>();
        static List<string> searchDirectories = new List<string>();
        static string sessionTempStorage = Path.Combine(Path.GetTempPath(), "DeepSearch");
        static void Main(string[] args)
        {
            if (!Directory.Exists(sessionTempStorage))
            {
                Directory.CreateDirectory(sessionTempStorage);
            }
            progress = new ConsoleProgressBar();
            files = new List<FileCopyData>();
            Search();
            

            Console.WriteLine("press any key to reset or press 0 to exit and close temp folders");

            char input = Console.ReadKey(true).KeyChar;
            Console.WriteLine("\n \n");
            switch (input)
            {
                case '0':
                    Close();
                    break;
                default:
                    Console.WriteLine("invalid input");
                    Console.ReadKey();
                    Console.Clear();
                    Main(args);
                    break;
            }

        }
        public static void Search()
        {
            Console.WriteLine("Please enter source directory: ");
            string source = Console.ReadLine();
            if (!Directory.Exists(source)) 
            {
                throw new DirectoryNotFoundException($"Directory not found: {source}");
            }               


            Console.WriteLine("What are we looking for?");
            string query = Console.ReadLine();

            //Console.WriteLine("Please enter destination directory: ");
            string destination = Path.Combine(sessionTempStorage, query + "_Search_" + Guid.NewGuid()); //Console.ReadLine();
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
                //throw new DirectoryNotFoundException($"Directory not found: {destination}");
            }

            //var progress = new ConsoleProgressBar();
            progress.Start(0);

            CopyMatchingFilesPreserveStructure(source, query, destination);

            progress.Stop();

            Process.Start("explorer.exe", destination);
            searchDirectories.Add(destination);
            Console.WriteLine("Complete!");


        }
        public static void Close()
        {
            Console.WriteLine("Exiting. press any button to continue...");
            var input = Console.ReadKey();
            foreach(string s in searchDirectories)
            {
                if (Directory.Exists(s))
                    Directory.Delete(s, true);
            }
            Environment.Exit(0);
        }
        public static void Reset()
        {
            Console.WriteLine("press any button to continue...");
            var input = Console.ReadKey();
            Console.Clear();
            string[] args = new string[] { };
            Main(args);
        }
        public static void CopyMatchingFilesPreserveStructure(
                string sourceDirectory,
                string searchQuery,
                string destinationDirectory)
        {
            if (!Directory.Exists(sourceDirectory))
                throw new DirectoryNotFoundException(sourceDirectory);

            Directory.CreateDirectory(destinationDirectory);

            ProcessDirectory(sourceDirectory, sourceDirectory, searchQuery, destinationDirectory);
            foreach(FileCopyData file in files)
            {
                try
                {
                    File.Copy(file.file, file.destinationPath, true);
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip folders we don't have access to
                }
                catch (PathTooLongException)
                {
                    // Skip invalid/long paths
                }
                catch (DirectoryNotFoundException)
                {

                }
                progress.Tick();
            }
        }

        private static void ProcessDirectory(
            string currentDirectory,
            string rootSource,
            string searchQuery,
            string destinationRoot)
        {
            //destinationRoot = Path.Combine(Path.GetTempPath(), searchQuery + Guid.NewGuid());            
            try
            {
                // Copy matching files in current directory
                foreach (var file in Directory.EnumerateFiles(currentDirectory))
                {
                    if (Path.GetFileName(file)
                        .IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string relativePath = file.Substring(rootSource.Length)
                                                  .TrimStart(Path.DirectorySeparatorChar);

                        string destinationPath = Path.Combine(destinationRoot, relativePath);

                        string destinationFolder = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(destinationFolder))
                            Directory.CreateDirectory(destinationFolder);

                        files.Add(new FileCopyData(file, destinationPath));
                        progress.AddToTotal(1);
                    }
                }

                // Recurse into subdirectories
                foreach (var directory in Directory.EnumerateDirectories(currentDirectory))
                {
                    ProcessDirectory(directory, rootSource, searchQuery, destinationRoot);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip folders we don't have access to
            }
            catch (PathTooLongException)
            {
                // Skip invalid/long paths
            }
            catch (DirectoryNotFoundException)
            {

            }
        }
    }
}
