using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocalCompiler
{
    class Token
    {
        public enum TokType
        {
            Id,
            Int,
            Letter,
            Indirect,
            Komma,
            Append,
            Number,
            Text,
            Comment,
            Eol
        }

        public TokType TokenType;
        public short IntValue;
        public string StringValue;

        public Token()
        {
        }

        public Token(Token Token2)
        {
            TokenType = Token2.TokenType;
            IntValue = Token2.IntValue;
            StringValue = Token2.StringValue;
        }
    }

}
