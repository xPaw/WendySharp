#region License
/*
NetIRC2
Copyright (c) 2013 James F. Bellinger <http://www.zer7.com>
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using NetIrc2.Details;

namespace NetIrc2
{
    /// <summary>
    /// Allows string-style manipulation of arrays of bytes.
    /// IRC does not define an encoding, so this provides for encoding-agnostic parsing.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(Details.IrcStringTypeConverter))]
    public sealed class IrcString : IEquatable<IrcString>, IList<byte>, ISerializable
    {
        byte[] _buffer;

        /// <summary>
        /// A zero-byte string.
        /// </summary>
        public static readonly IrcString Empty = "";

        /// <summary>
        /// Creates an IRC string by converting a .NET string using UTF-8 encoding.
        /// </summary>
        /// <param name="string">The .NET string to convert.</param>
        public IrcString(string @string)
        {
            Create(@string, Encoding.UTF8);
        }

        /// <summary>
        /// Creates an IRC string by converting a .NET string using the specified encoding.
        /// </summary>
        /// <param name="string">The .NET string to convert.</param>
        /// <param name="encoding">The encoding to use.</param>
        public IrcString(string @string, Encoding encoding)
        {
            Create(@string, encoding);
        }

        /// <summary>
        /// Creates an IRC string from a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        public IrcString(byte[] buffer)
        {
            Throw.If.Null(buffer, "buffer");
            Create(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Creates an IRC string from part of a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="startIndex">The index of the first byte in the new string.</param>
        /// <param name="length">The number of bytes in the new string.</param>
        public IrcString(byte[] buffer, int startIndex, int length)
        {
            Create(buffer, startIndex, length);
        }

        void Create(string @string, Encoding encoding)
        {
            Throw.If.Null(@string, "string").Null(encoding, "encoding");
            _buffer = encoding.GetBytes(@string);
        }

        void Create(byte[] buffer, int startIndex, int length)
        {
            Throw.If.OutOfRange(buffer, startIndex, length);
            _buffer = new byte[length]; Array.Copy(buffer, startIndex, _buffer, 0, length);
        }

        /// <summary>
        /// Checks if the string contains a particular byte.
        /// </summary>
        /// <param name="value">The byte to look for.</param>
        /// <returns><c>true</c> if the string contains the byte.</returns>
        public bool Contains(byte value)
        {
            return IndexOf(value) != -1;
        }

        /// <summary>
        /// Copies the string into a byte array.
        /// </summary>
        /// <param name="array">The byte array to copy to.</param>
        /// <param name="index">The starting index to copy to.</param>
        public void CopyTo(byte[] array, int index)
        {
            _buffer.CopyTo(array, index);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as IrcString);
        }

        /// <summary>
        /// Compares the current string with another string.
        /// </summary>
        /// <param name="other">The string to compare with.</param>
        /// <returns><c>true</c> if the strings are equal.</returns>
        public bool Equals(IrcString other)
        {
            return other != null && _buffer.SequenceEqual(other._buffer);
        }

        /// <inheritdoc />
        public IEnumerator<byte> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Scans the string for the first instance of a particular byte.
        /// </summary>
        /// <param name="value">The byte to look for.</param>
        /// <returns>The index of the first matching byte, or <c>-1</c>.</returns>
        public int IndexOf(byte value)
        {
            return IndexOf(value, 0);
        }

        /// <summary>
        /// Scans part of the string for the first instance of a particular byte.
        /// </summary>
        /// <param name="value">The byte to look for.</param>
        /// <param name="startIndex">The first byte to begin scanning at.</param>
        /// <returns>The index of the first matching byte, or <c>-1</c>.</returns>
        public int IndexOf(byte value, int startIndex)
        {
            return IndexOf(@byte => @byte == value, startIndex, Length - startIndex);
        }

        /// <summary>
        /// Scans part of the string for the first byte that matches the specified condition.
        /// </summary>
        /// <param name="matchCondition">The condition to match.</param>
        /// <param name="startIndex">The first byte to begin scanning at.</param>
        /// <param name="length">The distance to scan.</param>
        /// <returns>The index of the first matching byte, or <c>-1</c>.</returns>
        public int IndexOf(Func<byte, bool> matchCondition, int startIndex, int length)
        {
            return IndexOf(_buffer, matchCondition, startIndex, length);
        }

        internal static int IndexOf(byte[] buffer, Func<byte, bool> matchCondition, int startIndex, int length)
        {
            Throw.If.Null(matchCondition, "matchCondition");
            Throw.If.OutOfRange(buffer, startIndex, length, offsetName: "startIndex", countName: "length");

            for (int i = 0; i < length; i++)
            {
                int j = i + startIndex;
                if (matchCondition(buffer[j])) { return j; }
            }

            return -1;
        }

        /// <summary>
        /// Joins together a number of strings.
        /// </summary>
        /// <param name="separator">The string to separate individual strings with.</param>
        /// <param name="strings">The strings to join.</param>
        /// <returns>The joined string.</returns>
        public static IrcString Join(IrcString separator, IrcString[] strings)
        {
            Throw.If.Null(separator, "separator").NullElements(strings, "strings");

            int count = 0;
            for (int i = 0; i < strings.Length; i ++)
            {
                if (i != 0) { count += separator.Length; }
                count += strings[i].Length;
            }

            var bytes = new byte[count]; int offset = 0;
            for (int i = 0; i < strings.Length; i++)
            {
                if (i != 0) { separator.CopyTo(bytes, offset); offset += separator.Length; }
                strings[i].CopyTo(bytes, offset); offset += strings[i].Length;
            }

            Debug.Assert(offset == count);
            return new IrcString(bytes);
        }

        /// <summary>
        /// Splits the string into a number of substrings based on a separator.
        /// </summary>
        /// <param name="separator">The byte to separate strings by.</param>
        /// <returns>An array of substrings.</returns>
        public IrcString[] Split(byte separator)
        {
            return Split(separator, int.MaxValue);
        }

        /// <summary>
        /// Splits the string into a limited number of substrings based on a separator.
        /// </summary>
        /// <param name="separator">The byte to separate strings by.</param>
        /// <param name="count">The maximum number of substrings. The last substring will contain the remaining bytes.</param>
        /// <returns>An array of substrings.</returns>
        public IrcString[] Split(byte separator, int count)
        {
            Throw.If.Negative(count, "count");
            var strings = new List<IrcString>();

            if (count > 0)
            {
                int i = 0;
                while (i < Length)
                {
                    int j = IndexOf(separator, i);
                    if (j == -1 || strings.Count + 1 == count) { j = Length; }

                    int byteCount = j - i;
                    strings.Add(Substring(i, byteCount));
                    i += byteCount + 1;
                }
            }

            return strings.ToArray();
        }

        /// <summary>
        /// Checks if the start of the current string matches the specified string.
        /// </summary>
        /// <param name="value">The string to match with.</param>
        /// <returns><c>true</c> if the current string starts with <paramref name="value"/>.</returns>
        public bool StartsWith(IrcString value)
        {
            Throw.If.Null(value, "value");

            if (Length < value.Length) { return false; }
            for (int i = 0; i < value.Length; i++)
            {
                if (this[i] != value[i]) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Extracts the end of a string.
        /// </summary>
        /// <param name="startIndex">The index of the first byte in the substring.</param>
        /// <returns>The substring.</returns>
        public IrcString Substring(int startIndex)
        {
            return Substring(startIndex, Length - startIndex);
        }

        /// <summary>
        /// Extracts part of a string.
        /// </summary>
        /// <param name="startIndex">The index of the first byte in the substring.</param>
        /// <param name="length">The number of bytes to extract.</param>
        /// <returns>The substring.</returns>
        public IrcString Substring(int startIndex, int length)
        {
            Throw.If.OutOfRange(_buffer, startIndex, length, offsetName: "startIndex", countName: "length");
            return new IrcString(_buffer, startIndex, length);
        }

        /// <summary>
        /// Gets the bytes that make up the IRC string.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] ToByteArray()
        {
            return (byte[])_buffer.Clone();
        }

        /// <summary>
        /// Converts to a .NET string using UTF-8 encoding.
        /// </summary>
        /// <returns>The converted string.</returns>
        public override string ToString()
        {
            return ToString(Encoding.UTF8);
        }

        /// <summary>
        /// Converts to a .NET string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use in the conversion.</param>
        /// <returns>The converted string.</returns>
        public string ToString(Encoding encoding)
        {
            Throw.If.Null(encoding, "encoding");
            return encoding.GetString(_buffer);
        }

        /// <summary>
        /// The length of the IRC string, in bytes.
        /// </summary>
        public int Length
        {
            get { return _buffer.Length; }
        }

        /// <summary>
        /// Gets a byte from the IRC string.
        /// </summary>
        /// <param name="index">The index into the byte array.</param>
        /// <returns>The byte at the specified index.</returns>
        public byte this[int index]
        {
            get { return _buffer[index]; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Casts the IRC string to a byte array.
        /// </summary>
        /// <param name="string">The IRC string.</param>
        /// <returns>An array of bytes</returns>
        public static implicit operator byte[](IrcString @string)
        {
            return @string != null ? @string.ToByteArray() : null;
        }

        /// <summary>
        /// Casts the IRC string to a .NET string using UTF-8 encoding.
        /// </summary>
        /// <param name="string">The IRC string.</param>
        /// <returns>A .NET string.</returns>
        public static implicit operator string(IrcString @string)
        {
            return @string != null ? @string.ToString() : null;
        }

        /// <summary>
        /// Casts a byte array to an IRC string.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <returns>An IRC string.</returns>
        public static implicit operator IrcString(byte[] buffer)
        {
            return buffer != null ? new IrcString(buffer) : null;
        }

        /// <summary>
        /// Casts a .NET string to an IRC string using UTF-8 encoding.
        /// </summary>
        /// <param name="string">The .NET string.</param>
        /// <returns>An IRC string.</returns>
        public static implicit operator IrcString(string @string)
        {
            return @string != null ? new IrcString(@string) : null;
        }

        /// <summary>
        /// Compares two strings for equality.
        /// </summary>
        /// <param name="string1">The first string.</param>
        /// <param name="string2">The second string.</param>
        /// <returns><c>true</c> if the strings are equal.</returns>
        public static bool operator ==(IrcString string1, IrcString string2)
        {
            return object.Equals(string1, string2);
        }

        /// <summary>
        /// Compares two strings for inequality.
        /// </summary>
        /// <param name="string1">The first string.</param>
        /// <param name="string2">The second string.</param>
        /// <returns><c>true</c> if the strings are not equal.</returns>
        public static bool operator !=(IrcString string1, IrcString string2)
        {
            return !object.Equals(string1, string2);
        }

        /// <summary>
        /// Concatenates two strings.
        /// </summary>
        /// <param name="string1">The first string.</param>
        /// <param name="string2">The second string.</param>
        /// <returns>A string that is the concatentaion of the two.</returns>
        public static IrcString operator +(IrcString string1, IrcString string2)
        {
            Throw.If.Null(string1).Null(string2);

            var buffer = new byte[string1.Length + string2.Length];
            string1.CopyTo(buffer, 0); string2.CopyTo(buffer, string1.Length);
            return new IrcString(buffer);
        }

        #region IList
        void ICollection<byte>.Add(byte value)
        {
            throw new NotSupportedException();
        }

        void ICollection<byte>.Clear()
        {
            throw new NotSupportedException();
        }

        void IList<byte>.Insert(int index, byte value)
        {
            throw new NotSupportedException();
        }

        bool ICollection<byte>.Remove(byte value)
        {
            throw new NotSupportedException();
        }

        void IList<byte>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        int ICollection<byte>.Count
        {
            get { return Length; }
        }

        bool ICollection<byte>.IsReadOnly
        {
            get { return true; }
        }
        #endregion

        #region ISerializable
        IrcString(SerializationInfo info, StreamingContext context)
        {
            Throw.If.Null(info, "info");

            var buffer = (byte[])info.GetValue("bytes", typeof(byte[])) ?? Array.Empty<byte>();
            Create(buffer, 0, buffer.Length);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Throw.If.Null(info, "info");

            info.AddValue("bytes", ToByteArray(), typeof(byte[]));
        }
        #endregion
    }
}
