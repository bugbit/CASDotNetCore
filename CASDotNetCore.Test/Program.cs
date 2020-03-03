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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using ST = CASDotNetCore.Syntax;

namespace CASDotNetCore.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            RunTest().RunSynchronously();
        }

        static async Task RunTest()
        {
            var pMethods = typeof(Program).Assembly.GetTypes().Select(t => t.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.NonPublic, (m, c) => ((MethodInfo)m).GetCustomAttributes((Type)c, true).Length != 0, typeof(TestAttribute)).OfType<MethodInfo>()).SelectMany(m => m);

            foreach (var pMethod in pMethods)
            {
                if (pMethod.ReturnType == typeof(Task))
                    await ((TestHandler)pMethod.CreateDelegate(typeof(TestHandler))).Invoke();
                else
                    pMethod.Invoke(null, null);
                break;
            }
        }

        [Test]
        static async Task TokernizerTest()
        {
            var pTexts = new[]
            {
                "20"
            };

            foreach (var pText in pTexts)
            {
                var pTokens = await ST.Tokenizer.ReadTokens(new StringReader(pText), CancellationToken.None);

                Console.WriteLine($"{pText} :");
                foreach (var pToken in pTokens)
                    Console.WriteLine($"{pToken.Type} : {pToken.Word}");
            }
        }
    }
}
