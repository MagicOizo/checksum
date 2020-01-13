using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace checksum
{
    class Program
    {
        enum SearchMode { String, File, Directory }
        enum HashAlgorithms { md5, sha1, sha265, sha384, sha512}

        static int Main(string[] args)
        {
            bool printHelp = true;
            bool printHeader = true;
            string currentDirectory = System.Environment.CurrentDirectory;
            List<KeyValuePair<SearchMode, string>> hashCommands = new List<KeyValuePair<SearchMode, string>>();
            HashAlgorithms selectedHash = HashAlgorithms.sha265;

            if (args.Length == 1)
            {
                if (args[0] == "-?" || args[0] == "--help")
                {
                    PrintHelpMessage();
                    return 0;
                }
                if (args[0] == "-v" || args[0] == "--version")
                {
                    Console.WriteLine("checksum for Windows by Max Zoeller - Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                    return 0;
                }
            }
            // Evaluate Parameters, change parameters, save hash commands
            if (args.Length > 0)
            {
                for (int thisArg = 0; thisArg < args.Length; thisArg++)
                {

                    if (args[thisArg] == "-h" || args[thisArg] == "--hash")
                    {
                        if (args.Length <= thisArg + 1)
                        {
                            PrintHelpMessage("Hash-Algorythm not selected.");
                            return 1;
                        }
                        switch (args[++thisArg].ToLower())
                        {
                            case "md5":
                                selectedHash = HashAlgorithms.md5;
                                break;
                            case "sha1":
                                selectedHash = HashAlgorithms.sha1;
                                break;
                            case "sha256":
                                selectedHash = HashAlgorithms.sha265;
                                break;
                            case "sha384":
                                selectedHash = HashAlgorithms.sha384;
                                break;
                            case "sha512":
                                selectedHash = HashAlgorithms.sha512;
                                break;
                            default:
                                PrintHelpMessage("Unknown hash algorithm selected.");
                                return 1;
                        }
                    }

                    if (args[thisArg] == "-n" || args[thisArg] == "--no-header")
                    {
                        printHeader = false;
                        continue;
                    }
                    if (args[thisArg] == "-s" || args[thisArg] == "--string")
                    {
                        if (args.Length <= thisArg + 1)
                        {
                            PrintHelpMessage("String to be hashed is missing.");
                            return 1;
                        }
                        hashCommands.Add(new KeyValuePair<SearchMode, string>(SearchMode.String, args[++thisArg]));
                        continue;
                    }
                    if (args[thisArg] == "-f" || args[thisArg] == "--file" || args[thisArg] == "-d" || args[thisArg] == "--directory")
                    {
                        SearchMode searchMode = SearchMode.File;
                        if (args[thisArg] == "-d" || args[thisArg] == "--directory")
                        {
                            searchMode = SearchMode.Directory;
                        }

                        if (args.Length <= thisArg + 1)
                        {
                            PrintHelpMessage($"Path to {searchMode.ToString().ToLower()} is not provided.");
                            return 1;
                        }
                        hashCommands.Add(new KeyValuePair<SearchMode, string>(searchMode, args[++thisArg]));
                        continue;
                    }
                }
            }
            foreach (KeyValuePair<SearchMode, string> hashCommand in hashCommands)
            {
                if (hashCommand.Key == SearchMode.String)
                {
                    string outputHash = GetStringHash(selectedHash, hashCommand.Value);
                    PrintHeader(printHeader);
                    Console.WriteLine($"{outputHash} : \"{hashCommand.Value}\"");
                    printHelp = false;
                    continue;
                }
                else
                {
                    string originalPattern = hashCommand.Value;

                    // Get directory and file parts of complete relative pattern
                    string[] files;
                    if (hashCommand.Key == SearchMode.File)
                    {
                        // Get directory and file parts of complete relative pattern
                        string pattern = Path.GetFileName(originalPattern);
                        string relDir = originalPattern.Substring(0, originalPattern.Length - pattern.Length);
                        // Get absolute path (root+relative)
                        string absPath = Path.GetFullPath(Path.Combine(currentDirectory, relDir));
                        // Search files mathing the pattern
                        files = Directory.GetFiles(absPath, pattern, SearchOption.TopDirectoryOnly);
                    }
                    else
                    {
                        // Get directory part of complete relative pattern
                        string pattern = originalPattern;
                        // Get absolute path (root+relative)
                        string absPath = Path.GetFullPath(Path.Combine(currentDirectory, pattern));
                        if (!Directory.Exists(absPath))
                        {
                            PrintHelpMessage($"Directory \"{absPath}\" not found.");
                            return 1;
                        }
                        // Search files mathing the pattern
                        files = Directory.GetFiles(absPath);
                    }
                    if (files.Length == 0)
                    {
                        PrintHelpMessage("File(s) not found.");
                        return 1;
                    }
                    PrintHeader(printHeader);
                    foreach (string file in files)
                    {
                        FileInfo inputFile = new FileInfo(file);
                        string outputHash = GetFileHash(selectedHash, inputFile);
                        Console.WriteLine($"{outputHash} : {inputFile.Name}");
                    }
                    printHelp = false;
                    continue;
                }
            }
            if (printHelp)
            {
                PrintHelpMessage();
            }
            return 0;
        }

        private static void PrintHeader(bool printHeader)
        {
            if (printHeader)
            {
                Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine("\\\\\r\n\\\\ checksum for Windows by Max Zoeller - Version " + curVersion.Major + "." + curVersion.Minor + "\r\n\\\\\r\n");
            }
        }

        private static void PrintHelpMessage()
        {
            PrintHelpMessage(null);
        }
        private static void PrintHelpMessage(string errorMsg)
        {
            PrintHeader(true);
            if (errorMsg != null)
            {
                Console.WriteLine(errorMsg + "\r\n");
            }
            Console.WriteLine("Usage:   checksum.exe <Parameter>\r\n");
            Console.WriteLine("Parameter:\r\n");
            Console.WriteLine("     -d <dir>    --directory <dir>  Hashes all files in the given <dir> with the selected hash algorithm\r\n");
            Console.WriteLine("     -f <file>   --string <file>    Hashes the given <file> with the selected hash algorithm");
            Console.WriteLine("                                    Wildcards and relative pathes allowed\r\n");
            Console.WriteLine("     -h <value>  --hash <value>     Sets the hash algorithm (defaults to sha256)");
            Console.WriteLine("                                    valid values are: md5, sha1, sha256, sha384, sha512\r\n");
            Console.WriteLine("     -n          --no-header        No output of programm header with output\r\n");
            Console.WriteLine("     -s <input>  --string <input>   Hashes the given <input>-string with the selected hash algorithm\r\n");
            Console.WriteLine("     -v          --version          Prints the full version of this application\r\n");
            Console.WriteLine("     -?          --help             Prints this help message\r\n");
        }

        private static string GetStringHash(HashAlgorithms hashAlgorithmSelected, string input)
        {
            HashAlgorithm hashAlgorithm;
            switch(hashAlgorithmSelected)
            {
                case HashAlgorithms.md5:
                    hashAlgorithm = MD5.Create();
                    break;
                case HashAlgorithms.sha1:
                    hashAlgorithm = SHA1.Create();
                    break;
                case HashAlgorithms.sha384:
                    hashAlgorithm = SHA384.Create();
                    break;
                case HashAlgorithms.sha512:
                    hashAlgorithm = SHA512.Create();
                    break;
                default:
                    hashAlgorithm = SHA256.Create();
                    break;
            }
            try
            {

                // Convert the input string to a byte array and compute the hash.
                byte[] hashValue = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < hashValue.Length; i++)
                {
                    sBuilder.Append(hashValue[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
            finally
            {
                hashAlgorithm.Dispose();
            }
        }

        private static string GetFileHash(HashAlgorithms hashAlgorithmSelected, FileInfo inputFile)
        {
            HashAlgorithm hashAlgorithm;
            switch (hashAlgorithmSelected)
            {
                case HashAlgorithms.md5:
                    hashAlgorithm = MD5.Create();
                    break;
                case HashAlgorithms.sha1:
                    hashAlgorithm = SHA1.Create();
                    break;
                case HashAlgorithms.sha384:
                    hashAlgorithm = SHA384.Create();
                    break;
                case HashAlgorithms.sha512:
                    hashAlgorithm = SHA512.Create();
                    break;
                default:
                    hashAlgorithm = SHA256.Create();
                    break;
            }
            try
            {
                // Create a fileStream for the file.
                FileStream fileStream = inputFile.Open(FileMode.Open, FileAccess.Read);
                // Be sure it's positioned to the beginning of the stream.
                fileStream.Position = 0;
                // Compute the hash of the fileStream.
                byte[] hashValue = hashAlgorithm.ComputeHash(fileStream);
                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < hashValue.Length; i++)
                {
                    sBuilder.Append(hashValue[i].ToString("x2"));
                }

                // Close the file.
                fileStream.Close();

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
            catch (Exception e)
            {
                return e.GetType().Name;
            }
            finally
            {
                hashAlgorithm.Dispose();
            }
        }
    }
}
