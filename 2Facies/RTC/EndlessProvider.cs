using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace _2Facies.RTC
{
    public class EndlessProvider: IWaveProvider, IDisposable
    {
        public WaveFormat WaveFormat { get { return waveProvider.WaveFormat; } }

        private readonly IWaveProvider waveProvider;
        private readonly WaveFileWriter waveWriter;
        private bool isWriterDisposed;

        public EndlessProvider(IWaveProvider provider, Stream waveStream)
        {
            this.waveProvider = provider;
            waveWriter = new WaveFileWriter(waveStream, provider.WaveFormat);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var read = waveProvider.Read(buffer, offset, count);
            if (count > 0 && !isWriterDisposed)
            {
                waveWriter.Write(buffer, offset, read);
            }

            if (count == 0)
                Dispose();

            return read;
        }
        public void Dispose()
        {
            if (!isWriterDisposed)
            {
                isWriterDisposed = true;
                waveWriter.Dispose();
            }
        }
    }
    
}
