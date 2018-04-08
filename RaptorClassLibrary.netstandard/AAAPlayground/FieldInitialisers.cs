using System;
using System.Collections.Generic;
using System.Text;

namespace RaptorClassLibrary.netstandard.AAAPlayground
{
    public class FieldInitialisers
    {
        public int bob = 42;
        public int mary = 42;

        //public FieldInitialisers(){}

        public FieldInitialisers(int _bob)
        {
            bob = _bob;
            mary = 0;
        }

        //public FieldInitialisers(int _mary, int dummy)
        //{
        //    mary = _mary;
        //}

        public void DSomething()
        {
            if (mary == 0)
            {
                bob = mary;
            }
        }
    }
}
