namespace MabiMultiClientHelper
{
    public class Setting
    {
        /// <summary>
        /// 활성화중인 서브 클라이언트 CPU 제한 해제
        /// </summary>
        public bool SkipWhenSubClientActivated { get; set; }

        /// <summary>
        /// 클라이언트 선호도 변경
        /// </summary>
        public bool ChangeClientAffinity { get; set; }

        /// <summary>
        /// 정지간격
        /// </summary>
        public int SuspendInterval { get; set; }
    }
}
