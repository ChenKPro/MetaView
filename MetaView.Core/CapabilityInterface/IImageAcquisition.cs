using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaView.Core.ModulesInterface
{
    public interface IImageAcquisition
    {
        /// <summary>
        /// Flsorescence Camera
        /// </summary>
        /// <returns></returns>
        Task<bool> WideFieldFluorescenceImaging‌();

        /// <summary>
        /// Flsorescence Camera
        /// </summary>
        /// <returns></returns>
        Task<bool> FlsorescenceilluminationControl();

        /// <summary>
        /// Hikrobot Camera
        /// </summary>
        /// <returns></returns>
        Task<bool> BrightFieldImaging();

        /// <summary>
        /// Hikrobot Camera
        /// </summary>
        /// <returns></returns>
        Task<bool> BrightFieldIlluminationControl();
    }
}
