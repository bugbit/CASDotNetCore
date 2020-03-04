#region LICENSE
/*
MIT License

Copyright (c) 2020 bugbit

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CASDotNetCore.Syntax
{
    public class Tokenizer
    {
        private static readonly Dictionary<char, ETokenType> mDictTypeSymbol = new Dictionary<char, ETokenType>
        {
            ['('] = ETokenType.OpenParens,
            [')'] = ETokenType.CloseParens,
            [';'] = ETokenType.Terminate,
            ['$'] = ETokenType.TerminateHide
        };

        private TextReader mReader;
        private CancellationToken mTokenCancel;
        private List<string> mLinesReads = new List<string>();
        private string mLineStr;
        private char mCurrentChar;
        private int mLine = 0;
        private int mPosition;
        private bool mReturnLine;

        public Tokenizer(TextReader argReader, CancellationToken argTokenCancel)
        {
            mReader = argReader;
            mTokenCancel = argTokenCancel;
        }

        public bool EOF { get; private set; }
        public bool EOL { get; private set; }

        public LinkedList<Token> Tokens { get; } = new LinkedList<Token>();

        public async Task ReadTokens()
        {
            if (!await NextChar(true))
                return;

            while (!EOF)
            {
                mTokenCancel.ThrowIfCancellationRequested();
                await NextToken();
            }
        }

        public async static Task<LinkedList<Token>> ReadTokens(TextReader argReader, CancellationToken argTokenCancel)
        {
            var pTokenizer = new Tokenizer(argReader, argTokenCancel);

            await pTokenizer.ReadTokens();

            return pTokenizer.Tokens;
        }

        private async Task<bool> NextChar(bool argCanNextLine)
        {
            mTokenCancel.ThrowIfCancellationRequested();
            mReturnLine = false;
            if (EOF)
            {
                mCurrentChar = '\x0';

                return false;
            }
            if (mLineStr == null)
            {
                if (!await ReadNextLine())
                    return false;
            }
            else if (mPosition >= mLineStr.Length)
            {
                if (!argCanNextLine)
                {
                    EOL = true;

                    return false;
                }

                if (mLine >= mLinesReads.Count)
                {
                    if (!await ReadNextLine())
                        return false;
                }
                else
                    mLineStr = mLinesReads[mLine++];
                mPosition = 0;
                mReturnLine = true;
            }

            mCurrentChar = mLineStr[mPosition++];

            return true;
        }

        private async Task<bool> ReadNextLine()
        {
            if ((mLineStr = await mReader.ReadLineAsync()) == null)
            {
                EOF = true;
                mCurrentChar = '\x0';

                return false;
            }
            EOL = false;
            mLine++;
            mLinesReads.Add(mLineStr);
            mPosition = 0;

            return true;
        }

        private async Task<bool> BackChar()
        {
            if (mPosition > 0)
                mPosition--;
            else
            {
                if (mLine <= 0)
                    return false;

                mLine--;
            }

            return await NextChar(true);
        }

        private async Task NextToken()
        {
            if (EOF)
                return;

            var pToken = new Token();

            if (EOL)
                if (!await NextChar(true))
                    return;

            while (char.IsWhiteSpace(mCurrentChar))
            {
                mTokenCancel.ThrowIfCancellationRequested();
                pToken.TrivialBefore.Append(mCurrentChar);
                if (!await NextChar(true))
                    break;
            }

            pToken.Line = mLine;
            pToken.Position = mPosition;

            if (char.IsLetter(mCurrentChar) || mCurrentChar == '%')
            {
                do
                {
                    mTokenCancel.ThrowIfCancellationRequested();
                    pToken.TokenStr.Append(mCurrentChar);
                    if (!await NextChar(false))
                        break;
                } while (char.IsLetterOrDigit(mCurrentChar) || mCurrentChar == '%');
                pToken.Type = ETokenType.Word;
                pToken.Word = pToken.TokenStr.ToString();
            }
            else if (char.IsDigit(mCurrentChar) || mCurrentChar == '.')
            {
                var pHaveDecimalPoint = false;

                do
                {
                    mTokenCancel.ThrowIfCancellationRequested();
                    pToken.TokenStr.Append(mCurrentChar);
                    if (mCurrentChar == '.')
                    {
                        if (pHaveDecimalPoint)
                            break;

                        pHaveDecimalPoint = true;
                    }
                    if (!await NextChar(false))
                        break;
                } while (char.IsDigit(mCurrentChar));
                pToken.Type = ETokenType.Number;
                pToken.Word = pToken.TokenStr.ToString();
            }
            else
            {
                if (mDictTypeSymbol.TryGetValue(mCurrentChar, out ETokenType pType))
                {
                    pToken.Type = pType;
                    await NextChar(false);
                }
            }

            Tokens.AddLast(pToken);
        }
    }
}
