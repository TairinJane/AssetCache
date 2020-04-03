using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace AssetCache {
    internal class Program {
        public static void Main(string[] args) {
            var path = "F:\\jopa\\stuff\\SampleScene\\SampleScene.unity";
            
            AssetCache assetCache = new AssetCache();
            var result = assetCache.Build(path, () => throw new OperationCanceledException());
            assetCache.Merge(path, result);
            
            Console.WriteLine("guid 8a53381b50169634491102a6508752e1: " +
                              assetCache.GetGuidUsages("8a53381b50169634491102a6508752e1"));
            Console.WriteLine("components for 17640:");
            foreach (var component in assetCache.GetComponentsFor(17640)) {
                Console.WriteLine("comp: " + component);
            }

            Console.WriteLine("local anchor 241: " + assetCache.GetLocalAnchorUsages(241));
            Console.WriteLine("END");
        }

        private static string GetHash(string input) {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        private static void WriteIdAndHash(Dictionary<ulong, SceneObject> dict) {
            foreach (var pair in dict) {
                Console.WriteLine(pair.Key + " " + pair.Value.Hash);
            }
        }

        private static ulong GetObjectFileId(string line) {
            var pattern = new Regex(@"--- !u!\d+ &(?<id>\d+)");
            var match = pattern.Match(line);
            if (match.Success) {
                var key = match.Groups["id"].Value;
                return Convert.ToUInt64(key);
            }

            throw new ArgumentException("No id in string: " + line);
        }

        private static Dictionary<string, object> ReadObject(string obj) {
            var deserializer = new DeserializerBuilder()
                .Build();
            var dict = deserializer.Deserialize<Dictionary<string, object>>(obj);
            return dict;
        }
    }
}