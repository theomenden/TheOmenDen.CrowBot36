using Microsoft.Data.SqlClient;
using TheOmenDen.Shared.Responses;

namespace TheOmenDen.Crowbot36.UI.Server.Middleware;

public static class OptionsDelegates
{
    public static void UpdateApiErrorResponse(HttpContext context, Exception exception, OperationOutcome operationOutcome)
    {
        if (exception.GetType().Name.Equals(nameof(SqlException), StringComparison.OrdinalIgnoreCase))
        {
            operationOutcome.ClientErrorPayload.Message += $"{Environment.NewLine}The exception was a database exception";
        }
    }

    public static LogLevel DetermineLogLevel(Exception exception) =>
        exception.Message.StartsWith("cannot open database", StringComparison.OrdinalIgnoreCase)
        || exception.Message.StartsWith("a network-related", StringComparison.OrdinalIgnoreCase)
        ? LogLevel.Critical
        : LogLevel.Error;
}
