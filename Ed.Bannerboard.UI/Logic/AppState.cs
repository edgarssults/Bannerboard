namespace Ed.Bannerboard.UI.Logic
{
    /// <summary>
    /// Shared state for communicating between components.
    /// </summary>
    public class AppState
    {
        /// <summary>
        /// Event raised when dashboard layout should be reset.
        /// </summary>
        public event Action? OnResetLayout;

        /// <summary>
        /// Raise the dashboard layout reset event.
        /// </summary>
        public void NotifyResetLayout() => OnResetLayout?.Invoke();
    }
}
