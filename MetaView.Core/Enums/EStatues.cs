using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaView.Core.Enums
{
    /// <summary>
    /// AppMainState
    /// </summary>
    public enum EnumAppMainState
    {
        Standby,
        Stoping,
        Acquiring,
        Initializing,
        Loading,
        Exporting,
        Abnormal,
        CalculateTime
    }

    /// <summary>
    /// 任务运行状态
    /// </summary>
    public enum EnumTaskStatus
    {
        Run,
        Stoping,
        Stoped,
    }
}
