using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Camagru.Infrastructure.Persistence.Init;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DbInitializer));

            try
            {
                var appliedMigrations = (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();

                logger.LogInformation(
                    "Database initialization attempt {Attempt}/{MaxAttempts}. Applied migrations: {AppliedCount}. Pending migrations: {PendingCount}.",
                    attempt,
                    maxAttempts,
                    appliedMigrations.Length,
                    pendingMigrations.Length);

                if (pendingMigrations.Length > 0)
                {
                    logger.LogInformation("Pending migrations: {PendingMigrations}", string.Join(", ", pendingMigrations));
                }

                await context.Database.MigrateAsync(cancellationToken);

                if (!await TableExistsAsync(context, "Posts", cancellationToken))
                {
                    throw new InvalidOperationException(
                        "Database schema mismatch: table \"Posts\" is missing after migrations. " +
                        "If you are reusing an older Docker Postgres volume, reset that volume or apply migrations manually before restarting the web container.");
                }

                logger.LogInformation("Database initialization completed successfully.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientStartupFailure(ex))
            {
                lastException = ex;
                var delay = TimeSpan.FromSeconds(Math.Min(attempt * 2, 10));
                logger.LogWarning(
                    ex,
                    "Database initialization attempt {Attempt}/{MaxAttempts} failed with a transient error. Retrying in {DelaySeconds} seconds.",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialization failed during attempt {Attempt}/{MaxAttempts}.", attempt, maxAttempts);
                throw;
            }
        }

        throw new InvalidOperationException("Database initialization failed after exhausting retries.", lastException);
    }

    private static bool IsTransientStartupFailure(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            NpgsqlException => true,
            _ when exception.InnerException is not null => IsTransientStartupFailure(exception.InnerException),
            _ => false
        };
    }

    private static async Task<bool> TableExistsAsync(AppDbContext context, string tableName, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name = @tableName
                );
                """;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var scalar = await command.ExecuteScalarAsync(cancellationToken);
            return scalar is bool exists && exists;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }
}
