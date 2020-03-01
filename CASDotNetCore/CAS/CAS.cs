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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CASDotNetCore.CAS
{
    public class CAS
    {
        protected CASProgress mProgress;
        private string[] mArgs;
        private bool mExit;

        public CAS(Action<CASProgressInfo> argProgressAction)
        {
            mProgress = new CASProgress(argProgressAction);
        }
        public async Task<int> RunAsync(string[] args)
        {
            mArgs = args;
            try
            {
                PrintHeader();
                await ParseCommandLine();
                if (mExit)
                    return 0;

                BeforeRun();
            }
            catch (Exception ex)
            {
                mProgress.PrintException(ex);

                return -1;
            }

            RunInternal();

            try
            {
                AfterRun();
            }
            catch (Exception ex)
            {
                mProgress.PrintException(ex);

                return -1;
            }

            return 0;
        }

        public int Run(string[] args)
        {
            var pEvent = new AutoResetEvent(false);
            var pAwaiter = RunAsync(args).ConfigureAwait(true).GetAwaiter();

            pAwaiter.OnCompleted(() => pEvent.Set());

            pEvent.WaitOne();

            return pAwaiter.GetResult();
        }

        //private IProgress<string> mPrint
        private void GetHeader(out string argText, out string argTitle)
        {
            var pAssembly = Assembly.GetEntryAssembly();
            var pAttrs = pAssembly.GetCustomAttributes(false);
            var pName = pAttrs.OfType<AssemblyTitleAttribute>().First().Title;
            var pVersion = pAssembly.GetName().Version.ToString();
            var pDescription = pAttrs.OfType<AssemblyDescriptionAttribute>().First().Description;
            var pLicense = pAttrs.OfType<AssemblyCopyrightAttribute>().First().Copyright;

            argTitle = $"{pName} {pVersion}";
            argText =
$@"
/*
{pName} Version {pVersion}
{pDescription}
https://github.com/bugbit/CASDotNetCore

{pLicense}
MIT LICENSE
*/"
;
        }

        private void PrintHeader()
        {
            GetHeader(out string pText, out string pTitle);

            mProgress.SetTitle(pTitle);
            mProgress.Print(pText, true);
        }

        private async Task ParseCommandLine()
        {
#if DEBUG
            if (mArgs != null && mArgs.Length > 0 && mArgs[0].Equals("--t", StringComparison.InvariantCultureIgnoreCase))
            {
                await Test();
            }
#endif
        }

        private void BeforeRun()
        {

        }

        private void RunInternal()
        {

        }

        private void AfterRun()
        {

        }

        private async Task Test()
        {
            await Task.Run(() => { });
        }
    }
}
