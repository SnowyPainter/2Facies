using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace _2Facies.RTC
{
    public class SharpRTA
    {
        private WaveIn recorder;
        private BufferedWaveProvider bufferedWaveProvider;
        private EndlessProvider endlessProvider;
        private WaveOut player;

        private Action<SampleRTA> bufferSendingHandler;
        public bool Active;

        public SharpRTA()
        {
            Active = false;
        }

        public void Start(Action<SampleRTA> bufferHandler)
        {
            bufferSendingHandler = bufferHandler;

            Active = true;
            recorder = new WaveIn();
            recorder.DataAvailable += recordDataAvailable;

            bufferedWaveProvider = new BufferedWaveProvider(recorder.WaveFormat);
            endlessProvider = new EndlessProvider(bufferedWaveProvider, new MemoryStream() );

            player = new WaveOut();
            player.Init(endlessProvider);

            player.Play();
            recorder.StartRecording();
        }
        public void Stop()
        {
            Active = false;
            recorder.StopRecording();
            player.Stop();
            endlessProvider.Dispose();
        }
        public void Keep(byte[] samples)
        {
            bufferedWaveProvider.AddSamples(samples, 0, samples.Length);
        }
        private void recordDataAvailable(object sender, WaveInEventArgs eventArgs)
        {
            bufferSendingHandler(new SampleRTA(eventArgs.Buffer, eventArgs.BytesRecorded));
        }

    }
}
