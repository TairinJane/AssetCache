﻿using System;
using System.Collections.Generic;

namespace AssetCache {
    public class Cache {
        private Dictionary<ulong, SceneObject> cache;

        public Cache(Dictionary<ulong, SceneObject> cache) {
            this.cache = cache;
        }

        public int GetLocalAnchorUsages(ulong anchor) {
            var count = 0;
            foreach (var obj in cache.Values) {
                count += obj.GetLocalAnchorUsages(anchor);
            }

            return count;
        }

        public int GetGuidUsages(string guid) {
            var count = 0;
            var guidLong = Convert.ToUInt64(guid, 16);
            foreach (var obj in cache.Values) {
                count += obj.GetLocalAnchorUsages(guidLong);
            }

            return count;
        }

        public IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor) {
            if (cache.ContainsKey(gameObjectAnchor)) {
                return cache[gameObjectAnchor].GetComponents();
            }

            throw new KeyNotFoundException("No GameObject with id = " + gameObjectAnchor);
        }
    }
}