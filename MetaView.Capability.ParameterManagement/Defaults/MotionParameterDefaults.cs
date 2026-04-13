using MetaView.Capability.ParameterManagement.Sources;
using MetaView.Core.MotionControl;

namespace MetaView.Capability.ParameterManagement.Defaults;

/// <summary>
/// Creates motion system configuration from the configured parameter source.
/// </summary>
internal static class MotionParameterDefaults
{
    /// <summary>
    /// Creates the motion system configuration.
    /// </summary>
    public static MotionSystemConfiguration CreateSystem(EnvironmentParameterReader reader)
    {
        var controllerType = reader.GetString("METAVIEW_MOTION_CONTROLLER_TYPE", "Demo");
        if (string.Equals(controllerType, "Demo", StringComparison.OrdinalIgnoreCase))
        {
            return new MotionSystemConfiguration { UseDemo = true };
        }

        var endpoint = new MotionControllerEndpoint
        {
            Id = "SampleStage",
            ControllerType = controllerType,
            PortName = reader.GetString("METAVIEW_MOTION_PORT", "COM1"),
            BaudRate = reader.GetInt32("METAVIEW_MOTION_BAUD_RATE", 9600),
            SlaveId = reader.GetByte("METAVIEW_MOTION_SLAVE_ID", 1),
            IpAddress = reader.GetString("METAVIEW_MOTION_IP", "192.168.0.11"),
            TimeoutMs = reader.GetInt32("METAVIEW_MOTION_TIMEOUT_MS", 5000),
            AxisCount = reader.GetInt32("METAVIEW_MOTION_AXIS_COUNT", 3),
            Module = "Microscope Module / Sample Movement"
        };

        return new MotionSystemConfiguration
        {
            UseDemo = false,
            Controllers = [endpoint],
            AxisBindings =
            [
                new MotionAxisBinding { Axis = MotionAxis.X, ControllerId = endpoint.Id, PhysicalAxisIndex = 0, Function = "Sample Movement X" },
                new MotionAxisBinding { Axis = MotionAxis.Y, ControllerId = endpoint.Id, PhysicalAxisIndex = 1, Function = "Sample Movement Y" },
                new MotionAxisBinding { Axis = MotionAxis.Z, ControllerId = endpoint.Id, PhysicalAxisIndex = 2, Function = "Sample Movement Z" }
            ]
        };
    }
}
