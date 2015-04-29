using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadAndRun
{
    internal class PatternClass
    {
        private static readonly bool hasInitialized;

        static PatternClass()
        {
            if (hasInitialized)
                return;

            hasInitialized = true;
        }
    }
}