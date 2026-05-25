using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaView.Core.ModulesInterface
{
    internal interface ILaserSource
    {
        /// <summary>
        /// Hikrobot Camera
        /// </summary>
        /// <returns></returns>
        Task<bool> LaserPowerMeassurement_LaserPowerMeter();
    }
}
