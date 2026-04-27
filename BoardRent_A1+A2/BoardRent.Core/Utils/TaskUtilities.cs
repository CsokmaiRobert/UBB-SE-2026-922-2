namespace BoardRent.Utils
{
    using System;
    using System.Threading.Tasks;

    public static class TaskUtilities
    {
        public static async void FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
            }
        }
    }
}