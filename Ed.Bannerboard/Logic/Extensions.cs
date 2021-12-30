using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text;

namespace Ed.Bannerboard.Logic
{
    public static class Extensions
    {
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        public static string ToJson(this object source)
        {
            return JsonConvert.SerializeObject(source, new VersionConverter());
        }

        /// <summary>
        /// Converts a string to a byte array.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        public static byte[] ToByteArray(this string source)
        {
            return Encoding.UTF8.GetBytes(source);
        }

        /// <summary>
        /// Converts a byte array to an ArraySegment of bytes.
        /// </summary>
        /// <param name="source">The byte array to convert.</param>
        public static ArraySegment<byte> ToArraySegment(this byte[] source)
        {
            return new ArraySegment<byte>(source);
        }

        /// <summary>
        /// Serializes an object to JSON and then converts it to an ArraySegment.
        /// </summary>
        /// <param name="source">The object to convert.</param>
        public static ArraySegment<byte> ToJsonArraySegment(this object source)
        {
            return source.ToJson().ToByteArray().ToArraySegment();
        }
    }
}
