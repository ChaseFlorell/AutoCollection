using System.ComponentModel;
using Nuke.Common.Tooling;

namespace AutoCollection.Build;
/// <summary>
///     Represents build configuration options.
/// </summary>
/// <remarks>
///     This class provides predefined build configurations that can be used throughout the build process.
/// </remarks>
[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
	/// <summary>
	///     Represents the Debug configuration.
	/// </summary>
	/// <remarks>
	///     The Debug configuration typically includes debugging information and is not optimized.
	/// </remarks>
	public static readonly Configuration Debug = new() {Value = nameof(Debug), };

	/// <summary>
	///     Represents the Release configuration.
	/// </summary>
	/// <remarks>
	///     The Release configuration is typically optimized and does not include debugging information.
	/// </remarks>
	public static readonly Configuration Release = new() {Value = nameof(Release),};

	/// <summary>
	///     Implicitly converts a <see cref="Configuration" /> to a <see cref="string" />.
	/// </summary>
	/// <param name="configuration">The configuration to convert.</param>
	/// <returns>The string value of the configuration.</returns>
	public static implicit operator string(Configuration configuration) => configuration.Value;
}