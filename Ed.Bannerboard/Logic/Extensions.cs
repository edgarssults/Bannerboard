using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text;
using TaleWorlds.CampaignSystem;

namespace Ed.Bannerboard.Logic
{
    public static class Extensions
    {
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
		/// <param name="versionConverter">Version converter to use.</param>
        public static string ToJson(this object source, VersionConverter versionConverter)
        {
            return JsonConvert.SerializeObject(source, versionConverter);
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
		/// <param name="versionConverter">Version converter to use.</param>
		public static ArraySegment<byte> ToJsonArraySegment(this object source, VersionConverter versionConverter)
        {
            return source.ToJson(versionConverter).ToByteArray().ToArraySegment();
        }

        /// <summary>
        /// Determines whether a character is infantry.
        /// </summary>
        /// <param name="source">The character to check.</param>
        public static bool IsInfantry(this CharacterObject source)
        {
            return source.IsInfantry
                || 
                (
                    source.IsHero // Game always considers companions as infantry
                    && !source.IsPlayerCharacter
                );
        }

        /// <summary>
        /// Determines whether a character is an archer.
        /// </summary>
        /// <param name="source">The character to check.</param>
        public static bool IsArcher(this CharacterObject source)
        {
            return !source.IsHero
                && source.IsRanged
                && !source.IsMounted;
        }

        /// <summary>
        /// Determines whether a character is cavalry.
        /// </summary>
        /// <param name="source">The character to check.</param>
        public static bool IsCavalry(this CharacterObject source)
        {
            return source.IsPlayerCharacter // Game always considers player character as cavalry
                ||
                (
                    !source.IsHero
                    && !source.IsRanged
                    && source.IsMounted
                );
        }

        /// <summary>
        /// Determines whether a character is a mounted archer.
        /// </summary>
        /// <param name="source">The character to check.</param>
        public static bool IsMountedArcher(this CharacterObject source)
        {
            return !source.IsHero
                && source.IsRanged
                && source.IsMounted;
        }
    }
}
