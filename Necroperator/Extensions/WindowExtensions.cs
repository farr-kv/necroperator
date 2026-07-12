namespace Necroperator.Extensions
{
    internal static class WindowExtensions
    {
        public static void RunOnUiThread(this System.Windows.Window window, Action action)
        {
            if (window.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                window.Dispatcher.Invoke(action);
            }
        }
    }
}
