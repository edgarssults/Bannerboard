using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ed.Bannerlord.Dashboard.Logic
{
    public static class Extensions
    {
        /// <summary>
        /// Serializes an object to a byte array.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        public static byte[] ToByteArray(this object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
