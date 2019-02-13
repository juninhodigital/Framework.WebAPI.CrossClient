using System;

using Framework.Core;

namespace Framework.WebAPI.CrossClient
{
    public static class Extensions
    {
        #region| Methods |

        public static bool IsPunchoutRequired(this string @string)
        {
            return @string.IsEqual("PUNCHOUT_SETUP_REQUEST");
        }

        public static bool IsAuthenticationRequired(this string @string)
        {
            return @string.IsEqual("AUTHENTICATE");
        } 

        #endregion
    }
}
