using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;

namespace Ed.Bannerboard.Logic
{
    public static class Extensions
    {
        /// <summary>
        /// Serializes an object to a JSON byte array.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        public static byte[] ToJsonByteArray(this object obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, new VersionConverter()));
        }
    }
}
