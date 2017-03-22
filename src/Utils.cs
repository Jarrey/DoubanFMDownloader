using Newtonsoft.Json.Linq;
using System;

namespace DoubanMusicDownloader
{
    class Utils
    {
        /// <summary>
        /// The parse.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <typeparam name="T">
        /// The type want to get value
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T Parse<T>(JObject obj, string propertyName)
        {
            try
            {
                JToken value;
                if (obj != null && obj.TryGetValue(propertyName, out value))
                {
                    var returnValue = value.Value<T>();
                    return returnValue;
                }
            }
            catch (Exception)
            {
            }

            return default(T);
        }

    }
}
