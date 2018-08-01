using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using TasksInfo;
using TasksManagarCommands;
using test;
using Xamarin.Forms;


// page.DisplayAlert("Alert", CrossConnectivity.Current.IsConnected.ToString(), "OK");
namespace RemoteTaskManager.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        MainPage page;
        public MainViewModel(MainPage getPage)
        {
            IsConnected = false;
            page = getPage;
            IpInput = "10.2.26.18";
            
        }
        static private int _port = 8005;

        private TcpClient _client;
        private NetworkStream _stream;


        private ObservableCollection<ProcessInfo> processes;
        public ObservableCollection<ProcessInfo> Processes
        {
            get => processes;
            set => Set(ref processes, value);

        }


        private ProcessInfo currentProcess;
        public ProcessInfo CurrentProcess
        {
            get => currentProcess;
            set  { Set(ref currentProcess, value); KillCommand.ChangeCanExecute(); }
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
            set
            {
                Set(ref isConnected, value);
                ConnectionAllowed = !value;
                CloseConnectionCommand.ChangeCanExecute();
                GetProcesses.ChangeCanExecute();
            }
        }

        private bool connectionAllowed;
        public bool ConnectionAllowed
        {
            get => connectionAllowed;
            set => Set(ref connectionAllowed, value);
        }



        private Command getProcesses;
        public Command GetProcesses
        {
            get
            {
                return getProcesses ?? (getProcesses = new Command(() => GetProcessesFromServer(),
                () => IsConnected));
            }
        }

        private void GetProcessesFromServer()
        {
            var cmd = new GetProcessesCommand();
            var formatter2 = new BinaryFormatter();
            if (_stream != null)
            formatter2.Serialize(_stream, cmd);
        }

        private Command killCommand;
        public Command KillCommand
        {
            get
            {
                return killCommand ?? (killCommand = new Command(() =>
                {
                    var cmd = new KillProcessCommand { CommandParameter = CurrentProcess.Id.ToString() };
                    var formatter2 = new BinaryFormatter();
                    formatter2.Serialize(_stream, cmd);
                    GetProcessesFromServer();
                }, () => IsConnected && CurrentProcess != null));
            }
        }

        private Command closeConnectionCommand;
        public Command CloseConnectionCommand
        {
            get
            {
                return closeConnectionCommand ?? (closeConnectionCommand = new Command(CloseConnection, () => IsConnected));
            }
        }

        private Command<string> startProcessCommand;
        public Command<string> StartProcessCommand
        {
            get
            {
                return startProcessCommand ?? (startProcessCommand = new Command<string>(param =>
                {
                    var cmd = new StartProcess { CommandParameter = param };
                    var formatter2 = new BinaryFormatter();
                    formatter2.Serialize(_stream, cmd);
                }, param => IsConnected));
            }
        }

        private Command<string> connectToServer;
        public Command<string> ConnectToServer
        {
            get
            {
                return connectToServer ?? (connectToServer = new Command<string>(param =>
                 Connect(param), param => ConnectionAllowed));
            }
        }

        private void CloseConnection()
        {
            //_client.Close();
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
                _client.SendTimeout = 5000;
                _client.Connect(ip, _port);
                _stream = _client.GetStream();
                IsConnected = true;
                StartListening();
                GetProcessesFromServer();
                IpInput = "Connected: " + _client.Client.LocalEndPoint.ToString().Split(':')[0];
            }
            catch (Exception ex)
            {
                page.DisplayAlert("Alert", ex.Message, "OK");
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
                        IClientCommand obj;
                        lock (this) { obj = formatter.Deserialize(_stream) as IClientCommand; };
                        if (obj != null)
                        {
                            if (obj.ResponseObject is ObservableCollection<ProcessInfo>) Processes = new ObservableCollection<ProcessInfo>((obj.ResponseObject as ObservableCollection<ProcessInfo>).OrderBy(x => x.ProcessName));
                            else if (obj.ResponseObject as string != null) page.DisplayAlert("Alert", obj.ResponseObject.ToString(), "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        CloseConnection();
                        page.DisplayAlert("Alert", ex.Message, "OK");
                        break;
                    }
                }
            });
        }
    }
}

