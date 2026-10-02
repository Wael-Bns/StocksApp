using Microsoft.Extensions.Options;

namespace StocksApp.Infrastructure.Options
{
    public sealed class LeaderElectionOptionsValidator : IValidateOptions<LeaderElectionOptions>
    {
        public ValidateOptionsResult Validate(string? name, LeaderElectionOptions o)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(o.ConnectionString))
                errors.Add("LeaderElection:ConnectionString is required (direct Postgres connection, no pooler).");

            if (o.LockKey <= 0)
                errors.Add("LeaderElection:LockKey must be a positive number.");

            if (o.FenceAfter <= TimeSpan.Zero)
                errors.Add("LeaderElection:FenceAfter must be greater than zero.");

            if (o.HeartbeatInterval <= TimeSpan.Zero)
                errors.Add("LeaderElection:HeartbeatInterval must be greater than zero (or omit it to derive it).");

            if (o.HeartbeatTimeout <= TimeSpan.Zero)
                errors.Add("LeaderElection:HeartbeatTimeout must be greater than zero (or omit it to derive it).");

            if (o.RetryInterval <= TimeSpan.Zero)
                errors.Add("LeaderElection:RetryInterval must be greater than zero (or omit it to derive it).");

            // The one real invariant: a single slow-but-healthy heartbeat cycle must never
            // be enough to fence the leader.
            if (errors.Count == 0 && o.HeartbeatInterval + o.HeartbeatTimeout >= o.FenceAfter)
                errors.Add(
                    $"LeaderElection:FenceAfter ({o.FenceAfter}) must be greater than HeartbeatInterval + " +
                    $"HeartbeatTimeout ({o.HeartbeatInterval + o.HeartbeatTimeout}). " +
                    "Increase FenceAfter, or remove the HeartbeatInterval/HeartbeatTimeout overrides.");

            return errors.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(errors);
        }
    }
}