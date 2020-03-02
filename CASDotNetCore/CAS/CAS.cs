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

using CASDotNetCore.Extensions;

namespace CASDotNetCore.CAS
{
    /*
     urls:
     . https://ace.c9.io/#nav=about
     . https://practicasdeprogramacion.wordpress.com/2011/09/08/raices-de-polinomios-regla-de-ruffini/
https://es.wikipedia.org/wiki/Algoritmo_de_Horner
https://es.wikipedia.org/wiki/Regla_de_Ruffini

https://www.solumaths.com/es/sitio/pagina-principal
http://www.librosmaravillosos.com/eldiablodelosnumeros/
https://www.smartick.es

prime factorization

Simplificar
http://www.montereyinstitute.org/courses/Algebra1/COURSE_TEXT_RESOURCE/U11_L1_T1_text_final_es.html
         */
    public class CAS
    {
        private string[] mArgs;
        private bool mExit;

        protected CASProgress mProgress;
        protected CancellationTokenSource mRunTokenCancel = new CancellationTokenSource();
        protected CancellationTokenSource mEvalTokenCancel = null;

        public CAS(Action<CASProgressInfo> argProgressAction)
        {
            mProgress = new CASProgress(argProgressAction);
        }
        public async Task<int> RunAsync(string[] args)
        {
            mArgs = args;
            try
            {
                await PrintHeader();
                await ParseCommandLine();
                if (mExit)
                    return 0;

                BeforeRun();
            }
            catch (Exception ex)
            {
                await mProgress.PrintException(ex);

                return -1;
            }

            RunInternal();

            try
            {
                AfterRun();
            }
            catch (Exception ex)
            {
                await mProgress.PrintException(ex);

                return -1;
            }

            return 0;
        }

        public int Run(string[] args) => RunAsync(args).WaitAndResult();

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

        private async Task PrintHeader()
        {
            GetHeader(out string pText, out string pTitle);

            await mProgress.SetTitle(pTitle);
            await mProgress.Print(pText, true);
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

#if DEBUG

        #region Test

        private delegate Task TestHandler();

        private async Task Test()
        {
            var pType = this.GetType();
            var pMethods = new List<MethodInfo>();

            do
            {
                var pRet = pType.FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.NonPublic, (m, c) => ((MethodInfo)m).GetCustomAttributes((Type)c, true).Length != 0, typeof(TestAttribute)).OfType<MethodInfo>();

                pMethods.AddRange(pRet);
                pType = pType.BaseType;
            } while (pType != null);

            foreach (var pMethod in pMethods)
            {
                var pArgs = pMethod.GetParameters();

                switch (pArgs.Length)
                {
                    case 0:
                        if (pMethod.ReturnType == typeof(Task))
                            await ((TestHandler)pMethod.CreateDelegate(typeof(TestHandler), this)).Invoke();
                        else
                            pMethod.Invoke(this, null);
                        break;
                }
            }
        }

        #endregion
#endif
    }
}

