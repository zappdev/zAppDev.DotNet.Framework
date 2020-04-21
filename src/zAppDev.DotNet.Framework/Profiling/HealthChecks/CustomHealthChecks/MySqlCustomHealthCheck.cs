#if NETFRAMEWORK
#else

using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySql.Data.MySqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Profiling.HealthChecks.CustomHealthChecks
{
    public class MySqlCustomHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        public MySqlCustomHealthCheck(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    if (!connection.Ping())
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"The {nameof(MySqlCustomHealthCheck)} check fail.");
                    }

                    return HealthCheckResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}

#endif