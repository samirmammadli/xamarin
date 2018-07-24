using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using TasksInfo;

namespace TasksManagarCommands
{
    public interface IClientCommand
    {
        string CommandParameter { get; set; }
        object ResponseObject { get; set; }
        void ExecuteCommand(NetworkStream stream);
    }

    [Serializable]
    public class StartProcess : IClientCommand
    {
        public string CommandParameter { get; set; }
        public object ResponseObject { get; set; }

        public void ExecuteCommand(NetworkStream stream)
        {
            string result = null;
            try
            {
                result = "Success!";
                Process.Start(CommandParameter);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                if (stream.CanRead && stream.CanWrite)
                {
                    var response = new KillProcessCommand() { ResponseObject = result };
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, response);
                }
            }
        }

        public override string ToString()
        {
            return "Start Process";
        }
    }

    [Serializable]
    public class KillProcessCommand : IClientCommand
    {
        public string CommandParameter { get; set; }
        public object ResponseObject { get; set; }
        public void ExecuteCommand(NetworkStream stream)
        {
            string result = null;
            try
            {
                result = "Success!";
                var process = Process.GetProcessById(Int32.Parse(CommandParameter));
                process?.Kill();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                if (stream.CanRead && stream.CanWrite)
                {
                    var response = new KillProcessCommand() { ResponseObject = result };
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, response);
                }
            }
        }

        public override string ToString()
        {
            return "Kill Process";
        }
    }

    [Serializable]
    public class GetProcessesCommand : IClientCommand
    {
        public string CommandParameter { get; set; }
        public object ResponseObject { get; set; }
        private List<ProcessInfo> GetProcessesInfo(Process[] processes)
        {
            var ProcessesInfo = new List<ProcessInfo>();
            foreach (var item in processes)
            {
                try
                {
                    var info = new ProcessInfo(item.ProcessName, item.Id, item.NonpagedSystemMemorySize64);
                    ProcessesInfo.Add(info);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return ProcessesInfo;
        }

        public void ExecuteCommand(NetworkStream stream)
        {
            Process[] procs = null;
            List<ProcessInfo> procsInfo = null;
            var formatter = new BinaryFormatter();
            try
            {
                procs = Process.GetProcesses();
                procsInfo = GetProcessesInfo(Process.GetProcesses());
                var response = new GetProcessesCommand() { ResponseObject = procsInfo };
                formatter.Serialize(stream, response);
            }
            catch (Exception ex)
            {
                if (stream.CanRead && stream.CanWrite)
                {
                    var response = new GetProcessesCommand() { ResponseObject = ex.Message };
                    formatter.Serialize(stream, response);
                }
            }
        }
        public override string ToString()
        {
            return "Get Processess";
        }
    }
}
