using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LabelManager
{
    /// <summary>
    /// The JavaProperties class loads and saves Java style properties from a Stream.
    /// This version uses C# friendly names and implements the most important features 
    /// of the Java Properties class in Sun Java version 1.5.0 with some methods 
    /// omitted (e.g. the loadFrom/storeToXML methods as C# provides simple 
    /// alternatives and any @deprecated methods).
    /// 
    /// NOTE: An important detail to remember is that non-string keys and values were 
    /// usually ignored in the Java version - I have used 'ToString()' to allow them to be
    /// handled more easily.
    /// 
    /// See http://java.sun.com/j2se/1.5.0/docs/api/java/util/Properties.html for full details.
    /// </summary>
    public class JavaProperties : Hashtable
    {
        /// <summary>
        /// A property list that contains default values for any keys not found in this property list.
        /// </summary>
        protected JavaProperties defaults;

        /// <summary>
        /// Creates an empty property list with no default values.
        /// </summary>
        public JavaProperties()
            : this(null)
        {
        }

        /// <summary>
        /// Creates an empty property list with the specified defaults.
        /// </summary>
        /// <param name="defaults">An instance of JavaProperties containing default values for the properties.</param>
        public JavaProperties(JavaProperties defaults)
        {
            this.defaults = defaults;
        }

        /// <summary>
        /// Reads a property list (key and element pairs) from the input stream.
        /// The stream is assumed to be using the ISO 8859-1 character encoding;
        /// that is each byte is one Latin1 character. Characters not in Latin1,
        /// and certain special characters, can be represented in keys and
        /// elements using escape sequences.
        /// 
        /// See http://java.sun.com/j2se/1.5.0/docs/api/java/util/Properties.html#load(java.io.InputStream)
        /// </summary>
        /// <param name="inStream">The input stream to read properties from.</param>
        public void Load(Stream inStream)
        {
            char[] convtBuf = new char[1024];
            LineReader lr = new LineReader(inStream);

            int limit;
            int keyLen;
            int valueStart;
            char c;
            bool hasSep;
            bool precedingBackslash;

            while ((limit = lr.ReadLine()) >= 0)
            {
                keyLen = 0;
                valueStart = limit;
                hasSep = false;

                precedingBackslash = false;
                while (keyLen < limit)
                {
                    c = lr.lineBuffer[keyLen];

                    // need check if escaped.
                    if ((c == '=' || c == ':') && !precedingBackslash)
                    {
                        valueStart = keyLen + 1;
                        hasSep = true;
                        break;
                    }
                    else if ((c == ' ' || c == '\t' || c == '\f') && !precedingBackslash)
                    {
                        valueStart = keyLen + 1;
                        break;
                    }

                    if (c == '\\')
                    {
                        precedingBackslash = !precedingBackslash;
                    }
                    else
                    {
                        precedingBackslash = false;
                    }

                    keyLen++;
                }

                while (valueStart < limit)
                {
                    c = lr.lineBuffer[valueStart];
                    if (c != ' ' && c != '\t' && c != '\f')
                    {
                        if (!hasSep && (c == '=' || c == ':'))
                        {
                            hasSep = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    valueStart++;
                }

                string key = loadConvert(lr.lineBuffer, 0, keyLen, convtBuf);
                string value = loadConvert(lr.lineBuffer, valueStart, limit - valueStart, convtBuf);

                SetProperty(key, value);
            }
        }

        /// <summary>
        /// Converts encoded &#92;uxxxx to unicode chars and changes special saved chars to their original forms.
        /// </summary>
        /// <param name="input">The source character array which includes the text to be converted.</param>
        /// <param name="off">Offset into the source array of the text to be converted.</param>
        /// <param name="len">Length of the text to be converted.</param>
        /// <param name="convtBuf">A buffer to hold the converted text.</param>
        /// <returns></returns>
        private string loadConvert(char[] input, int off, int len, char[] convtBuf)
        {
            if (convtBuf.Length < len)
            {
                int newLen = len * 2;
                if (newLen < 0)
                {
                    newLen = Int32.MaxValue;
                }

                convtBuf = new char[newLen];
            }

            char aChar;
            char[] output = convtBuf;
            int outLen = 0;
            int end = off + len;

            while (off < end)
            {
                aChar = input[off++];
                if (aChar == '\\')
                {
                    aChar = input[off++];
                    if (aChar == 'u')
                    {
                        // Read the xxxx
                        int value = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            aChar = input[off++];
                            switch (aChar)
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    value = (value << 4) + aChar - '0';
                                    break;
                                case 'a':
                                case 'b':
                                case 'c':
                                case 'd':
                                case 'e':
                                case 'f':
                                    value = (value << 4) + 10 + aChar - 'a';
                                    break;
                                case 'A':
                                case 'B':
                                case 'C':
                                case 'D':
                                case 'E':
                                case 'F':
                                    value = (value << 4) + 10 + aChar - 'A';
                                    break;
                                default:
                                    throw new ArgumentException(
                                        "Malformed \\uxxxx encoding.");
                            }
                        }

                        output[outLen++] = (char)value;
                    }
                    else
                    {
                        if (aChar == 't') aChar = '\t';
                        else if (aChar == 'r') aChar = '\r';
                        else if (aChar == 'n') aChar = '\n';
                        else if (aChar == 'f') aChar = '\f';

                        output[outLen++] = aChar;
                    }
                }
                else
                {
                    output[outLen++] = (char)aChar;
                }
            }

            return new string(output, 0, outLen);
        }

        /// <summary>
        /// Writes this property list (key and element pairs) in this JavaProperties
        /// table to the output stream in a format suitable for loading into a
        /// JavaProperties table using the load method. The stream is written
        /// using the ISO 8859-1 character encoding.
        /// </summary>
        /// <param name="output">An output stream.</param>
        /// <param name="comments">A description of the property list to add to the top of the properties file.</param>
        public void Store(Stream output, string comments)
        {
            // Create a writer to output to an ISO-8859-1 encoding (code page 28592).
            StreamWriter writer = new StreamWriter(output, System.Text.Encoding.GetEncoding(28592));

            if (comments != null)
            {
                writer.WriteLine("#" + comments);
            }

            writer.WriteLine("#" + DateTime.Now.ToString());

            for (IEnumerator e = this.Keys.GetEnumerator(); e.MoveNext(); )
            {
                string key = e.Current.ToString();
                string val = this[key].ToString();
                key = saveConvert(key, true);

                /* No need to escape embedded and trailing spaces for value, hence
                 * pass false to flag.
                 */
                val = saveConvert(val, false);
                writer.WriteLine(key + "=" + val);
            }

            writer.Flush();
        }

        /// <summary>
        /// Converts unicodes to encoded &#92;uxxxx and escapes special
        /// characters with a preceding slash.
        /// </summary>
        /// <param name="theString">The string to be converted.</param>
        /// <param name="escapeSpace">If true, spaces need to be escaped (for keys).</param>
        /// <returns>A fully escaped string.</returns>
        private string saveConvert(string theString, bool escapeSpace)
        {
            int len = theString.Length;
            int bufLen = len * 2;
            if (bufLen < 0)
            {
                bufLen = Int32.MaxValue;
            }

            StringBuilder outBuffer = new StringBuilder(bufLen);

            for (int x = 0; x < len; x++)
            {
                char aChar = theString[x];

                // Handle common case first, selecting largest block that
                // avoids the specials below
                if ((aChar > 61) && (aChar < 127))
                {
                    if (aChar == '\\')
                    {
                        outBuffer.Append('\\'); outBuffer.Append('\\');
                        continue;
                    }

                    outBuffer.Append(aChar);
                    continue;
                }

                switch (aChar)
                {
                    case ' ':
                        if (x == 0 || escapeSpace)
                            outBuffer.Append('\\');

                        outBuffer.Append(' ');
                        break;
                    case '\t': outBuffer.Append('\\'); outBuffer.Append('t');
                        break;
                    case '\n': outBuffer.Append('\\'); outBuffer.Append('n');
                        break;
                    case '\r': outBuffer.Append('\\'); outBuffer.Append('r');
                        break;
                    case '\f': outBuffer.Append('\\'); outBuffer.Append('f');
                        break;
                    case '=': // Fall through
                    case ':': // Fall through
                    case '#': // Fall through
                    case '!':
                        outBuffer.Append('\\'); outBuffer.Append(aChar);
                        break;

                    default:
                        if ((aChar < 0x0020) || (aChar > 0x007e))
                        {
                            outBuffer.Append('\\');
                            outBuffer.Append('u');
                            outBuffer.Append(toHex((aChar >> 12) & 0xF));
                            outBuffer.Append(toHex((aChar >> 8) & 0xF));
                            outBuffer.Append(toHex((aChar >> 4) & 0xF));
                            outBuffer.Append(toHex(aChar & 0xF));
                        }
                        else
                        {
                            outBuffer.Append(aChar);
                        }
                        break;
                }
            }

            return outBuffer.ToString();
        }

        /// <summary>
        /// Searches for the property with the specified key in this property list.
        /// If the key is not found in this property list, the default property list,
        /// and its defaults, recursively, are then checked. The method returns
        /// <code>null</code> if the property is not found.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The value in this property list with the specified key.</returns>
        public string GetProperty(string key)
        {
            Object objectValue = this[key];
            if (objectValue != null)
            {
                return objectValue.ToString();
            }
            else if (defaults != null)
            {
                return defaults.GetProperty(key);
            }

            return null;
        }

        /// <summary>
        /// Searches for the property with the specified key in this property list.
        /// If the key is not found in this property list, the default property list,
        /// and its defaults, recursively, are then checked. The method returns the
        /// default value argument if the property is not found.
        /// </summary>
        /// <param name="key">The hashtable key.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value in this property list with the specified key value.</returns>
        public string GetProperty(string key, string defaultValue)
        {
            string val = GetProperty(key);
            return (val == null) ? defaultValue : val;
        }

        /// <summary>
        /// Adds a string key/value property pair to the underlying Hashtable.
        /// Enforces use of strings for property keys and values.
        /// </summary>
        /// <param name="key">the property name to use as the key.</param>
        /// <param name="newValue">the value of the property.</param>
        /// <returns>an Object - which should be a string if properties have been used property.</returns>
        public Object SetProperty(string key, string newValue)
        {
            Object oldValue = this[key];
            this[key] = newValue;
            return oldValue;
        }

        /// <summary>
        /// Returns an enumeration of all the keys in this property list,
        /// including distinct keys in the default property list if a key
        /// of the same name has not already been found from the main
        /// properties list.
        /// </summary>
        /// <returns>An enumeration of all the keys in this property list,
        /// including the keys in the default property list.</returns>
        public IEnumerator PropertyNames()
        {
            Hashtable combined;
            if (defaults != null)
            {
                combined = new Hashtable(defaults);

                for (IEnumerator e = this.Keys.GetEnumerator(); e.MoveNext(); )
                {
                    string key = (string)e.Current;
                    combined.Add(key, this[key]);
                }
            }
            else
            {
                combined = new Hashtable(this);
            }

            return combined.Keys.GetEnumerator();
        }

        /// <summary>
        /// A table of hex digits - used for converting Unicode excapes.
        /// </summary>
        private static char[] hexDigit = new char[]
		{
			'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'
		};

        /// <summary>
        /// Convert a nibble to a hex character.
        /// </summary>
        /// <param name="nibble">the nibble to convert.</param>
        /// <returns>A hex character representation of the 'nibble'.</returns>
        private static char toHex(int nibble)
        {
            return hexDigit[(nibble & 0xF)];
        }

        /// <summary>
        /// A private class to read lines from the input stream in the special 
        /// escaped ".properties" format.
        /// </summary>
        protected class LineReader
        {
            /// <summary>
            /// After calling ReadLine, holds the characters of the line read.
            /// </summary>
            public char[] lineBuffer = new char[1024];

            // Raw characters read in from the stream.
            private char[] inBuf = new char[8192];
            // Number of characters read into inBuf.
            private int inLimit = 0;
            // Currenty offset in inBuf.
            private int inOff = 0;

            private StreamReader reader;

            /// <summary>
            /// Construct the LineReader with a stream to read from.
            /// </summary>
            /// <param name="stream">The stream to be read from - it is then wrapped in a StreamReader.</param>
            public LineReader(Stream stream)
            {
                this.reader = new StreamReader(stream);
            }

            /// <summary>
            /// Read a line in the specialised ".properties" format.
            /// </summary>
            /// <returns>The number of characters in lineBuffer.</returns>
            public int ReadLine()
            {
                int len = 0;
                char c;

                bool skipWhiteSpace = true;
                bool isCommentLine = false;
                bool isNewLine = true;
                bool appendedLineBegin = false;
                bool precedingBackslash = false;
                bool skipLF = false;

                while (true)
                {
                    if (inOff >= inLimit)
                    {
                        inLimit = reader.ReadBlock(inBuf, 0, inBuf.Length);
                        inOff = 0;
                        if (inLimit <= 0)
                        {
                            if (len == 0 || isCommentLine)
                            {
                                return -1;
                            }

                            return len;
                        }
                    }

                    c = inBuf[inOff++];
                    if (skipLF)
                    {
                        skipLF = false;
                        if (c == '\n')
                        {
                            continue;
                        }
                    }

                    if (skipWhiteSpace)
                    {
                        if (c == ' ' || c == '\t' || c == '\f')
                        {
                            continue;
                        }

                        if (!appendedLineBegin && (c == '\r' || c == '\n'))
                        {
                            continue;
                        }
                        skipWhiteSpace = false;
                        appendedLineBegin = false;
                    }

                    if (isNewLine)
                    {
                        isNewLine = false;
                        if (c == '#' || c == '!')
                        {
                            isCommentLine = true;
                            continue;
                        }
                    }

                    if (c != '\n' && c != '\r')
                    {
                        lineBuffer[len++] = c;
                        if (len == lineBuffer.Length)
                        {
                            int newLength = lineBuffer.Length * 2;
                            if (newLength < 0)
                            {
                                newLength = Int32.MaxValue;
                            }

                            char[] buf = new char[newLength];
                            Array.Copy(lineBuffer, buf, lineBuffer.Length);
                            lineBuffer = buf;
                        }

                        //flip the preceding backslash flag
                        if (c == '\\')
                        {
                            precedingBackslash = !precedingBackslash;
                        }
                        else
                        {
                            precedingBackslash = false;
                        }
                    }
                    else
                    {
                        // reached EOL
                        if (isCommentLine || len == 0)
                        {
                            isCommentLine = false;
                            isNewLine = true;
                            skipWhiteSpace = true;
                            len = 0;
                            continue;
                        }

                        if (inOff >= inLimit)
                        {
                            inLimit = reader.ReadBlock(inBuf, 0, inBuf.Length);
                            inOff = 0;
                            if (inLimit <= 0)
                            {
                                return len;
                            }
                        }

                        if (precedingBackslash)
                        {
                            len -= 1;
                            //skip the leading whitespace characters in following line
                            skipWhiteSpace = true;
                            appendedLineBegin = true;
                            precedingBackslash = false;

                            if (c == '\r')
                            {
                                skipLF = true;
                            }
                        }
                        else
                        {
                            return len;
                        }
                    }
                }
            }
        }
    }
}
