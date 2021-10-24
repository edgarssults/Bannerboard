using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ed.Bannerboard.UI.Logic
{
    public static class Extensions
    {
        /// <summary>
        /// Deserializes an object from a byte array.
        /// </summary>
        /// <param name="arrBytes">The byte array.</param>
        public static object ToObject(this byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
