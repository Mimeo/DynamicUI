namespace Mimeo.DynamicUI
{
    public enum DateDisplayMode
    {
        /// <summary>
        /// The date will be displayed without any conversion
        /// </summary>
        Raw = 0,

        /// <summary>
        /// The date will be displayed in UTC. Because is expected to already be UTC, this should be equivalent to <see cref="Raw"/>.
        /// </summary>
        Utc,

        /// <summary>
        /// The date will be displayed in the user's local time
        /// </summary>
        UserLocal,

        /// <summary>
        /// The date will be displayed in the local time of the server or a similar entity depending on the implementation
        /// </summary>
        ServerLocal
    }
}
