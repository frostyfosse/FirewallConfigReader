using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FirewallConfigReaderCS
{
    public static class ConfigProcessor
    {
        public const string InterfaceTag = "interface";
        public const string EndInterfaceBlockTag = "!";
        public const string AddressTag = "address 10.254.247.";
        public const string SubNetTag = ".192";
        public const string LineDivider = "============================================";

        //Local $addressTag = "address 10.254.247."
        //Local $subNetTag = ".192 " ;4th octet

        public static string FilePath { get; private set; }
        public static string OutputFilePath { get; private set; }

        public static void Process(string configFilePath, string outputFilePath)
        {
            var files = Directory.GetFiles(configFilePath);
            var results = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    ProcessSingleFile(file, results);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Unable to process the file '{Path.GetFileName(file)}' for the following reasons: {e}");
                }
            }

            if (results.Any())
            {
                Debug.WriteLine($"Writing results to '{outputFilePath}'.");
                File.WriteAllText(outputFilePath, string.Join(Environment.NewLine, results));
            }
            else
                Debug.WriteLine("No conditions found to output");
        }

        static void ProcessSingleFile(string filePath, IList<string> results)
        {
            Debug.WriteLine($"Processing file '{Path.GetFileName(filePath)}'.");

            var blocks = GetInterfaceBlocks(filePath);

            if (blocks == null || !blocks.Any())
            {
                Debug.WriteLine("No conditions found.");
                return;
            }

            var header = string.Join(Environment.NewLine, LineDivider, Path.GetFileName(filePath), LineDivider);
            var result = string.Join(Environment.NewLine, header, string.Join(Environment.NewLine, blocks));

            results.Add(result);
        }

        static IEnumerable<string> GetInterfaceBlocks(string configFilePath)
        {
            var lines = GetFileData(configFilePath);
            var startingLines = lines.Where(x => x.StartsWith(InterfaceTag)).ToList();
            var completedBlocks = new List<string>();

            foreach (var interfaceLine in startingLines)
            {
                var interfaceBlockReader = new StringBuilder();
                var index = lines.IndexOf(interfaceLine);

                interfaceBlockReader.AppendLine(interfaceLine);

                foreach (var blockLine in lines.Skip(++index))
                {
                    if (string.Equals(blockLine, EndInterfaceBlockTag, StringComparison.OrdinalIgnoreCase))
                        break;

                    interfaceBlockReader.AppendLine(blockLine);
                }

                interfaceBlockReader.AppendLine(EndInterfaceBlockTag);

                var result = interfaceBlockReader.ToString();

                if (HasCondition(result))
                    completedBlocks.Add(result);
            }

            return completedBlocks;
        }

        static bool HasCondition(string interfaceBlock)
        {
            var addressSplit = interfaceBlock.Split(new[] { AddressTag }, StringSplitOptions.None);

            if (addressSplit.Count() < 2)
                return false;

            if (addressSplit.Last().Contains(SubNetTag))
                return true;
            else
                return false;
        }

        static IList<string> GetFileData(string filePath)
        {
            return File.ReadAllLines(filePath);
        }
    }
}
