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
using System.Text;
using System.Threading.Tasks;

namespace CASDotNetCore.CAS
{
    public class CASProgress : ICASProgress
    {
        private IProgress<CASProgressInfo> mProgress;

        public CASProgress(Action<CASProgressInfo> argProgressAction)
        {
            mProgress = new Progress<CASProgressInfo>(argProgressAction);
        }

        public void Print(string argMsg, bool argNewLine) => mProgress.Report(new CASProgressInfo { Type = ECASProgressType.Print, Text = argMsg, NewLine = argNewLine });

        public void PrintException(Exception ex) => mProgress.Report(new CASProgressInfo { Ex = ex });

        public void SetTitle(string argTitle) => mProgress.Report(new CASProgressInfo { Text = argTitle });
    }
}
