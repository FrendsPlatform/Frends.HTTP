using Frends.HTTP.Request.Definitions;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.HTTP.Request;

/// <summary>
/// Task class.
/// </summary>
public class HTTP
{
    /// <summary>
    /// Execute a stored procedure to MySQL.
    /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.HTTP.Request)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"/>
    /// <returns>Object { int AffectedRows }</returns>
    public static async Task<Result> Request(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken
    )
    {
        return null;
    }

}
