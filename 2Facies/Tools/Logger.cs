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
        public FileInfo Path { get; private set; }

        private Action<string> logEvent;
        private FileStream stream;
        private List<string> logs;
        
        public Logger(FileInfo logFile)
        {
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
            logEvent = loggingEvent;
        }
        public void SetLogEvent(Action<string> ev) {
            logEvent = ev;
        }
        public void SaveFile(string path)
        {
            File.WriteAllLines(path, logs);
        }
        public void Clean()
        {
            logs = new List<string>();
        }
        public void Log(string log, bool fileLog = true)
        {
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
            this.logs.AddRange(logs);
        }

        public IEnumerable<string> ReadAll()
        {
            return logs;
        }
    }
}
