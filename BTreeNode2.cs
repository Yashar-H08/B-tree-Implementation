using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAMA_2
{
    class BTreeNode2
    {
        public int n;
        public bool leaf;
        public Tuple<string, List<int> >[] k;
        private const int defaultNodeSize = 100; // Customizable
        public int[] c;

        public BTreeNode2(int t)
        {
            k = new Tuple<string, List<int> >[2 * t + 5];
            c = new int[2 * t + 5];
        }

    }
}
