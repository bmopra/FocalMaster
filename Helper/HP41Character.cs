using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocalMaster.Helper
{
    internal class HP41Character: 
        IComparable<byte>, IEquatable<char>
    {
        public bool HasSubstitute { get; private set; } = false;
        public char Substitute { get; private set; } = default(char);
        public bool SubstituteHidesNativeCharacter { get; private set; } = false;
        public string Unicode { get; private set; }
        public char SortCode
            => Unicode[0];
        public byte Code { get; private set; }

        public int CompareTo(byte aValue)
            => Code.CompareTo(aValue);

        public bool Equals(char aCharacter)
            => (HasSubstitute && aCharacter  == Substitute)
                || aCharacter + "" == Unicode;

        public bool Equals(string anIdentifier)
            => anIdentifier == Unicode
                || (HasSubstitute && anIdentifier.Length == 1
                    && anIdentifier[0] == Substitute);

        public HP41Character(
            byte theValue, char theCharacter)
        {
            Code = theValue;
            Unicode = theCharacter + "";
        }

        public HP41Character(
            byte theValue, string theCompositeCharacter)
        {
            Code = theValue;
            Unicode = theCompositeCharacter;
        }

        public HP41Character(
            byte theValue,
            string theCompositeCharacter,
            char theSubstituteCharacter)
        {
            Code = theValue;
            Substitute = theSubstituteCharacter;
            Unicode = theCompositeCharacter;
        }

        public HP41Character(
            byte theValue,
            char theCompositeCharacter,
            char theSubstituteCharacter)
        {
            Code = theValue;
            Substitute = theSubstituteCharacter;
            Unicode = theCompositeCharacter + "";
        }

        internal void OnRegistred()
        {
            SubstituteHidesNativeCharacter =
                HasSubstitute &&
                    Substitute < 256
                    && HP41CharacterEncoding.TryGetCharacter(
                        (byte)Substitute, out HP41Character character)
                    && character.Unicode == Substitute + "";
        }
    }

}
