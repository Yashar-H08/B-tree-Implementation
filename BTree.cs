using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAMA_2
{
    class BTree
    {
        private const int t = 3, maxSize = 10000, lastposp = 10; //Customizable
        //private int root, lastpos;
        private string fileName, rootFileName;
        //private BTreeNode[] nodes;

        private int root()
        {
            return Convert.ToInt32(Filem.rpos(rootFileName, 0, maxSize));
        }

        private void wroot(int x)
        {
            string x2 = Convert.ToString(x);
            Filem.wpos(rootFileName, 0, x2);
        }

        private int lastpos()
        {
            return Convert.ToInt32(Filem.rpos(rootFileName, lastposp, maxSize));
        }

        private void wlastpos(int x)
        {
            string x2 = Convert.ToString(x);
            Filem.wpos(rootFileName, lastposp, x2);
        }

        public BTree(string fn)
        {
            fileName = fn;
            rootFileName = "root" + fn;
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
                BTreeNode r = new BTreeNode(t);
                r.leaf = true;
                r.n = 0;
                wpos(1, r);
            }
            if (!File.Exists(rootFileName))
            {
                File.Create(rootFileName).Close();// root = 1; lastpos = 2;
                wroot(1); wlastpos(2);
            }
            //nodes = new BTreeNode[maxNodes];
            //for (int i = 0; i < maxNodes; i++) nodes[i] = null;
            //nodes[1] = r; //DISK-WRITE
        }

        private int search(int x, string k)
        {
            int i = 1;
            BTreeNode X = rpos(x);
            while (i <= X.n && String.Compare(X.k[i].Item1, k) < 0) i++;
            if (i <= X.n && String.Compare(k, X.k[i].Item1) == 0) return X.k[i].Item2; //returns value
            if (X.leaf) return -1; //not found
            else return search(X.c[i], k); //DISK-READ
        }

        public int search(string k)
        {
            BTreeNode roo = rpos(root());
            if (roo == null || roo.n == 0) return -1; //not found
            return search(root(), k);
        }

        private void split(int X, int i, int Y)
        {
            BTreeNode x = rpos(X);
            BTreeNode y = rpos(Y);
            BTreeNode z = new BTreeNode(t);
            z.leaf = y.leaf;
            z.n = t - 1;
            for (int j = 1; j <= t - 1; j++) z.k[j] = y.k[t + j];
            if (!y.leaf)
            {
                for (int j = 1; j <= t; j++) z.c[j] = y.c[t + j];
            }
            y.n = t - 1;
            for (int j = x.n + 1; j >= i + 1; j--) x.c[j + 1] = x.c[j];
            x.c[i + 1] = lastpos();
            for (int j = x.n; j >= i; j--) x.k[j + 1] = x.k[j];
            x.k[i] = y.k[t];
            x.n++;
            wpos(Y, y);
            //nodes[Y] = y; //DISK-WRITE
            //nodes[lastpos] = z;
            wpos(lastpos(), z);
            wlastpos(lastpos() + 1); //DISK-WRITE
            wpos(X, x);
            //nodes[X] = x; //DISK-WRITE
        }

        private void insertNonFull(int X, string k, int v)
        {
            BTreeNode x = rpos(X);
            int i = x.n;
            if (x.leaf)
            {
                while (i >= 1 && String.Compare(k, x.k[i].Item1) < 0)
                {
                    x.k[i + 1] = x.k[i];
                    i--;
                }
                x.k[i + 1] = new Tuple<string, int>(k, v);
                x.n++;
                wpos(X, x);
                //nodes[X] = x; //DISK-WRITE
            }
            else
            {
                while (i >= 1 && String.Compare(k, x.k[i].Item1) < 0) i--;
                i++;
                BTreeNode ci = rpos(x.c[i]); //DISK-READ
                if (ci.n == 2 * t - 1)
                {
                    split(X, i, x.c[i]);
                    if (String.Compare(k, rpos(X).k[i].Item1) > 0) i++;
                }
                insertNonFull(rpos(X).c[i], k, v);
            }
        }

        public void insert(string k, int v)
        {
            int r = root();
            //Filem.show("here is r:"+ Convert.ToString(r));
            if (rpos(r) != null && rpos(r).n == 2 * t - 1)
            {
                BTreeNode s = new BTreeNode(t);
                s.leaf = false;
                s.n = 0;
                s.c[1] = r;
                wroot(lastpos());
                //root = lastpos;
                wpos(lastpos(), s);

                //nodes[lastpos] = s;
                //lastpos++;
                wlastpos(lastpos() + 1);
                split(root(), 1, r);
                insertNonFull(root(), k, v);
            }
            else insertNonFull(r, k, v);
        }

        private Tuple<string, int> findGreatest(int x)
        {
            BTreeNode X = rpos(x); //DISK-READ
            if (X.leaf) return X.k[X.n];
            return findGreatest(X.c[X.n + 1]);
        }

        private Tuple<string, int> findLeast(int x)
        {
            BTreeNode X = rpos(x); //DISK-READ
            if (X.leaf) return X.k[1];
            return findLeast(X.c[1]);
        }

        private void delete(int x, string k)
        {
            if (rpos(x) == null) x = root();
            int ishere = 0;
            for (int i = 1; i <= rpos(x).n; i++)
            {
                if (rpos(x).k[i].Item1 == k) { ishere = i; break; }
            }
            if (ishere != 0)
            {
                if (rpos(x).leaf)
                {
                    BTreeNode X = rpos(x); //DISK-READ
                    for (int i = ishere; i < X.n; i++) X.k[i] = X.k[i + 1];
                    X.n--;
                    wpos(x, X);
                    //nodes[x] = X; //DISK-WRITE
                }
                else
                {
                    if (rpos(rpos(x).c[ishere]).n >= t)
                    //if (nodes[nodes[x].c[ishere]].n >= t)
                    {
                        Tuple<string, int> g = findGreatest(rpos(x).c[ishere]);
                        delete(x, g.Item1);///////////////////////////////////////////////////////
                        BTreeNode X = rpos(x); //DISK-READ
                        X.k[ishere] = g;
                        wpos(x, X);
                        //nodes[x] = X; //DISK-WRITE
                    }
                    else if (rpos(rpos(x).c[ishere + 1]).n >= t)
                    //else if (nodes[nodes[x].c[ishere + 1]].n >= t)
                    {
                        Tuple<string, int> g = findLeast(rpos(x).c[ishere + 1]);
                        delete(x, g.Item1);///////////////////////////////////////////////////
                        BTreeNode X = rpos(x); //DISK-READ
                        X.k[ishere] = g;
                        wpos(x, X);
                        //nodes[x] = X; //DISK-WRITE
                    }
                    else
                    {
                        BTreeNode X = rpos(x); //DISK-READ
                        BTreeNode y = rpos(rpos(x).c[ishere]); //DISK-READ
                        BTreeNode z = rpos(rpos(x).c[ishere + 1]); //DISK-READ

                        y.k[y.n + 1] = new Tuple<string, int>(k, 0); y.n++;
                        for (int i = 1, j = y.n + 1; i <= z.n; i++, j++) { y.k[j] = z.k[i]; }
                        for (int i = 1, j = y.n + 1; i <= z.n + 1; i++, j++) { y.c[j] = z.c[i]; }
                        y.n += z.n;
                        wpos(rpos(x).c[ishere], y);
                        //nodes[nodes[x].c[ishere]] = y; //DISK-WRITE
                        wpos(rpos(x).c[ishere + 1], null);
                        //nodes[nodes[x].c[ishere + 1]] = null; //FREE z
                        for (int i = ishere; i < X.n; i++) X.k[i] = X.k[i + 1]; //FREE v
                        for (int i = ishere + 1; i < X.n + 1; i++) X.c[i] = X.c[i + 1]; //FREE pointer to z
                        X.n--;

                        ////////////////////////////////////////////////////////////////////////
                        if (x == root() && X.n == 0) { wroot(X.c[1]); wpos(x, null); delete(root(), k); } //DISK-WRITE
                        else { wpos(x, X); delete(x, k); } //DISK-WRITE //////////////////////////////////////////////////
                                                           ////////////////////////////////////////////////////////////////////////

                    }
                }
            }
            else
            {
                if (rpos(x).leaf) return; //not found at all
                int i = 1;
                while (i <= rpos(x).n && String.Compare(k, rpos(x).k[i].Item1) > 0) i++;
                if (rpos(rpos(x).c[i]).n == t - 1)
                //if (nodes[nodes[x].c[i]].n == t - 1)
                {
                    if (i > 1 && rpos(rpos(x).c[i - 1]).n >= t)
                    //if (i > 1 && nodes[nodes[x].c[i - 1]].n >= t)
                    {
                        BTreeNode X = rpos(x); //DISK-READ
                        BTreeNode sib = rpos(rpos(x).c[i - 1]);
                        //BTreeNode sib = nodes[nodes[x].c[i - 1]]; //DISK-READ
                        BTreeNode Y = rpos(rpos(x).c[i]);
                        //BTreeNode Y = nodes[nodes[x].c[i]]; //DISK-READ

                        Tuple<string, int> g = X.k[i - 1];
                        for (int j = Y.n + 1; j > 1; j--) Y.k[j] = Y.k[j - 1];
                        Y.k[1] = g;
                        g = sib.k[sib.n];
                        X.k[i - 1] = g;
                        for (int j = Y.n + 2; j > 1; j--) Y.c[j] = Y.c[j - 1];
                        Y.c[1] = sib.c[sib.n + 1];//////////////////////////////////////////////////
                        sib.n--;
                        Y.n++;

                        wpos(x, X);
                        //nodes[x] = X; //DISK-WRITE
                        wpos(rpos(x).c[i - 1], sib);
                        //nodes[nodes[x].c[i - 1]] = sib; //DISK-WRITE
                        wpos(rpos(x).c[i], Y);
                        //nodes[nodes[x].c[i]] = Y; //DISK-WRITE
                        delete(rpos(x).c[i], k);
                    }
                    else if (i < rpos(x).n + 1 && rpos(rpos(x).c[i + 1]).n >= t)
                    //else if (i < nodes[x].n + 1 && nodes[nodes[x].c[i + 1]].n >= t)
                    {
                        BTreeNode X = rpos(x); //DISK-READ
                        BTreeNode sib = rpos(rpos(x).c[i + 1]);
                        //BTreeNode sib = nodes[nodes[x].c[i + 1]]; //DISK-READ
                        //BTreeNode Y = nodes[nodes[x].c[i]]; //DISK-READ
                        BTreeNode Y = rpos(rpos(x).c[i]);

                        Tuple<string, int> g = X.k[i];
                        Y.k[Y.n + 1] = g;
                        g = sib.k[1];
                        X.k[i] = g;
                        Y.c[Y.n + 2] = sib.c[1];
                        for (int j = 1; j < sib.n; j++) sib.k[j] = sib.k[j + 1];
                        for (int j = 1; j < sib.n + 1; j++) sib.c[j] = sib.c[j + 1];
                        sib.n--;
                        Y.n++;

                        //nodes[x] = X; //DISK-WRITE
                        wpos(x, X);
                        //nodes[nodes[x].c[i + 1]] = sib; //DISK-WRITE
                        wpos(rpos(x).c[i + 1], sib);
                        //nodes[nodes[x].c[i]] = Y; //DISK-WRITE
                        wpos(rpos(x).c[i], Y);
                        //delete(nodes[x].c[i], k);
                        delete(rpos(x).c[i], k);
                    }
                    else
                    {
                        int tmp = rpos(x).c[i];
                        if (i > 1)
                        {
                            BTreeNode X = rpos(x); //DISK-READ
                            //BTreeNode sib = nodes[nodes[x].c[i - 1]]; //DISK-READ
                            BTreeNode sib = rpos(rpos(x).c[i - 1]);
                            //BTreeNode Y = nodes[nodes[x].c[i]]; //DISK-READ
                            BTreeNode Y = rpos(rpos(x).c[i]);

                            Tuple<string, int> g = X.k[i - 1];
                            sib.k[sib.n + 1] = g; sib.n++;
                            for (int j = 1; j <= Y.n; j++) sib.k[sib.n + j] = Y.k[j];
                            for (int j = 1; j <= Y.n + 1; j++) sib.c[sib.n + j] = Y.c[j];
                            sib.n += Y.n;

                            //nodes[nodes[x].c[i - 1]] = sib; //DISK-WRITE
                            wpos(rpos(x).c[i - 1], sib);
                            //nodes[nodes[x].c[i]] = null; //FREE y
                            wpos(rpos(x).c[i], null);

                            for (int j = i - 1; j < X.n; j++) X.k[j] = X.k[j + 1]; //FREE g
                            for (int j = i; j < X.n + 1; j++) X.c[j] = X.c[j + 1]; //FREE y
                            X.n--;


                            ////////////////////////////////////////////////////////////////////////
                            if (x == root() && X.n == 0) { wroot(X.c[1]); wpos(x, null); } //DISK-WRITE
                            else wpos(x, X); //DISK-WRITE
                            ////////////////////////////////////////////////////////////////////////

                        }
                        else if (i < rpos(x).n + 1)
                        {
                            BTreeNode X = rpos(x); //DISK-READ
                            //BTreeNode sib = nodes[nodes[x].c[i + 1]]; //DISK-READ
                            BTreeNode sib = rpos(rpos(x).c[i + 1]);
                            //BTreeNode Y = nodes[nodes[x].c[i]]; //DISK-READ
                            BTreeNode Y = rpos(rpos(x).c[i]);

                            Tuple<string, int> g = X.k[i];
                            Y.k[Y.n + 1] = g; Y.n++;
                            for (int j = 1; j <= sib.n; j++) Y.k[Y.n + j] = sib.k[j];
                            for (int j = 1; j <= sib.n + 1; j++) Y.c[Y.n + j] = sib.c[j];
                            Y.n += sib.n;

                            //nodes[nodes[x].c[i + 1]] = null; //FREE sib
                            wpos(rpos(x).c[i + 1], null);
                            //nodes[nodes[x].c[i]] = Y; //DISK-WRITE
                            wpos(rpos(x).c[i], Y);

                            for (int j = i; j < X.n; j++) X.k[j] = X.k[j + 1]; //FREE g
                            for (int j = i + 1; j < X.n + 1; j++) X.c[j] = X.c[j + 1]; //FREE sib
                            X.n--;

                            ////////////////////////////////////////////////////////////////////////
                            if (x == root() && X.n == 0) { wroot(X.c[1]); wpos(x, null); } //DISK-WRITE
                            else wpos(x, X); //DISK-WRITE
                            ////////////////////////////////////////////////////////////////////////

                        }
                        delete(tmp, k);
                    }
                    //delete(x, k);
                }
                else delete(rpos(x).c[i], k);
            }
        }

        public void delete(string k)
        {
            if (rpos(root()) == null || rpos(root()).n == 0) return; //no values at all
            delete(root(), k);
        }

        private int[] upd;
        public void update() //delets nulls
        {
            upd = new int[lastpos() + 5];
            for (int i = 0; i < lastpos() + 5; i++) upd[i] = 0;

            int mx = 0;
            for (int i = 1; i < lastpos(); i++)
            {
                upd[i] = upd[i - 1];
                if (rpos(i) == null) upd[i]++;
                mx = Math.Max(mx, upd[i]);
            }

            for (int i = 1; i < lastpos(); i++)
            {
                if (rpos(i) == null) continue;
                BTreeNode x = rpos(i); //DISK-READ
                //nodes[i - upd[i]] = x; //DISK-WRITE
                wpos(i - upd[i], x);
            }

            //lastpos -= mx;
            wlastpos(lastpos() - mx);

            for (int i = 1; i < lastpos(); i++)
            {
                BTreeNode x = rpos(i); //DISK-READ
                for (int j = 1; j <= x.n; j++)
                {
                    x.c[j] -= upd[x.c[j]];
                }
                //nodes[i] = x; //DISK-WRITE
                wpos(i, x);
            }
        }

        private const char seperator = '|', ender = '@'; //Customizable
        private void wpos(int pos, BTreeNode b)
        {
            string what = nodeToString(b);
            Filem.wpos(fileName, (pos - 1) * maxSize, what);
        }

        private BTreeNode rpos(int pos)
        {
            return stringToNode(Filem.rpos(fileName, (pos - 1) * maxSize, maxSize) + '@');
        }

        public static string nodeToString(BTreeNode b)
        {
            if (b == null) return "" + seperator + ender;
            string res = "";
            res += Convert.ToString(b.n); res += seperator;
            if (b.leaf) res += '1';
            else res += '0';
            res += seperator;
            for (int i = 1; i <= b.n; i++) res += (b.k[i].Item1 + seperator + b.k[i].Item2 + seperator);
            for (int i = 1; i <= b.n + 1; i++) res += (Convert.ToString(b.c[i]) + seperator);
            res += ender;
            return res;
        }

        public static BTreeNode stringToNode(string s)
        {
            if (s == null || s[0] == seperator || s[0] == ender) return null;
            BTreeNode b = new BTreeNode(t);
            string tmp = "";
            int i = 0;
            while (i < s.Length && s[i] != ender && s[i] != seperator)
            {
                tmp += s[i]; i++;
            }
            i++;
            b.n = Convert.ToInt32(tmp);
            int tmp2 = Convert.ToInt32("" + s[i]); i += 2;
            if (tmp2 == 0) b.leaf = false;
            else b.leaf = true;
            for (int j = 1; j <= b.n; j++)
            {
                tmp = "";
                while (i < s.Length && s[i] != ender && s[i] != seperator)
                {
                    tmp += s[i]; i++;
                }
                i++;

                string tmp3 = "";
                while (i < s.Length && s[i] != ender && s[i] != seperator)
                {
                    tmp3 += s[i]; i++;
                }
                i++;
                b.k[j] = new Tuple<string, int>(tmp, Convert.ToInt32(tmp3));
            }
            for (int j = 1; j <= b.n + 1; j++)
            {
                tmp = "";
                while (i < s.Length && s[i] != ender && s[i] != seperator)
                {
                    tmp += s[i]; i++;
                }
                i++;
                b.c[j] = Convert.ToInt32(tmp);
            }
            return b;
        }

        public void traverse() //for testing (BFS)
        {
            Console.Write("root: " + root() + '\n');
            int r = root();
            Queue<int> q = new Queue<int>();
            q.Enqueue(r);
            while (q.Count() != 0)
            {
                int x = q.Dequeue();
                if (rpos(x) == null) { Console.Write("null\n"); continue; }
                Console.Write("Keys of node " + x + ":\n");
                for (int i = 1; i <= rpos(x).n; i++)
                {
                    Console.Write(rpos(x).k[i] + ", ");
                }
                Console.Write('\n');
                for (int i = 1; i <= rpos(x).n + 1; i++) q.Enqueue(rpos(x).c[i]);
            }
        }

        public void traverse2() //for testing
        {
            Console.Write("root: " + root() + '\n');
            for (int i = 1; i < lastpos(); i++)
            {
                if (rpos(i) == null) { Console.Write("null\n"); continue; }
                Console.Write("Keys of node " + i + ":\n");
                for (int j = 1; j <= rpos(i).n; j++) Console.Write(rpos(i).k[j] + ", ");
                Console.Write('\n');
            }
        }

    }
}
