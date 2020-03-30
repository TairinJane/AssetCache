using System;
using System.Collections.Generic;
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
            var components = (Dictionary<string, object>) content["m_Component"];
            foreach (var component in components) {
                var dict = (Dictionary<string, string>) component.Value;
                if (dict["fileID"].Equals("" + anchor)) count++;
            }

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

        IEnumerable<ulong> GetComponents() {
            List<ulong> fileIds = new List<ulong>();
            var components = (Dictionary<string, object>) content["m_Component"];
            foreach (var component in components) {
                var dict = (Dictionary<string, string>) component.Value;
                fileIds.Add(Convert.ToUInt64(dict["fileID"]));
            }

            return fileIds;
        }
    }
}