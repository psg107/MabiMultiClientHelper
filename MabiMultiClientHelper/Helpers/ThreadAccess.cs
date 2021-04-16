using System;

namespace MabiMultiClientHelper.Helpers
{
    /// <summary>
    /// 스레드 액세스
    /// </summary>
    [Flags]
    public enum ThreadAccess : int
    {
        /// <summary>
        /// TERMINATE
        /// </summary>
        TERMINATE = (0x0001),

        /// <summary>
        /// SUSPEND_RESUME
        /// </summary>
        SUSPEND_RESUME = (0x0002),

        /// <summary>
        /// GET_CONTEXT
        /// </summary>
        GET_CONTEXT = (0x0008),

        /// <summary>
        /// SET_CONTEXT
        /// </summary>
        SET_CONTEXT = (0x0010),

        /// <summary>
        /// SET_INFORMATION
        /// </summary>
        SET_INFORMATION = (0x0020),

        /// <summary>
        /// QUERY_INFORMATION
        /// </summary>
        QUERY_INFORMATION = (0x0040),

        /// <summary>
        /// SET_THREAD_TOKEN
        /// </summary>
        SET_THREAD_TOKEN = (0x0080),

        /// <summary>
        /// IMPERSONATE
        /// </summary>
        IMPERSONATE = (0x0100),

        /// <summary>
        /// DIRECT_IMPERSONATION
        /// </summary>
        DIRECT_IMPERSONATION = (0x0200)
    }
}
