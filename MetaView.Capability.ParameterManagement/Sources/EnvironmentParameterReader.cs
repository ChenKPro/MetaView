namespace MetaView.Capability.ParameterManagement.Sources;

/// <summary>
/// Reads typed parameter values from process environment variables.
/// </summary>
internal sealed class EnvironmentParameterReader
{
    /// <summary>
    /// Gets a string environment value.
    /// </summary>
    public string GetString(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    /// <summary>
    /// Gets an integer environment value.
    /// </summary>
    public int GetInt32(string name, int defaultValue)
    {
        return int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets a byte environment value.
    /// </summary>
    public byte GetByte(string name, byte defaultValue)
    {
        return byte.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets an unsigned integer environment value.
    /// </summary>
    public uint? GetUInt32(string name, uint defaultValue)
    {
        return uint.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets a single-precision floating-point environment value.
    /// </summary>
    public float? GetSingle(string name, float defaultValue)
    {
        return float.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets a double-precision floating-point environment value.
    /// </summary>
    public double GetDouble(string name, double defaultValue)
    {
        return double.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets a Boolean environment value.
    /// </summary>
    public bool GetBoolean(string name, bool defaultValue)
    {
        return bool.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;
    }
}
