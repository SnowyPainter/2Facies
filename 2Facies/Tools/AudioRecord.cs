using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Facies
{
    public class AudioRecord
    {
        private void waveMaxSample(WaveInEventArgs e, Action<float> handler)
        {
            if (waveWriter != null)
            {
                waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
                waveWriter.Flush();
            }
            float maxRecorded = 0.0f;
            for (int i = 0;i < e.BytesRecorded;i+=2)
            {
                short sample = (short)((e.Buffer[i + 1] << 8) |
                                e.Buffer[i + 0]);
                var sample32 = sample / 32768f;

                if (sample32 < 0) sample32 = -sample32;
                if (sample32 > maxRecorded) maxRecorded = sample32;
            }

            handler(maxRecorded);
        }
        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveWriter != null)
            {
                waveWriter.Close();
                waveWriter = null;
            }
        }

        private WaveIn wave;
        private WaveFileWriter waveWriter;
        private Stream memoryStream;

        public AudioRecord(int device, int sampleRate, int channels, Action<float> dataAvailableHandler)
        {
            int waveInDevices = WaveIn.DeviceCount;
            if(waveInDevices < 1)
            {
                throw new Exception("there's no connectable devices in computer : AudioRecord constructor");
            }

            wave = new WaveIn();
            wave.DeviceNumber = device;
            wave.DataAvailable += (sender, e) =>
            {
                waveMaxSample(e, dataAvailableHandler);
            };
            wave.RecordingStopped += OnRecordingStopped;
            
            wave.WaveFormat = new WaveFormat(sampleRate, channels);
            
        }

        public void Start()
        {
            memoryStream = new MemoryStream();
            waveWriter = new WaveFileWriter(new IgnoreDisposeStream(memoryStream), wave.WaveFormat);
            wave.StartRecording();
        }
        public Stream Stop()
        {
            if (waveWriter == null || memoryStream == null) return null;

            wave.StopRecording();

            return memoryStream;
        }
    }
}
