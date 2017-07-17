using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FirewallConfigReaderCS
{
    class Program
    {
        static void Main(string[] args)
        {
            string configPath = "";
            
            if (args != null && args.Any())
                configPath = args.FirstOrDefault();
            else if (DefaultConfigurationDirectoryExists)
            {
                configPath = DefaultConfigurationDirectoryPath;
            }
            else
            {
                Debug.WriteLine($"Not parameter provided and default directory not found ({DefaultConfigurationDirectoryPath}).");
                Environment.Exit(1);
            }

            try
            {
                ConfigProcessor.Process(configPath, OutputFilePath);
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Failed to complete process. {e}");
                Environment.Exit(1);
            }

            Environment.Exit(0);
        }

        public const string DefaultConfigurationDirectoryName = "Configs";
        public const string OutputFileName = "OutputLog.log";

        public static string DefaultConfigurationDirectoryPath
        {
            get
            {
                return Path.Combine(Environment.CurrentDirectory, DefaultConfigurationDirectoryName);
            }
        }

        public static bool DefaultConfigurationDirectoryExists
        {
            get
            {
                return Directory.Exists(DefaultConfigurationDirectoryPath);
            }
        }

        public static string OutputFilePath
        {
            get
            {
                return Path.Combine(Environment.CurrentDirectory, OutputFileName);
            }
        }
    }
}
