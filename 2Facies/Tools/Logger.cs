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
        public bool FileLog { get; private set; }

        private FileStream stream;

        private List<string> logs;
        
        public Logger()
        {
            logs = new List<string>();
            FileLog = false;
        }
        public Logger(FileInfo logFile):this()
        {
            var fullName = logFile.FullName;
            FileLog = true;
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
        public void SaveFile(string path)
        {
            File.WriteAllLines(path, logs);
        }
        public void Clean()
        {
            logs = new List<string>();
        }
        public void Log(string log, bool fileLog = false)
        {
            var normalizedLog = $"{DateTime.Now} {log}";
            logs.Add(normalizedLog);
            
            if(FileLog && fileLog)
            {
                var fslog = normalizedLog + Environment.NewLine;
                stream.Write(Encoding.UTF8.GetBytes(fslog), 0, Encoding.UTF8.GetByteCount(fslog));
                stream.Flush();
            }
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
