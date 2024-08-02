using FocalCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocalCompiler
{
    partial class Compiler
    {
        /////////////////////////////////////////////////////////////

        CompileResult CompileNumber(Token token, out byte[] outCode, out string errorMsg)
        {
            outCode = new byte[token.StringValue.Length];
            int outCodeLength = 0;

            foreach (char c in token.StringValue)
            {
                if (c == '-')
                {
                    outCode[outCodeLength] = 0x1C;
                }
                else if (c == 'E' || c == 'e')
                {
                    outCode[outCodeLength] = 0x1B;
                }
                else if (c == '.')
                {
                    outCode[outCodeLength] = 0x1A;
                }
                else
                {
                    outCode[outCodeLength] = (byte)((byte)0x10 + (byte)c - (byte)'0');
                }

                outCodeLength++;
            }
            errorMsg = string.Empty;
            return CompileResult.Ok;
        }
    }
}
