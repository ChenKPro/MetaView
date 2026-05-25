using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MetaView.Core.ModulesInterface
{
    public interface IDaqAndPreprocessing
    {
        /// <summary>
        ///Thorlabs Galvo X/Y （Sunny Galvo X/Y ）
        /// </summary>
        /// <returns></returns>
        Task<bool> LaserScanning();

        /// <summary>
        ///Shutter with build in TTL Drv
        /// </summary>
        /// <returns></returns>
        Task<bool> ShutterControl();

        /// <summary>
        ///Shutter with build in TTL Drv
        /// </summary>
        /// <returns></returns>
        Task<bool> FilterEncoder();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalSignalDetection_RAPD();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalSignalDetection_PMT();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalSignalDetection_SiPM();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Task<bool> ElectricalSignalDetection_XXX();
    }
}
