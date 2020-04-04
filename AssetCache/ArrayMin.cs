using System;
using System.Collections.Generic;

namespace AssetCache {
    public class ArrayMin {
        public static void MainArr(string[] args) {
            var k = Convert.ToInt32(Console.ReadLine());

            for (int i = 0; i < k; i++) {
                var line = Console.ReadLine().Split();
                var a = Convert.ToInt32(line[0]);
                var b = Convert.ToInt32(line[1]);
                Console.WriteLine("a[{0}, {1}] = {2}", a, b, GetIntByIndex(a, b));
            }
        }

        private static int GetIntByIndex(int i, int j) {
            var placeI = i % 8;
            var placeJ = j % 8;

            var leftCorner = i - placeI;
            i = leftCorner;

            while (i > 7) {
                i /= 8;
                i %= 8;
            }

            while (j > 7) {
                j /= 8;
                j %= 8;
            }

            leftCorner += 8 * (GetIntFromPlaceMatrix(i, j) - GetIntFromPlaceMatrix(i, 0));

            return leftCorner + GetIntFromPlaceMatrix(placeI, placeJ);
        }

        private static int GetIntFromPlaceMatrix(int i, int j) {
            i %= 8;
            j %= 8;
            int[,] placeMatrix = {
                {0, 1, 2, 3},
                {1, 0, 3, 2},
                {2, 3, 0, 1},
                {3, 2, 1, 0}
            };
            if ((i > 3 || j > 3) && !(i > 3 && j > 3)) {
                return placeMatrix[i % 4, j % 4] + 4;
            }

            return placeMatrix[i % 4, j % 4];
        }

        public void WriteMatrix(int n) {
            int[,] arr = new int[n, n];

            for (int i = 0; i < n; i++) {
                for (int j = 0; j < n; j++) {
                    var rowSet = new HashSet<int>();
                    var colSet = new HashSet<int>();
                    for (int k = 0; k < j; k++) {
                        rowSet.Add(arr[i, k]);
                    }

                    for (int k = 0; k < i; k++) {
                        colSet.Add(arr[k, j]);
                    }

                    rowSet.UnionWith(colSet);
                    var min = 0;
                    while (rowSet.Contains(min)) {
                        min++;
                    }

                    arr[i, j] = min;
                }
            }

            for (int i = 0; i < n; i++) {
                // Console.Write("Row {0}: ", i);
                for (int j = 0; j < n; j++) {
                    Console.Write("{0,-3}", arr[i, j]);
                }

                Console.WriteLine();
            }
        }
    }
}