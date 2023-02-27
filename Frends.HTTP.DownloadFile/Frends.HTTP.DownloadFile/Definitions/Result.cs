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
    public bool Success { get; private set; }

    /// <summary>
    /// Path to created file.
    /// </summary>
    /// <example>C:\\temp.txt</example>
    public string FilePath { get; private set; }

    internal Result(bool success, string filePath)
    {
        Success = success;
        FilePath = filePath;
    }
}