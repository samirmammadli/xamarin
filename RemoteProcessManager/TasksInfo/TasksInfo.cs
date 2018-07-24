using System;

namespace TasksInfo
{
    [Serializable]
    public class ProcessInfo
    {
        public string ProcessName { get; set; }
        public int Id { get; set; }
        public long NonpagedSystemMemorySize64 { get; set; }

        public ProcessInfo(string processName, int id, long nonpagedSystemMemorySize64)
        {
            ProcessName = processName;
            Id = id;
            NonpagedSystemMemorySize64 = nonpagedSystemMemorySize64;
        }
    }
}

