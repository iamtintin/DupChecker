using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Hasher
{
    public class DupChecker
    {
        private const string BASE_DIR = @"Z:\Pictures";
        private const string DEL_DIR = @"Z:\Pictures\To-be Deleted\";
        private const string IGN_DIR = @"Z:\Pictures\Imports (To sort)";
        private static Dictionary<long, List<int>> countSizes(FileInfo[] files)
        {
            var fileSizes = new Dictionary<long, List<int>>();

            for (int i = 0; i < files.Length; i++)
            {
                if (fileSizes.ContainsKey(files[i].Length)) {
                    fileSizes[files[i].Length].Add(i);
                }
                else {
                    fileSizes[files[i].Length] = new List<int>() { i };
                }
            }

            return fileSizes;
        }

        private static Dictionary<string, List<int>> countHashes(List<String[]> hashList)
        {
            var hashDict = new Dictionary<string, List<int>>();

            for (int i = 0; i < hashList.Count; i++)
            {
                if (hashDict.ContainsKey(hashList[i][0])) {
                    hashDict[hashList[i][0]].Add(i);
                } else {
                    hashDict[hashList[i][0]] = new List<int>() { i };
                }
            }

            return hashDict;
        }

        private static List<String[]> removeUniqueHashes(List<String[]> hashList, Dictionary<string, List<int>> hashDict)
        {
            foreach (var hash in hashDict) {
                if (hash.Value.Count == 1) {
                    for (int i = 0; i < hashList.Count; i++) {
                        if (hashList[i][0] == hash.Key) {
                            hashList.Remove(hashList[i]);
                            break;
                        }
                    }
                }
            }

            return hashList;
        }

        private static List<int> countNonImportDuplicates(List<String[]> hashList)
        {
            List<int> nonImports = new List<int>();

            for (int i = 0; i < hashList.Count; i++)
            {
                if (!(hashList[i][1].StartsWith(IGN_DIR))) {
                    nonImports.Add(i);
                }
            }

            return nonImports;
        }
        public static void Main(string[] args)
        {
            DirectoryInfo dir = new DirectoryInfo(BASE_DIR);           
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            var dict = countSizes(files);

            foreach (var pair in dict)
            {
                int length = pair.Value.Count;
                if(pair.Value.Count > 1) {
                    List<String[]> hashArray = new List<String[]>();

                    for (int i = 0; i < length; i++)
                    {
                        string filePath = files[pair.Value[i]].FullName;
                        Hash a = new Hash(filePath);
                        hashArray.Add(new String[] { a.HashString, filePath });
                    }

                    foreach (string[] array in hashArray)
                    {
                        Console.WriteLine("Checking: {0}: {1}", array[0], array[1]);
                    }

                    var hashDict = countHashes(hashArray);
                    hashArray = removeUniqueHashes(hashArray, hashDict);

                    foreach (string[] array in hashArray) {
                        Console.WriteLine("Duplicate: {0}: {1}", array[0], array[1]);
                    }

                    if (hashArray.Count >= 2) {
                        List<int> nonImports = countNonImportDuplicates(hashArray);

                        if (nonImports.Count == 0)
                        {
                            for(int i = 1; i < hashArray.Count; i++) {
                                File.Move(hashArray[i][1], DEL_DIR + Path.GetFileName(hashArray[i][1]));
                            }
                        }
                        else if (nonImports.Count == 1)
                        {
                            for (int i = 0; i < hashArray.Count; i++) {
                                if (i != nonImports[0]) {
                                    File.Move(hashArray[i][1], DEL_DIR + Path.GetFileName(hashArray[i][1]));
                                }
                            }
                        }
                        else if (hashArray.Count > nonImports.Count)
                        {
                            for (int i = 0; i < hashArray.Count; i++) {
                                if (nonImports.IndexOf(i) == -1) {
                                    File.Move(hashArray[i][1], DEL_DIR + Path.GetFileName(hashArray[i][1]));
                                }
                            }
                            Console.WriteLine("Multiple copies in non-imports folder:");
                            foreach (int num in nonImports) {
                                Console.WriteLine("- {0}", hashArray[num][1]);
                            }
                        }
                        else if (hashArray.Count == nonImports.Count)
                        {
                            Console.WriteLine("Multiple copies in non-imports folder:");
                            foreach (string[] hash in hashArray)
                            {
                                Console.WriteLine("- {0}", hash[1]);
                            }
                        }
                    }

                    Console.WriteLine(new String('-', 50));
                }
            }
        }
    }
}
