using System;
using System.Collections.Generic;
using System.Text;

namespace _2Facies.RTC
{
    public class SampleRTA
    {
        public byte[] Sample { get; private set; }
        public int Size { get; private set; }

        public SampleRTA(byte[] sample, int sampleSize)
        {
            Sample = sample;
            Size = sampleSize;
        }
    }
}
