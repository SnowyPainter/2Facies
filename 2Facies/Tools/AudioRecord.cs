using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

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
            for (int i = 0; i < e.BytesRecorded; i += 2)
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

        private WaveInEvent wave;
        private WaveFileWriter waveWriter;
        private Stream memoryStream;
        private DispatcherTimer timer;

        public bool Interrupted { get; private set; }

        private int tickCount;
        private float tickInterval;

        public AudioRecord(int device, int sampleRate, int channels, Action<float> dataAvailableHandler)
        {
            int waveInDevices = WaveIn.DeviceCount;
            if (waveInDevices < 1)
            {
                throw new Exception("there's no connectable devices in computer : AudioRecord constructor");
            }

            wave = new WaveInEvent();

            wave.DeviceNumber = device;
            wave.DataAvailable += (sender, e) =>
            {
                waveMaxSample(e, dataAvailableHandler);
            };
            wave.RecordingStopped += OnRecordingStopped;

            wave.WaveFormat = new WaveFormat(sampleRate, channels);


        }

        public void SetTimer(float interval, Action<object, int> tickEvent = null)
        {
            timer = new DispatcherTimer();
            tickInterval = interval;
            timer.Interval = TimeSpan.FromSeconds(tickInterval);
            timer.Tick += (sender, e) =>
            {
                tickEvent(sender, tickCount);
                tickCount++;
            };
        }

        public void Start()
        {
            memoryStream = new MemoryStream();
            waveWriter = new WaveFileWriter(new IgnoreDisposeStream(memoryStream), wave.WaveFormat);
            Interrupted = false;
            tickCount = 0;

            wave.StartRecording();

            if (timer != null)
                timer.Start();
        }
        public void InterruptRecording()
        {
            //Console.WriteLine($"Interrupted, Writer null ? {waveWriter == null}");
            if (waveWriter == null || memoryStream == null) return;

            Interrupted = true;
            wave.StopRecording();

            if (timer != null)
                timer.Stop();
        }
        public Stream Stop()
        {
            /*if(!Interrupted)
                Console.WriteLine($"Audio Rec End, Writer null ? {waveWriter == null}");*/
            if ((!Interrupted && waveWriter == null) || memoryStream == null)
            {
                return null;
            }

            if (!Interrupted)
            {
                wave.StopRecording();
                if (timer != null)
                    timer.Stop();
            }

            Interrupted = false;
            tickCount = 0;

            return memoryStream;
        }
    }
}
