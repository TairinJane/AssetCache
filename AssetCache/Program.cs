using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace AssetCache {
    internal class Program {
        public static void Main(string[] args) {
            StreamReader fs = File.OpenText("F:\\jopa\\stuff\\SampleScene\\SceneLittle.txt");
            Dictionary<ulong, object> cache = new Dictionary<ulong, object>();

            //two first lines
            Console.WriteLine(fs.ReadLine());
            Console.WriteLine(fs.ReadLine());

            string line = fs.ReadLine();

            while (line != null) {
                Console.WriteLine(line);
                var key = GetObjectFileId(line);
                string objectString = "";
                Console.WriteLine("obj Name: " + fs.ReadLine());
                while ((line = fs.ReadLine()) != null && !line.StartsWith("---")) {
                    objectString += line;
                    Console.WriteLine(line);
                }

                var objDict = ReadObject(objectString);
                
                cache.Add(key, objDict);
            }
        }

        private static ulong GetObjectFileId(string line) {
            Regex pattern = new Regex(@"--- !u!\d+ &(?<id>\d+)");
            Match match = pattern.Match(line);
            string key = match.Groups["id"].Value;
            Console.WriteLine("id = " + key);
            return UInt64.Parse(key);
        }

        private static Dictionary<string, object> ReadObject(string obj) {
            var deserializer = new DeserializerBuilder()
                .Build();
            var dict = deserializer.Deserialize<Dictionary<string, object>>(obj);
            return dict;
        }

        private void Component(Dictionary<string, object> dict) {
            var list = (List<object>) dict["m_Component"];
            var newList = new List<string>();
            foreach (var str in list) {
                Console.WriteLine(str);
                foreach (var pair in (Dictionary<object, object>) str) {
                    Console.WriteLine("  " + pair.Key + " " + pair.Value);
                    var fileId = (Dictionary<object, object>) pair.Value;
                    foreach (var id in fileId) {
                        Console.WriteLine("    " + id.Key + " " + id.Value);
                        newList.Add((string) id.Value);
                    }
                }
            }

            dict["m_Component"] = newList;
            Console.WriteLine(dict["m_Component"].GetType());
            foreach (var str in (List<string>) dict["m_Component"]) {
                Console.WriteLine("newList: " + str);
            }
        }
    }
}