namespace Frends.HTTP.DownloadFile.Definitions;

/// <summary>
/// Result class
/// </summary>
public class Result
{
    /// <summary>
    /// Task complete.
    /// </summary>
    /// <example>True</example>
    public bool Success { get; set; }

    /// <summary>
    /// Path to created file.
    /// </summary>
    /// <example>C:\\temp.txt</example>
    public string FilePath { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
