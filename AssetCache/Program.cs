using System;

namespace AssetCache {
    internal class Program {
        public static void Main(string[] args) {
            var path = "F:\\SampleScene\\SceneLittle.txt";
            var path2 = "F:\\SampleScene\\SceneLittle2.txt";

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

            result = assetCache.Build(path2, () => throw new OperationCanceledException());
            assetCache.Merge(path2, result);

            Console.WriteLine("END");
        }
    }
}