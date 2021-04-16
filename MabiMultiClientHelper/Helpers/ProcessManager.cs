﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        //////////////////////////////////////////////////////////////////////////////////////////////////// Import
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Private

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

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Public

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
        private Task ThrottleProcessAsync(int processID, double limitPercent, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                if (limitPercent < 0 || limitPercent > 100)
                {
                    throw new Exception("limitPercent는 0부터 100까지");
                }

                Process process = Process.GetProcessById(processID);

                string instanceName = process.ProcessName;

                PerformanceCounter counter = new PerformanceCounter("Process", "% Processor Time", instanceName);

                int interval = 100;

                double defaultUsage = 0;
                double limitUsage = 0;

                while (defaultUsage == 0)
                {
                    Thread.Sleep(100);

                    defaultUsage = counter.NextValue() / Environment.ProcessorCount;
                    limitUsage = defaultUsage * limitPercent / 100;
                }

                while (true)
                {
                    float currentUsage = counter.NextValue() / Environment.ProcessorCount;

                    if (currentUsage < limitUsage)
                    {
                        continue;
                    }

                    double suspensionTime = (currentUsage - limitUsage) / currentUsage * interval;

                    SuspendProcess(processID);

                    Thread.Sleep((int)suspensionTime);

                    ResumeProcess(processID);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        throttleProcessPool.Remove(processID);
                        return;
                    }
                }
            });
        }

        #endregion

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private static readonly List<int> throttleProcessPool = new List<int>();

        public void Start(int percent)
        {
            var cancellationToken = cancellationTokenSource.Token;

            List<Task> throttleProcessTasks = new List<Task>();
            foreach (var processID in throttleProcessPool)
            {
                throttleProcessTasks.Add(ThrottleProcessAsync(processID, percent, cancellationToken));
            }
            Task.WaitAll();
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            return;
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
    }
}