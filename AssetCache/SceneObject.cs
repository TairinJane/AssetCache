using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetCache {
    public class SceneObject {
        public string Hash { get; }

        private Dictionary<string, object> content;

        public SceneObject(string hash, Dictionary<string, object> content) {
            Hash = hash;
            this.content = content;
        }

        public int GetLocalAnchorUsages(ulong anchor) {
            var pattern = new Regex($@".*fileID: {anchor}\D.*");
            var count = SearchByRegex(pattern);
            if (GetComponents().Contains(anchor)) count++;
            return count;
        }

        public int GetGuidUsages(ulong guid) {
            var pattern = new Regex($@".*guid: {guid}\D.*");
            return SearchByRegex(pattern);
        }

        private int SearchByRegex(Regex pattern) {
            var count = 0;
            foreach (var value in content.Values) {
                if (value is string) {
                    Match match = pattern.Match((string) value);
                    if (match.Success) count++;
                }
            }

            return count;
        }

        public IEnumerable<string> GetKeys() {
            return content.Keys;
        }

        public void WriteContent() {
            foreach (var pair in content) {
                Console.WriteLine(pair.Key + ": " + pair.Value);
            }
        }

        public IEnumerable<ulong> GetComponents() {
            if (content.ContainsKey("m_Component")) {
                return (List<ulong>) content["m_Component"];
            }

            throw new KeyNotFoundException("No components for this object");
        }
    }
}