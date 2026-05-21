namespace BoardRentAndProperty.Utilities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    public static class TaskUtilities
    {
        [SuppressMessage(
            "Design",
            "CA1031:Do not catch general exception types",
            Justification = "Fire-and-forget tasks must not crash the app thread.")]
        public static async void FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
            }
        }
    }
}
