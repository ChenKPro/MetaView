using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaView.Core.ModulesInterface
{
    public interface IMotionControl
    {
        /// <summary>
        ///芯明天 Motorized Mirror Mount
        /// </summary>
        /// <returns></returns>
        Task<bool> BeamSteering();//
        /// <summary>
        ///Pusi Step Motor Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> LaserPowerAdjustment();//
        /// <summary>
        ///Pusi Step Motor Driver 
        /// </summary>
        /// <returns></returns>
        Task<bool> LaserPolarizationAdjustment();//
        /// <summary>
        /// YuanRui Break Step Motor Pusi Step Motor Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> LaserPowerMeassurement_StepMotor();
        /// <summary>
        ///KaiFu Linear Step Stage with build in Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalDelayControl();//
        /// <summary>
        ///Pusi Step Motor Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalPathSwitch_BridgeSwitch();//
        /// <summary>
        ///Pusi Step Motor Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalPathSwitch_GlassRodSwitch();//
        /// <summary>
        ///Pusi Step Motor Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalPathSwitch_Up();//
        /// <summary>
        ///Pusi Step Motor Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalPathSwitch_Down();//
        /// <summary>
        ///Heidstar/Prior XYZ + T Sample Stagewith build in Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> SampleMovementXYZ();//
        /// <summary>
        ///Heidstar/Prior XYZ + T Sample Stagewith build in Driver
        /// </summary>
        /// <returns></returns>
        Task<bool> ObjectiveSwitch();//
        /// <summary>
        /// Elliptec Ell18 Rotary Motor
        /// </summary>
        /// <returns></returns>
        Task<bool> OpticalFilterControl();//
        
    }
}
