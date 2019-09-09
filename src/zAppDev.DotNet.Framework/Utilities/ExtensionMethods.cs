// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Linq;

namespace zAppDev.DotNet.Framework.Utilities
{
    public static class ExtensionMethods
    {
        #region Split() // Helper, private
        public static string[] Split(this string @this, string[] stringArray, bool removeEmptyEntries)
        {
            var options = removeEmptyEntries
                ? StringSplitOptions.RemoveEmptyEntries
                : StringSplitOptions.None;

            return @this.Split(stringArray, options);
        }

        public static string[] Split(this string @this, char[] characters, bool removeEmptyEntries)
        {
            var stringArray = characters.ToStringArray();

            return @this.Split(stringArray, removeEmptyEntries);
        }
        #endregion

        #region SplitExtended()

        public static string[] SplitExtended(this string @this, string[] stringArray, bool removeEmptyEntries = false)
        {
            return @this?.Split(stringArray, removeEmptyEntries);
        }

        public static string[] SplitExtended(this string @this, char[] characters, bool removeEmptyEntries = false)
        {
            return @this?.Split(characters, removeEmptyEntries);
        }

        public static string[] SplitExtended(this string @this, char?[] characters, bool removeEmptyEntries = false)
        {
            return @this?.Split(characters?.ToStringArray(), removeEmptyEntries);
        }

        public static string[] SplitExtended(this string @this, char character, bool removeEmptyEntries = false)
        {
            return @this?.Split(character.ToStringArray(), removeEmptyEntries);
        }

        public static string[] SplitExtended(this string @this, char? character, bool removeEmptyEntries = false)
        {
            return @this?.Split(character?.ToStringArray(), removeEmptyEntries);
        }

        #endregion

        #region ToStringArray()
        public static string[] ToStringArray(this char[] characters)
        {
            return characters?.Select(c => c.ToString()).ToArray();
        }

        public static string[] ToStringArray(this char?[] characters)
        {
            return characters?.Select(c => c?.ToString()).ToArray();
        }

        public static string[] ToStringArray(this char character)
        {
            return new string[] { character.ToString() };
        }

        public static string[] ToStringArray(this char? character)
        {
            return new string[] { character?.ToString() };
        }
        #endregion
    }

}
