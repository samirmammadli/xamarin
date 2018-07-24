using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using TasksInfo;
using TasksManagarCommands;

namespace RemoteTaskManager.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            IsConnected = false;
            IpInput = "salam bliat";
        }
        static private int _port = 8005;

        private TcpClient _client;
        private NetworkStream _stream;


        private List<ProcessInfo> processes;
        public List<ProcessInfo> Processes
        {
            get => processes;
            set => Set(ref processes, value);
           
        }


        private ProcessInfo currentProcess;
        public ProcessInfo CurrentProcess
        {
            get => currentProcess;
            set => Set(ref currentProcess, value);
        }

        private string ipInput;
        public string IpInput
        {
            get => ipInput;
            set => Set(ref ipInput, value);
        }


        private bool isConnected;
        public bool IsConnected
        {
            get => isConnected;
            set { Set(ref isConnected, value); ConnectionAllowed = !value; }
        }

        private bool connectionAllowed;
        public bool ConnectionAllowed
        {
            get => connectionAllowed;
            set => Set(ref connectionAllowed, value);
        }



        private RelayCommand getProcesses;
        public RelayCommand GetProcesses
        {
            get
            {
                return getProcesses ?? (getProcesses = new RelayCommand(() => GetProcessesFromServer(),
                () => IsConnected));
            }
        }

        private void GetProcessesFromServer()
        {
            var cmd = new GetProcessesCommand();
            var formatter2 = new BinaryFormatter();
            formatter2.Serialize(_stream, cmd);
        }

        private RelayCommand killCommand;
        public RelayCommand KillCommand
        {
            get
            {
                return killCommand ?? (killCommand = new RelayCommand(() =>
                {
                    var cmd = new KillProcessCommand { CommandParameter = CurrentProcess.Id.ToString() };
                    var formatter2 = new BinaryFormatter();
                    formatter2.Serialize(_stream, cmd);
                    GetProcessesFromServer();
                }, () => IsConnected && CurrentProcess != null));
            }
        }

        private RelayCommand closeConnectionCommand;
        public RelayCommand CloseConnectionCommand
        {
            get
            {
                return closeConnectionCommand ?? (closeConnectionCommand = new RelayCommand(() =>
                {
                    CloseConnection();
                }, () => IsConnected));
            }
        }

        private RelayCommand<string> startProcessCommand;
        public RelayCommand<string> StartProcessCommand
        {
            get
            {
                return startProcessCommand ?? (startProcessCommand = new RelayCommand<string>(param =>
                {
                    var cmd = new StartProcess { CommandParameter = param };
                    var formatter2 = new BinaryFormatter();
                    formatter2.Serialize(_stream, cmd);
                }, param => IsConnected));
            }
        }

        private RelayCommand<string> connectToServer;
        public RelayCommand<string> ConnectToServer
        {
            get
            {
                return connectToServer ?? (connectToServer = new RelayCommand<string>(param =>
                Connect(param), param => !IsConnected));
            }
        }

        private void CloseConnection()
        {
            _client.Close();
            IsConnected = false;
            Processes = null;
            IpInput = string.Empty;
        }

        private void Connect(string ip)
        {
            try
            {
                if (_client != null && _client.Client != null) CloseConnection();
                _client = new TcpClient();
                _client.Connect(ip, _port);
                _stream = _client.GetStream();
                IsConnected = true;
                StartListening();
                GetProcessesFromServer();
                IpInput = "Connected: " + _client.Client.LocalEndPoint.ToString().Split(':')[0];
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void StartListening()
        {
            var formatter = new BinaryFormatter();
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var obj = formatter.Deserialize(_stream) as IClientCommand;
                        if (obj != null)

                        {
                            if (obj.ResponseObject is List<ProcessInfo>) Processes = (obj.ResponseObject as List<ProcessInfo>).OrderBy(x => x.ProcessName).ToList();
                            //else if (obj.ResponseObject is string) MessageBox.Show(obj.ResponseObject.ToString());
                        }
                    }
                    catch (Exception)
                    {
                        CloseConnection();
                        //MessageBox.Show("Disconnected!");
                        break;
                    }
                }
            });
        }
    }
}

