﻿using System;
using System.Collections.Generic;

namespace AssetCache {
    public class AssetCache : IAssetCache{
        private Dictionary<ulong, SceneObject> cache;
        
        private Dictionary<string, Dictionary<ulong, SceneObject>> index;
        public object Build(string path, Action interruptChecker) {
            throw new NotImplementedException();
        }

        public void Merge(string path, object result) {
            throw new NotImplementedException();
        }

        public int GetLocalAnchorUsages(ulong anchor) {
            throw new NotImplementedException();
        }

        public int GetGuidUsages(string guid) {
            throw new NotImplementedException();
        }

        public IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor) {
            throw new NotImplementedException();
        }
    }
}