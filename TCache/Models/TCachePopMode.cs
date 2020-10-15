using System;
namespace TCache
{
    public enum TCachePopMode
    {
        /// <summary>
        /// Get the item and take no further actions
        /// </summary>
        Get = 0,
        /// <summary>
        /// Get the item and delete the message
        /// </summary>
        Delete = 1
    }
}
