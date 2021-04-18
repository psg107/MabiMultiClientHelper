﻿using GalaSoft.MvvmLight;
using MabiMultiClientHelper.Helpers;
using MabiMultiClientHelper.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MabiMultiClientHelper.ViewModels
{
    /// <summary>
    /// <seealso cref="Views.MainWindow"/>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region property

        private readonly MessageBoxHelper messageBoxHelper;
        private readonly ClientManager clientManager;
        private readonly ProcessManager processManager;

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<ClientInfo> MainClients
        {
            get => mainClients;
            set => Set(ref mainClients, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<ClientInfo> SubClients
        {
            get => subClients;
            set => Set(ref subClients, value);
        }

        /// <summary>
        /// 작동중 true, 그 외 false
        /// </summary>
        public bool Running
        {
            get => running;
            set => Set(ref running, value);
        }

        /// <summary>
        /// 작동중지중 true, 그 외 false
        /// </summary>
        public bool Stopping
        {
            get => stopping;
            set => Set(ref stopping, value);
        }

        /// <summary>
        /// true: 활성화 중에는 CPU 제한 해제, false: 작동 중에는 항상 서브 클라이언트 CPU 제한
        /// </summary>
        public bool PassWhenActivateCheckBox
        {
            get => passWhenActivateCheckBox;
            set => Set(ref passWhenActivateCheckBox, value);
        }

        private ObservableCollection<ClientInfo> mainClients;
        private ObservableCollection<ClientInfo> subClients;
        private bool running;
        private bool stopping;
        private bool passWhenActivateCheckBox;

        #endregion

        #region ctor

        public MainViewModel()
        {
            MainClients = new ObservableCollection<ClientInfo>();
            SubClients = new ObservableCollection<ClientInfo>();

            messageBoxHelper = new MessageBoxHelper(this);
            clientManager = new ClientManager();
            processManager = new ProcessManager();
        }

        #endregion

        #region command

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand LoadCommand
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    ScanCommand.Execute(null);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ScanCommand
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (Running)
                    {
                        return;
                    }

                    this.MainClients.Clear();
                    this.SubClients.Clear();

                    var clients = clientManager.Scan();
                    foreach (var client in clients)
                    {
                        SubClients.Add(client);

                        WinAPI.SetWindowText(client.Handle, client.Name);
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ActivateWindowCommand
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (parameter is ClientInfo clientInfo)
                    {
                        WinAPI.SetForegroundWindow(clientInfo.Process.MainWindowHandle);
                    }
                });
            }
        }

        /// <summary>
        /// 도우미 강제종료로 인해 클라이언트 응답없음이 된 경우 복원 시도
        /// </summary>
        public DelegateCommand RestoreClientCommand
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (parameter is ClientInfo clientInfo)
                    {
                        processManager.TryResumeProcess(clientInfo.PID);
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand StartCommand
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (Running)
                    {
                        return;
                    }

                    var clients = this.MainClients.Concat(this.SubClients);
                    foreach (var client in clients)
                    {
                        try
                        {
                            var process = Process.GetProcessById(client.PID);
                        }
                        catch (System.Exception)
                        {
                            messageBoxHelper.ShowMessage($"" +
                                $"PID : {client.PID}를 찾는 중 오류가 발생했습니다.\n" +
                                $"클라이언트를 다시 스캔합니다.");
                            ScanCommand.Execute(null);
                            return;
                        }
                    }

                    List<string> warnings = new List<string>();

                    if (this.MainClients.Count == 0)
                    {
                        warnings.Add($"메인 클라이언트가 없음");
                    }
                    //if (this.MainClients.Count > 1)
                    //{
                    //    warnings.Add($"메인 클라이언트가 두개 이상임 ({this.MainClients.Count}개)");
                    //}

                    if (warnings.Count > 0)
                    {
                        var result = messageBoxHelper.ShowQuestionMessage($"" +
                            $"아래와 같은 문제가 있습니다.\n" +
                            $"계속 실행하시겠습니까?\n" +
                            $"\n" +
                            $"- {string.Join("\n", warnings)}");

                        if (result == false)
                        {
                            return;
                        }
                    }

                    Running = true;
                    processManager.PassWhenActivateCheckBox = this.PassWhenActivateCheckBox;
                    foreach (var clientInfo in this.SubClients)
                    {
                        processManager.AddThrottleProcess(clientInfo.PID);
                    }
                    processManager.Start();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand StopCommand
        {
            get
            {
                return new DelegateCommand(async (parameter) =>
                {
                    if (!Running)
                    {
                        return;
                    }
                    if (Stopping)
                    {
                        return;
                    }

                    try
                    {
                        Stopping = true;
                        await processManager.Stop();
                    }
                    finally
                    {
                        Running = false;
                        Stopping = false;
                    }
                });
            }
        }

        #endregion
    }
}
