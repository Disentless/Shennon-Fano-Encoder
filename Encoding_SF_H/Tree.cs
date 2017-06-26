using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encoding_SF_H
{
    public class Tree
    {
        public Tree Parent, Left, Right;
        public string Signal;
        public float Value;

        public Tree(string signal = null)
        {
            Signal = signal;
        }
    }
}
