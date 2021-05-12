using GalaSoft.MvvmLight;
using MabiMultiClientHelper.Helpers;
using MabiMultiClientHelper.Models;
using MabiMultiClientHelper.Services;
using MabiMultiClientHelper.Views;
using Newtonsoft.Json;
using Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace MabiMultiClientHelper.ViewModels
{
    /// <summary>
    /// <seealso cref="Views.MainWindow"/>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region property

        private readonly MessageBoxService messageBoxService;
        private readonly PopupWindowService popupWindowService;
        private readonly MabinogiClientManager clientManager;
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
        /// 
        /// </summary>
        public ClientInfo SelectedMainClient 
        {
            get => selectedMainClient;
            set => Set(ref selectedMainClient, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public ClientInfo SelectedSubClient 
        {
            get => selectedSubClient;
            set => Set(ref selectedSubClient, value);
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
        public bool SkipWhenSubClientActivated
        {
            get => skipWhenSubClientActivated;
            set => Set(ref skipWhenSubClientActivated, value);
        }

        /// <summary>
        /// cpu선호도 설정
        /// </summary>
        public bool ChangeClientAffinity
        {
            get => changeClientAffinity;
            set => Set(ref changeClientAffinity, value);
        }

        /// <summary>
        /// 정지간격 (ms, 값 높을수록 느려짐)
        /// </summary>
        public int SuspendInterval
        {
            get => suspendInterval;
            set
            {
                Set(ref suspendInterval, value);

                if (this.processManager != null)
                {
                    this.processManager.SuspendInterval = value;
                }
            }
        }

        private ObservableCollection<ClientInfo> mainClients;
        private ObservableCollection<ClientInfo> subClients;
        private ClientInfo selectedMainClient;
        private ClientInfo selectedSubClient;
        private bool running;
        private bool stopping;
        private bool skipWhenSubClientActivated;
        private bool changeClientAffinity;
        private int suspendInterval;

        #endregion

        #region ctor

        public MainViewModel()
        {
            MainClients = new ObservableCollection<ClientInfo>();
            SubClients = new ObservableCollection<ClientInfo>();

            messageBoxService = new MessageBoxService(this);
            popupWindowService = new PopupWindowService(this);
            clientManager = new MabinogiClientManager();
            processManager = new ProcessManager();

            var setting = SettingManager.Instance.LoadSetting();

            this.SkipWhenSubClientActivated = setting.SkipWhenSubClientActivated;
            this.ChangeClientAffinity = setting.ChangeClientAffinity;
            this.SuspendInterval = setting.SuspendInterval;
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
                return new DelegateCommand(() =>
                {
                    ScanCommand.Execute();
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
                return new DelegateCommand(() =>
                {
                    if (Running)
                    {
                        return;
                    }

                    ShowAllClientCommand.Execute();

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
        public DelegateCommand<ClientInfo> ActivateWindowCommand
        {
            get
            {
                return new DelegateCommand<ClientInfo>((clientInfo) =>
                {
                    if (clientInfo.IsHiddenWindow)
                    {
                        WinAPI.ShowWindow(clientInfo.Handle, WinAPI.SW_SHOW);
                        clientInfo.IsHiddenWindow = false;
                    }
                    WinAPI.SetForegroundWindow(clientInfo.Handle);
                    //WinAPI.ForceForegroundWindow(clientInfo.Process.MainWindowHandle);
                });
            }
        }

        /// <summary>
        /// 도우미 강제종료로 인해 클라이언트 응답없음이 된 경우 복원 시도
        /// </summary>
        public DelegateCommand RestoreAllClientCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (Running)
                    {
                        return;
                    }

                    var result = messageBoxService.ShowQuestionMessage("클라이언트 응답없음 복원을 시도하시겠습니까?");
                    if (result)
                    {
                        var clients = this.MainClients.Concat(this.SubClients);
                        foreach (var client in clients)
                        {
                            processManager.TryResumeProcess(client.PID);
                        }

                        messageBoxService.ShowMessage("복원 시도 완료");
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
                return new DelegateCommand(async () =>
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
                            messageBoxService.ShowMessage($"" +
                                $"PID : {client.PID}를 찾는 중 오류가 발생했습니다.\n" +
                                $"클라이언트를 다시 스캔합니다.");
                            ScanCommand.Execute();
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
                        var result = messageBoxService.ShowQuestionMessage($"" +
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

                    //프로세스 선호도 설정
                    if (this.ChangeClientAffinity)
                    {
                        var firstProcessID = this.MainClients.Concat(this.SubClients).First().PID;
                        var affinity = processManager.GetProcessAffinity(firstProcessID);
                        var affinityCount = processManager.GetProcessAffinityCount(firstProcessID);
                        var subClientAffinityCount = 2;

                        var mainAffinity = new string('0', subClientAffinityCount) + new string('1', affinityCount - subClientAffinityCount);
                        var subAffinity = new string('1', subClientAffinityCount) + new string('0', affinityCount - subClientAffinityCount);

                        foreach (var clientInfo in this.MainClients)
                        {
                            processManager.SetProcessAffinity(clientInfo.PID, mainAffinity);
                        }
                        foreach (var clientInfo in this.SubClients)
                        {
                            processManager.SetProcessAffinity(clientInfo.PID, subAffinity);
                        }
                    }

                    //프로세스 cpu 제한
                    processManager.SkipWhenActivate = this.skipWhenSubClientActivated;
                    processManager.ClearThrottleProcesses();
                    foreach (var clientInfo in this.SubClients)
                    {
                        processManager.AddThrottleProcess(clientInfo.PID);
                    }
                    processManager.SuspendInterval = this.SuspendInterval;
                    await processManager.StartThrottling();
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
                return new DelegateCommand(async () =>
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

                        //프로세스 선호도 설정
                        if (this.ChangeClientAffinity)
                        {
                            var firstProcessID = this.MainClients.Concat(this.SubClients).First().PID;
                            var affinity = processManager.GetProcessAffinity(firstProcessID);
                            var affinityCount = processManager.GetProcessAffinityCount(firstProcessID);

                            var defaultAffinity = new string('1', affinityCount);

                            foreach (var clientInfo in this.MainClients)
                            {
                                processManager.SetProcessAffinity(clientInfo.PID, defaultAffinity);
                            }
                            foreach (var clientInfo in this.SubClients)
                            {
                                processManager.SetProcessAffinity(clientInfo.PID, defaultAffinity);
                            }
                        }
                        await processManager.StopThrottling();
                    }
                    finally
                    {
                        Running = false;
                        Stopping = false;
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand<CancelEventArgs> CloseAppCommand
        {
            get
            {
                return new DelegateCommand<CancelEventArgs>((e) =>
                {
                    Keylogger.UninstallHook();

                    if (this.Running)
                    {
                        StopCommand.Execute();
                        e.Cancel = true;
                    }
                    else
                    {
                        var clients = this.MainClients.Concat(this.SubClients);
                        foreach (var client in clients)
                        {
                            WinAPI.SetWindowText(client.Handle, "마비노기");
                            WinAPI.ShowWindow(client.Handle, WinAPI.SW_SHOW);
                        }

                        var setting = SettingManager.Instance.LoadSetting();
                        setting.ChangeClientAffinity = this.ChangeClientAffinity;
                        setting.SkipWhenSubClientActivated = this.SkipWhenSubClientActivated;
                        setting.SuspendInterval = this.SuspendInterval;
                        SettingManager.Instance.SaveSetting(setting);
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand<ClientInfo> HideWindowCommand
        {
            get
            {
                return new DelegateCommand<ClientInfo>((clientInfo) =>
                {
                    if (clientInfo.IsHiddenWindow)
                    {
                        return;
                    }

                    WinAPI.ShowWindow(clientInfo.Handle, WinAPI.SW_HIDE);
                    clientInfo.IsHiddenWindow = true;
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand<ClientInfo> ShowWindowCommand
        {
            get
            {
                return new DelegateCommand<ClientInfo>((clientInfo) =>
                {
                    if (clientInfo.IsHiddenWindow == false)
                    {
                        return;
                    }

                    WinAPI.ShowWindow(clientInfo.Handle, WinAPI.SW_SHOW);
                    clientInfo.IsHiddenWindow = false;
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand HideAllClientCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var clients = this.SubClients;
                    foreach (var client in clients)
                    {
                        WinAPI.ShowWindow(client.Handle, WinAPI.SW_HIDE);
                        client.IsHiddenWindow = true;
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ShowAllClientCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var clients = this.MainClients.Concat(this.SubClients);
                    foreach (var client in clients)
                    {
                        WinAPI.ShowWindow(client.Handle, WinAPI.SW_SHOW);
                        client.IsHiddenWindow = false;
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ShowSettingWindowCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    popupWindowService.ShowPopup<SettingWindow>();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ActiveNextMainClientCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (SelectedMainClient == null)
                    {
                        SelectedMainClient = this.MainClients.FirstOrDefault();
                    }
                    if (SelectedMainClient == null)
                    {
                        return;
                    }

                    var activePID = WinAPI.GetActiveProcessId();
                    var idx = this.MainClients.IndexOf(SelectedMainClient);

                    if (SelectedMainClient.PID == activePID)
                    {
                        idx = (idx + 1) % this.MainClients.Count;
                    }

                    var clientInfo = this.MainClients[idx];
                    SelectedMainClient = clientInfo;

                    FocusHelper.ActivateMultiClientHelper();
                    Thread.Sleep(100);
                    this.ActivateWindowCommand.Execute(clientInfo);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ActiveNextSubClientCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (SelectedSubClient == null)
                    {
                        SelectedSubClient = this.SubClients.FirstOrDefault();
                    }
                    if (SelectedSubClient == null)
                    {
                        return;
                    }

                    var activePID = WinAPI.GetActiveProcessId();
                    var idx = this.SubClients.IndexOf(SelectedSubClient);

                    if (SelectedSubClient.PID == activePID)
                    {
                        idx = (idx + 1) % this.SubClients.Count;
                    }

                    var clientInfo = this.SubClients[idx];
                    SelectedSubClient = clientInfo;

                    FocusHelper.ActivateMultiClientHelper();
                    Thread.Sleep(100);
                    this.ActivateWindowCommand.Execute(clientInfo);
                });
            }
        }

        #endregion
    }
}
