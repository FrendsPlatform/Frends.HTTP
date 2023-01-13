using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HTTP.Request.Definitions;

/// <summary>
/// Parameter class
/// </summary>
public class Parameter
{
    /// <summary>
    /// The name of the parameter
    /// </summary>
    /// <example>ValueName</example>
    [DefaultValue("ParameterName")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; }

    /// <summary>
    /// The value of the parameter
    /// </summary>
    /// <example>123</example>
    [DefaultValue("ParameterValue")]
    [DisplayFormat(DataFormatString = "Text")]
    public dynamic Value { get; set; }
}
