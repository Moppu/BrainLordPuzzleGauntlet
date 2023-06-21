using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    /// <summary>
    /// Text conversion utils
    /// </summary>
    public class TextConversion
    {
        // dialogue converter
        public static byte convertChar(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }
            else if (c == ')')
            {
                return 0x10;
            }
            else if (c == '.')
            {
                return 0x17;
            }
            else if (c == '?')
            {
                return 0x56;
            }
            else if (c == ':')
            {
                return 0x5A;
            }
            else if (c == ';')
            {
                return 0x5B;
            }
            else if (c == '\'')
            {
                return 0x66;
            }
            else if (c == '\"')
            {
                return 0x67;
            }
            else if (c == '-')
            {
                return 0x68;
            }
            else if (c == ',')
            {
                return 0x69;
            }
            else if (c == '*')
            {
                return 0x6A;
            }
            else if (c == '~')
            {
                return 0x82; // alternate color for item name highlights etc
            }
            else if (c == '!')
            {
                return 0x85;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                return (byte)((c - 'A') + 0x20);
            }
            else if (c >= 'a' && c <= 'z')
            {
                return (byte)((c - 'a') + 0x3A);
            }
            else if (c == '/')
            {
                // newline
                return 0xf9;
            }
            else if (c == '\\')
            {
                // wait
                return 0xfa;
            }
            else
            {
                // space
                return 0x1f;
            }
        }
    }
}
