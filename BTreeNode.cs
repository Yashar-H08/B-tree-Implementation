using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAMA_2
{
    class BTreeNode
    {
        public int n;
        public bool leaf;
        public Tuple<string,int>[] k;
        public int[] c;

        public BTreeNode(int t)
        {
            k = new Tuple<string,int>[2 * t + 5];
            c = new int[2 * t + 5];
        }

    }
}
