﻿using System.Collections.Generic;

namespace UnityAssetCache {
    public class SceneObject {
        public string Type { get; set; }
        public ulong Hash { get; set; }
        public Dictionary<string, object> Content;
    }
}