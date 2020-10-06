using System;
namespace TCache
{
    public enum TCachePopMode
    {
        /// <summary>
        /// Get the item and take no further actions
        /// </summary>
        Get,
        /// <summary>
        /// Get the item and delete the message
        /// </summary>
        Delete
    }
}
