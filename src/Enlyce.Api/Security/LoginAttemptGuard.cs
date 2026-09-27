using System.Collections.Concurrent;

namespace Enlyce.Api.Security;

public sealed class LoginAttemptGuard
{
    private const int MaximumFailures = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private readonly ConcurrentDictionary<string, AttemptState> _attempts = new();

    public bool CanAttempt(string email, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;
        if (!_attempts.TryGetValue(Normalize(email), out var state))
            return true;

        lock (state)
        {
            var elapsed = DateTimeOffset.UtcNow - state.StartedAt;
            if (elapsed >= Window)
            {
                _attempts.TryRemove(Normalize(email), out _);
                return true;
            }

            if (state.Failures < MaximumFailures)
                return true;

            retryAfter = Window - elapsed;
            return false;
        }
    }

    public void RegisterFailure(string email)
    {
        var key = Normalize(email);
        var state = _attempts.GetOrAdd(key, _ => new AttemptState());
        lock (state)
        {
            if (DateTimeOffset.UtcNow - state.StartedAt >= Window)
            {
                state.StartedAt = DateTimeOffset.UtcNow;
                state.Failures = 0;
            }

            state.Failures++;
        }
    }

    public void Reset(string email) => _attempts.TryRemove(Normalize(email), out _);

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private sealed class AttemptState
    {
        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
        public int Failures { get; set; }
    }
}
