using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Facies
{
    public class Logger
    {
        bool STOPALL = true;

        public FileInfo Path { get; private set; }

        private Action<string> logEvent;
        private FileStream stream;
        private List<string> logs;
        
        public Logger(FileInfo logFile)
        {
            if (STOPALL) return;

            logs = new List<string>();
            logEvent = null;
            var fullName = logFile.FullName;

            if(logFile.Exists)
            {
                logs = File.ReadAllLines(fullName).ToList();

                stream = new FileStream(fullName, FileMode.Open);
            }
            else
            {
                stream = File.Create(fullName);
            }
        }
        public Logger(FileInfo logFile, Action<string> loggingEvent):this(logFile)
        {
            if (STOPALL) return;

            logEvent = loggingEvent;
        }
        public void SetLogEvent(Action<string> ev) {
            if (STOPALL) return;
            logEvent = ev;
        }
        public void SaveFile(string path)
        {
            if (STOPALL) return;
            File.WriteAllLines(path, logs);
        }
        public void Clean()
        {
            if (STOPALL) return;
            logs = new List<string>();
        }
        public void Log(string log, bool fileLog = true)
        {
            if (STOPALL) return;

            var normalizedLog = $"{DateTime.Now} {log}";
            logs.Add(normalizedLog);
            
            if(fileLog)
            {
                var fslog = normalizedLog + Environment.NewLine;
                stream.Write(Encoding.UTF8.GetBytes(fslog), 0, Encoding.UTF8.GetByteCount(fslog));
                stream.Flush();
            }

            logEvent?.Invoke(log);
        }
        public void LogAll(IEnumerable<string> logs)
        {
            if (STOPALL) return;

            this.logs.AddRange(logs);
        }

        public IEnumerable<string> ReadAll()
        {
            if (STOPALL) return null;

            return logs;
        }
    }
}
