using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MabiMultiClientHelper.Helpers
{
    /// <summary>
    /// https://icodebroker.tistory.com/6570
    /// </summary>
    public class ProcessManager
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 프로세스 풀
        /// </summary>
        private static readonly ThreadSafeList<int> throttleProcessPool = new ThreadSafeList<int>();

        /// <summary>
        /// 활성화중인 클라이언트 무시
        /// </summary>
        public bool PassWhenActivate { get; set; }

        /// <summary>
        /// 정지 간격
        /// </summary>
        public int SuspendInterval { get; set; } = 100;

        #region 스레드 열기 - OpenThread(threadAccess, inheritHandle, threadID)

        /// <summary>
        /// 스레드 열기
        /// </summary>
        /// <param name="threadAccess">스레드 액세스</param>
        /// <param name="inheritHandle">상속 핸들</param>
        /// <param name="threadID">스레드 ID</param>
        /// <returns>스레드 핸들</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess threadAccess, bool inheritHandle, uint threadID);

        #endregion

        #region 스레드 실행 일시 중지하기 - SuspendThread(threadHandle)

        /// <summary>
        /// 스레드 실행 일시 중지하기
        /// </summary>
        /// <param name="threadHandle">스레드 핸들</param>
        /// <returns>처리 결과</returns>
        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr threadHandle);

        #endregion

        #region 스레드 다시 실행하기 - ResumeThread(threadHandle)

        /// <summary>
        /// 스레드 다시 실행하기
        /// </summary>
        /// <param name="threadHandle">스레드 핸들</param>
        /// <returns>처리 결과</returns>
        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr threadHandle);

        #endregion

        #region 핸들 닫기 - CloseHandle(threadHandle)

        /// <summary>
        /// 핸들 닫기
        /// </summary>
        /// <param name="threadHandle">스레드 핸들</param>
        /// <returns>처리 결과</returns>
        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(IntPtr threadHandle);

        #endregion

        #region 프로세스 실행 일시 중지하기 - SuspendProcess(processID)

        /// <summary>
        /// 프로세스 실행 일시 중지하기
        /// </summary>
        /// <param name="processID">프로세스 ID</param>
        private void SuspendProcess(int processID)
        {
            Process process = Process.GetProcessById(processID);

            if (process.ProcessName == string.Empty)
            {
                return;
            }

            foreach (ProcessThread processThread in process.Threads)
            {
                IntPtr threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)processThread.Id);

                if (threadHandle == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(threadHandle);

                CloseHandle(threadHandle);
            }
        }

        #endregion

        #region 프로세스 다시 실행하기 - ResumeProcess(processiD)

        /// <summary>
        /// 프로세스 다시 실행하기
        /// </summary>
        /// <param name="processiD">프로세스 ID</param>
        private void ResumeProcess(int processiD)
        {
            Process process = Process.GetProcessById(processiD);

            if (process.ProcessName == string.Empty)
            {
                return;
            }

            foreach (ProcessThread processThread in process.Threads)
            {
                IntPtr threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)processThread.Id);

                if (threadHandle == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;

                do
                {
                    suspendCount = ResumeThread(threadHandle);
                }
                while (suspendCount > 0);

                CloseHandle(threadHandle);
            }
        }

        #endregion

        #region 프로세스 조절하기 - ThrottleProcess(processID, limitCPURate)

        /// <summary>
        /// 프로세스 조절하기
        /// </summary>
        /// <param name="processID">프로세스 ID</param>
        /// <param name="limitPercent">제한 사용률</param>
        private void ThrottleProcessAsync(int processID, CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throttleProcessPool.Remove(processID);
                        Console.WriteLine($"Cancel {processID} => {throttleProcessPool.Count} / [{string.Join(",", throttleProcessPool.Select(x => x.ToString()))}]");
                        return;
                    }

                    if (PassWhenActivate)
                    {
                        if (WinAPI.GetActiveProcessId() == processID)
                        {

                            Thread.Sleep(100);
                            continue;
                        }
                    }

                    SuspendProcess(processID);

                    Thread.Sleep(this.SuspendInterval);

                    ResumeProcess(processID);

#warning 하드코딩;;
                    Thread.Sleep(100 - this.SuspendInterval);
                }
            }
            catch (Exception ex)
            {
                throttleProcessPool.Remove(processID);
                Console.WriteLine($"Cancel {processID} => {throttleProcessPool.Count} / [{string.Join(",", throttleProcessPool.Select(x => x.ToString()))}]");
                return;
            }
        }

        #endregion

        #region 프로세스 우선순위 조절

        public int GetProcessAffinityCount(int processID)
        {
            var foramteedAffinity = GetProcessAffinity(processID);

            return foramteedAffinity.Length;
        }

        public string GetProcessAffinity(int processID)
        {
            try
            {
                Process process = Process.GetProcessById(processID);

                if (process.ProcessName == string.Empty)
                {
                    return string.Empty;
                }

                int processorAffinity = (int)process.ProcessorAffinity;
                var foramteedAffinity = FormatAffinity(processorAffinity);

                return foramteedAffinity;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public IntPtr SetProcessAffinity(int processID, string formattedAffinity)
        {
            try
            {
                Process process = Process.GetProcessById(processID);

                if (process.ProcessName == string.Empty)
                {
                    return IntPtr.Zero;
                }

                int processorAffinity = (int)process.ProcessorAffinity;
                var foramteedAffinity = FormatAffinity(processorAffinity);

                if (foramteedAffinity.Length != formattedAffinity.Length)
                {
                    return IntPtr.Zero;
                }

                int newAffinity = Convert.ToInt32(formattedAffinity, 2);
                process.ProcessorAffinity = new IntPtr(newAffinity);

                return process.ProcessorAffinity;
            }
            catch (Exception ex)
            {
                return IntPtr.Zero;
            }
        }

        private static string FormatAffinity(int affinity)
        {
            return Convert.ToString(affinity, 2).PadLeft(Environment.ProcessorCount, '0');
        } 

        #endregion

        public async Task StartThrottling()
        {
            var cancellationToken = cancellationTokenSource.Token;

            List<Task> throttleProcessTasks = new List<Task>();
            foreach (var processID in throttleProcessPool)
            {
                var throttleTask = Task.Run(() => ThrottleProcessAsync(processID, cancellationToken));
                throttleProcessTasks.Add(throttleTask);
            }
            await Task.WhenAll(throttleProcessTasks.ToArray());

            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StopThrottling()
        {
            cancellationTokenSource.Cancel();
            while (throttleProcessPool.Count > 0)
            {
                await Task.Delay(100);
            }

            return;
        }

        public void ClearThrottleProcesses()
        {
            throttleProcessPool.Clear();
        }

        public bool AddThrottleProcess(int processId)
        {
            if (!throttleProcessPool.Contains(processId))
            {
                throttleProcessPool.Add(processId);
                return true;
            }
            return false;
        }

        public void TryResumeProcess(int pid)
        {
            ResumeProcess(pid);
        }
    }
}
