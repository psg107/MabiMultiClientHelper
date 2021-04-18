using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MabiMultiClientHelper
{
    public class RunOneInstance
    {
        private const string APP_NAME = "MABI_MULTI_CLIENT_HELPER";

        private Mutex mutex;

        public bool CreateOnlyOneMutex(string mutexName)
        {
            bool canCreateNewMutex = false;

            // 보안 속성을 모든 사람이 사용할 수 있도록 셋팅한다.
            // 모든 사람이 접근 가능하므로 보안에 문제가 있을수 있다.
            var allowEveryoneRule = new MutexAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null), 
                MutexRights.FullControl, 
                AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            //보안 속성을 추가해서 mutex를 생성한다.
            //public Mutex(bool initiallyOwned, string name, out bool createdNew, MutexSecurity       
            //mutexSecurity);
            mutex = new Mutex(false, APP_NAME, out canCreateNewMutex, securitySettings);

            return canCreateNewMutex;
        }
    }
}
