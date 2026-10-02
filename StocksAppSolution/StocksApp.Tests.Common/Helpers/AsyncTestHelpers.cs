using Moq;

namespace StocksApp.Tests.Common
{
    public static class AsyncTestHelpers
    {
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);

        public static async Task WaitUntilAsync(Func<bool> condition, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);
            while (!condition())
            {
                if (DateTime.UtcNow > deadline)
                    throw new TimeoutException("Condition was not met in time.");
                await Task.Delay(10);
            }
        }

        public static int CountInvocations<T>(Mock<T> mock, string methodName) where T : class =>
            mock.Invocations.Count(i => i.Method.Name == methodName);
    }
}