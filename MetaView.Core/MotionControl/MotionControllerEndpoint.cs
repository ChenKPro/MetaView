namespace MetaView.Core.MotionControl;

/// <summary>
/// Describes one physical motion controller endpoint in the instrument.
/// </summary>
public sealed record MotionControllerEndpoint
{
    /// <summary>
    /// Gets the stable controller id used by axis bindings.
    /// </summary>
    public string Id { get; init; } = "Stage";

    /// <summary>
    /// Gets the controller type name from the foundation layer.
    /// Supported values include Pusi, Kaifull, Prior, ZMotionEthernet, E53XMT, and HeidStarGclib.
    /// </summary>
    public string ControllerType { get; init; } = "Pusi";

    /// <summary>
    /// Gets the serial port used by serial or Modbus RTU controllers.
    /// </summary>
    public string PortName { get; init; } = "COM1";

    /// <summary>
    /// Gets the serial baud rate.
    /// </summary>
    public int BaudRate { get; init; } = 9600;

    /// <summary>
    /// Gets the Modbus slave id for Pusi and Kaifull controllers.
    /// </summary>
    public byte SlaveId { get; init; } = 1;

    /// <summary>
    /// Gets the IP address used by Ethernet controllers.
    /// </summary>
    public string IpAddress { get; init; } = "192.168.0.11";

    /// <summary>
    /// Gets the connection timeout in milliseconds.
    /// </summary>
    public int TimeoutMs { get; init; } = 5000;

    /// <summary>
    /// Gets the number of physical axes on this endpoint.
    /// </summary>
    public int AxisCount { get; init; } = 1;

    /// <summary>
    /// Gets the diagram/module group this endpoint belongs to.
    /// </summary>
    public string Module { get; init; } = "Motion Control";
}
