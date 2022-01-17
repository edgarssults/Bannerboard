namespace Ed.Bannerboard.UI.Logic
{
    public interface IWidget
    {
        /// <summary>
        /// Updates the widget.
        /// </summary>
        /// <param name="model">JSON model received from the server.</param>
        Task Update(string model);

        /// <summary>
        /// Determines whether the model can be used by the widget for an update.
        /// </summary>
        /// <param name="model">JSON model received from the server.</param>
        /// <param name="version">Mod version the model was received from.</param>
        bool CanUpdate(string model, Version? version);
    }
}
