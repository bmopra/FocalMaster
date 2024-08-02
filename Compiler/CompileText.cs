using FocalMaster.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocalCompiler
{
    partial class Compiler
    {
        CompileResult ConvertText(
            string itemName,
            string characters, 
            byte[] code, 
            int startPosition, 
            out int numberOfGeneratedBytes, 
            out string errorMsg)
        {
            int numberOfCharacters = characters.Length;
            int characterIndex = 0;
            int codeIndex = startPosition;
            int maxNumberOfBytes = code.Length;
            numberOfGeneratedBytes = 0;
            do
            {
                if (codeIndex == maxNumberOfBytes)
                {
                    errorMsg = $"{itemName} could not be more than {maxNumberOfBytes - startPosition} characters";
                    numberOfGeneratedBytes = maxNumberOfBytes - codeIndex;
                    return CompileResult.CompileError;
                }
                try
                {
                    code[codeIndex++] =
                        HP41CharacterEncoding
                            .ParseCharacterAtPosition(
                                characters,
                                characterIndex,
                                out int numberOfCharactersUsed);
                    characterIndex +=
                        numberOfCharactersUsed;
                }
                catch (Exception e)
                {
                    errorMsg = e.Message;
                    return CompileResult.CompileError;
                }
            }
            while (characterIndex < numberOfCharacters);
            numberOfGeneratedBytes = codeIndex - startPosition;
            errorMsg = string.Empty;
            return CompileResult.Ok;
        }

        CompileResult CompileText(Token token, out byte[] outCode, out string errorMsg, bool append = false)
        {
            outCode = null;
            errorMsg = string.Empty;

            int maxNumberOfBytes = 16;
            byte[] code = new byte[maxNumberOfBytes];
            int codeIndex = 0;
            code[codeIndex++] = (byte)0xF0;
            if (append)
                code[codeIndex++] = (byte)0x7F;
            if(ConvertText("text", 
                    token.StringValue, code, codeIndex, 
                    out int numberOfGeneratedBytes, out errorMsg)
                == CompileResult.CompileError)
            { 
                outCode = null;
                return CompileResult.CompileError;
            }
            if (append)
                numberOfGeneratedBytes++;
            code[0] += (byte)(numberOfGeneratedBytes);
            Array.Resize(ref code, numberOfGeneratedBytes + 1);
            outCode = code;
            return CompileResult.Ok;
        }

        /////////////////////////////////////////////////////////////

        CompileResult CompileTextAppend(Token token, out byte[] outCode, out string errorMsg)
        {
            lex.GetToken(ref token);

            if (token.TokenType != Token.TokType.Text)
            {
                outCode = new byte[0];
                errorMsg = string.Format("Text expected \"{0}\"", token.StringValue);
                return CompileResult.CompileError;
            }

            return CompileText(token, out outCode, out errorMsg, true);
        }

    }
}
