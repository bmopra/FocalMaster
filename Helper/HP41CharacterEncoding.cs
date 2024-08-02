//
// Author:
//   Michael Göricke
//
// Copyright (c) 2023
//
// This file is part of FocalMaster.
//
// The FocalMaster is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.

using FocalMaster.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FocalMaster.Helper
{
    static class HP41CharacterEncoding
    {
        private interface IHP41Characters: 
            IEnumerable<HP41Character>
        {
            int Count { get; }
            IHP41Characters Add(HP41Character character);
        }

        private struct HP41SingleCharacter: IHP41Characters
        {
            public HP41Character Character;
            public int Count => 1;

            public IHP41Characters Add(HP41Character character)
                => new HP41ManyCharacters(this, character);

            public IEnumerator<HP41Character> GetEnumerator()
            {
                yield return Character;
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                => GetEnumerator();

            public static explicit operator HP41SingleCharacter(HP41Character character)
                => new HP41SingleCharacter(character);

            private HP41SingleCharacter(HP41Character character)
            {
                Character = character;
            }
        }

        private struct HP41ManyCharacters: IHP41Characters
        {
            public HP41Character[] Characters;
            public int Count => Characters.Length;

            public IHP41Characters Add(HP41Character character)
                => new HP41ManyCharacters(this, character);

            public IEnumerator<HP41Character> GetEnumerator()
                => ((IEnumerable< HP41Character >)Characters).GetEnumerator();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                => Characters.GetEnumerator();

            public HP41ManyCharacters(
                HP41SingleCharacter character,
                HP41SingleCharacter anotherCharacter)
            {
                Characters = new HP41Character[2];
                Characters[0] = character.Character;
                Characters[1] = character.Character;
            }
            public HP41ManyCharacters(
                HP41SingleCharacter character,
                HP41Character anotherCharacter)
            {
                Characters = new HP41Character[2];
                Characters[0] = character.Character;
                Characters[1] = character.Character;
            }

            public HP41ManyCharacters(
                HP41ManyCharacters characters,
                HP41SingleCharacter character)
            {
                Characters = new HP41Character[characters.Count + 1];
                Array.Copy(characters.Characters, Characters, characters.Count);
                Characters[characters.Count] = character.Character;
            }
            public HP41ManyCharacters(
                HP41ManyCharacters characters,
                HP41Character character)
            {
                Characters = new HP41Character[characters.Count + 1];
                Array.Copy(characters.Characters, Characters, characters.Count);
                Characters[characters.Count] = character;
            }

        }

        static private Dictionary<byte, HP41Character> decoderTable;
        static private Dictionary<char, IHP41Characters> encoderTable;
        static private byte[] hiddenNativeCharacters;

        static private HP41Character[] characterTable = 
            {
                new HP41Character(0, '‾'),
                new HP41Character(1, 'ˣ'),
                new HP41Character(2, "x̄",'ẍ'),
                new HP41Character(3, '←'),
                new HP41Character(4, 'α'),
                new HP41Character(5, 'β'),
                new HP41Character(6, 'Γ'),
                new HP41Character(7, '↓'),
                new HP41Character(8, 'Δ'),
                new HP41Character(9, 'σ'),
                new HP41Character(10, '♦'),
                new HP41Character(11, 'λ'),
                new HP41Character(12, 'μ'),
                new HP41Character(13, '∡', '@'), 
                    // according to the HP document "Creating Your Own HP-41 Bar Code" page 17
                    // Using '@' as a substitute hides the default '@' character. 
                new HP41Character(14, 'τ'),
                new HP41Character(15, 'Φ'),
                new HP41Character(16, 'Θ'),
                new HP41Character(17, 'Ω'),
                new HP41Character(18, 'δ'),
                new HP41Character(19, 'Ȧ'),
                new HP41Character(20, 'ȧ'),
                new HP41Character(21, 'Ä'),
                new HP41Character(22, 'ä'),
                new HP41Character(23, 'Ö'),
                new HP41Character(24, 'ö'),
                new HP41Character(25, 'Ü'),
                new HP41Character(26, 'ü'),
                new HP41Character(27, 'Æ'),
                new HP41Character(28, 'œ'),
                new HP41Character(29, '≠', '#'), 
                    // according to the HP document "Creating Your Own HP-41 Bar Code" page 17
                new HP41Character(30, '£'),
                new HP41Character(31, '▒'),
                new HP41Character(96, '┬'),
                new HP41Character(123, 'π'),
                new HP41Character(124, '|'),
                new HP41Character(125, '→'),
                new HP41Character(126, 'Σ', '&'), 
                    // according to the HP document "Creating Your Own HP-41 Bar Code" page 17
                    // Using '&' as a substitute hides the default '&' character. 
                new HP41Character(127, 'Ⱶ')
            };

        /////////////////////////////////////////////////////////////

        static HP41CharacterEncoding()
        {
            decoderTable = new Dictionary<byte, HP41Character>();
            encoderTable = new Dictionary<char, IHP41Characters>();
            
            foreach(var hp41Character in characterTable)
                decoderTable.Add(hp41Character.Code, hp41Character);
            foreach(var hp41Character in characterTable)
            {
                if(encoderTable.TryGetValue(
                    hp41Character.SortCode, 
                    out IHP41Characters characters))
                    encoderTable[hp41Character.SortCode] = 
                        characters.Add(hp41Character);
                else
                    encoderTable.Add(
                        hp41Character.SortCode, 
                        (HP41SingleCharacter)hp41Character);
            }
            LinkedList<byte> hiddenChars = new LinkedList<byte>();
            foreach (var hp41Character in characterTable)
            {
                hp41Character.OnRegistred();
                if (hp41Character.SubstituteHidesNativeCharacter)
                    hiddenChars.AddLast((byte)hp41Character.Substitute);
            }
            hiddenNativeCharacters = hiddenChars.ToArray();
            Array.Sort(hiddenNativeCharacters);
        }

        public static bool IsHiddenNativeChar(char character)
            => character < 256 &&
                Array.BinarySearch(hiddenNativeCharacters, (byte)character) >= 0;

        /////////////////////////////////////////////////////////////

        [Obsolete]
        public static string FromHp41(byte value, bool useSubstitute)
        {
            if (decoderTable.TryGetValue(value, out HP41Character hp41Character))
            {
                return useSubstitute && hp41Character.HasSubstitute ?
                    hp41Character.Substitute + "" :
                    hp41Character.Unicode;
            }
            return useSubstitute && Array.BinarySearch(hiddenNativeCharacters, value) >= 0 ?
                 "`" + (char)value:
                (char)value + "";
        }

        /////////////////////////////////////////////////////////////

        [Obsolete]
        public static byte ToHp41(char character)
        {
            if (encoderTable.TryGetValue(character,
                out IHP41Characters hp41Characters))
            {
                foreach (var hp41Character
                    in hp41Characters)
                {
                    if (hp41Character.Unicode.Length == 1)
                        return hp41Character.Code;
                }
            }
            if (character > 255)
                throw new ArgumentException($"Unsupported character: {character}.");
            return (byte)character;
        }

        /////////////////////////////////////////////////////////////

        public static bool TryGetCharacter(
            byte value, out HP41Character hp41Character)
        {
            if (!decoderTable.TryGetValue(value, out hp41Character))
                hp41Character = new HP41Character(value, (char)value);
            return true;
        }

        /////////////////////////////////////////////////////////////

        public static string FormatCharacter(
            byte characterCode, 
            bool useSubstitute = false)
        {
            if (decoderTable.TryGetValue(characterCode,
                out HP41Character hp41Character))
                return useSubstitute && hp41Character.HasSubstitute ?
                    hp41Character.Substitute + "" :
                    hp41Character.Unicode;
            return (char)characterCode + "";
        }

        /////////////////////////////////////////////////////////////

        public static byte ParseCharacterAtPosition(
            string characters, 
            int startPosition,
            out int numberOfCharactersUsed)
        {
            numberOfCharactersUsed = 0;
            int numberOfCharacters = characters.Length;
            if (numberOfCharacters <= startPosition)
                throw new ArgumentException($"The first character position {startPosition} is located after the position of the  last element of the string");

            char character = characters[startPosition];
            if(character == '`')
            {
                int characterCode;
                char nextChar;

                // The '`' character starts an escape sequence. 
                int currentPosition = startPosition + 1;
                numberOfCharactersUsed ++;
                if (currentPosition == numberOfCharacters)
                    throw new FormatException($"Invalid escape sequence starting at position: {startPosition}");

                character = characters[currentPosition];
                if (character == 'x' || character == 'X')
                {
                    numberOfCharactersUsed++;
                    characterCode = 0;
                    if (currentPosition + 1 == numberOfCharacters)
                        throw new FormatException($"Invalid hexadecimal escape sequence starting at position {startPosition}");

                    bool lastHexadecimalDigit = false;
                    for (; ; )
                    {
                        nextChar = characters[currentPosition + 1];
                        if (nextChar < '0' ||
                            (nextChar > '9' && nextChar < 'A') ||
                            (nextChar > 'F' && nextChar < 'a') ||
                            nextChar > 'f')
                        {
                            if (lastHexadecimalDigit)
                                return (byte)characterCode;
                            throw new FormatException($"Invalid character {nextChar} in hexadecimal escape sequence starting at position {startPosition}");
                        }
                        int digit =
                            nextChar < 'A' ? nextChar - '0' :
                            nextChar < 'a' ? nextChar - 'A' + 10 :
                                                nextChar - 'a' + 10;
                        characterCode = digit + (characterCode << 4);
                        currentPosition++;
                        numberOfCharactersUsed++;
                        if (lastHexadecimalDigit
                                || currentPosition + 1 == numberOfCharacters)
                            return (byte)characterCode;
                        lastHexadecimalDigit = true;
                    }
                }
                else if (character == 'b' || character == 'B')
                {
                    numberOfCharactersUsed++;
                    if (currentPosition + 1 == numberOfCharacters)
                        throw new FormatException($"Invalid binary escape sequence starting at position {startPosition}");

                    nextChar = characters[currentPosition + 1];
                    if (nextChar != '0' && nextChar != '1')
                        throw new FormatException($"Invalid character {nextChar} in binary escape sequence starting at position {startPosition}");

                    characterCode = nextChar - '0';
                    currentPosition++;
                    numberOfCharactersUsed++;
                    int maxNumberOfRemainingBits = 7;
                    do
                    {
                        if (currentPosition + 1 == numberOfCharacters)
                            return (byte)characterCode;
                        nextChar = characters[currentPosition + 1];
                        if (nextChar != '0' && nextChar != '1')
                            return (byte)characterCode;
                        characterCode = (characterCode << 1)
                            + (nextChar - '0');
                        currentPosition++;
                        numberOfCharactersUsed++;
                        maxNumberOfRemainingBits--;
                    }
                    while (maxNumberOfRemainingBits > 0);
                    return (byte)characterCode;
                }
                else if (character < '0' || character > '9')
                {
                    if (HP41CharacterEncoding.IsHiddenNativeChar(character))
                    {
                        numberOfCharactersUsed++;
                        return (byte)character;
                    }
                    throw new FormatException($"Invalid escape sequence starting at position {startPosition}");
                }
                characterCode = character - '0';
                numberOfCharactersUsed++;
                if (currentPosition + 1 == numberOfCharacters)
                    return (byte)characterCode;

                nextChar = characters[currentPosition + 1];
                if (nextChar < '0' || nextChar > '9')
                    return (byte)characterCode;

                characterCode = (characterCode * 10)
                    + (nextChar - '0');
                currentPosition++;
                numberOfCharactersUsed++;
                if (currentPosition + 1 == numberOfCharacters)
                    return (byte)characterCode;

                nextChar = characters[currentPosition + 1];
                if (nextChar >= '0' && nextChar <= '9')
                {
                    int nextCharacterCode = characterCode * 10
                        + (nextChar - '0');
                    if (nextCharacterCode <= 255)
                    {
                        characterCode = nextCharacterCode;
                        numberOfCharactersUsed++;
                    }
                }
                return (byte)characterCode;
            }

            if (encoderTable.TryGetValue(character, 
                out IHP41Characters hp41Characters))
            {
                foreach (var hp41Character in hp41Characters)
                {
                    if (hp41Character.HasSubstitute
                        && character == hp41Character.Substitute)
                    {
                        numberOfCharactersUsed++;
                        return hp41Character.Code;
                    }

                    string unicode = hp41Character.Unicode;
                    int unicodeLength = unicode.Length;
                    if (unicodeLength == 1)
                    {
                        numberOfCharactersUsed++;
                        return hp41Character.Code;
                    }
                    int remainingCharacters = characters.Length - startPosition;
                    if (remainingCharacters >= unicodeLength
                        && characters.Substring(startPosition, unicodeLength) == unicode)
                    {
                        numberOfCharactersUsed += unicodeLength;
                        return hp41Character.Code;
                    }
                }
            }
            if (character > 255)
                throw new ArgumentException($"Unsupported character: {character}");
            numberOfCharactersUsed = 1;
            return(byte)character;
        }

    }
}
