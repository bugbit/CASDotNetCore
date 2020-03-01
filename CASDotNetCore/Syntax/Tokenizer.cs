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
    class Tokenizer
    {
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

        private async Task<bool> NextChar(bool argCanNextLine)
        {
            mTokenCancel.ThrowIfCancellationRequested();
            mReturnLine = false;
            if (EOF)
            {
                mCurrentChar = '\x0';

                return false;
            }
            if (mLineStr == null || mPosition >= mLineStr.Length)
            {
                if (mLineStr != null && !argCanNextLine)
                    return false;

                if (mLine >= mLinesReads.Count)
                {
                    if ((mLineStr = await mReader.ReadLineAsync()) == null)
                    {
                        EOF = true;
                        mCurrentChar = '\x0';

                        return false;
                    }
                    mLine++;
                }
                else
                    mLineStr = mLinesReads[mLine++];
                mPosition = 0;
                mReturnLine = true;
            }

            mCurrentChar = mLineStr[mPosition++];

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
    }
}
