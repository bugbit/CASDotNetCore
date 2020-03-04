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
using System.Threading;

namespace CASDotNetCore.Syntax
{
    public sealed class TokensReader
    {
        public CancellationToken CancelToken { get; }
        public LinkedList<Token> TokensList { get; }
        public LinkedListNode<Token> NodeAct { get; private set; }
        public Token Token => NodeAct?.Value;
        public bool EOF => NodeAct == null;

        public TokensReader(LinkedList<Token> argTokens, CancellationToken argCancelToken)
        {
            CancelToken = argCancelToken;
            TokensList = argTokens;
            NodeAct = TokensList.First;
        }

        public bool Is(out LinkedListNode<Token>[] argNodes, int argCount, Func<LinkedListNode<Token>, LinkedListNode<Token>> argGetNode, Func<Token, bool> argCond)
        {
            var pNodes = new LinkedListNode<Token>[argCount];
            var pNode = NodeAct;

            for (var i = 0; i < argCount; i++)
            {
                CancelToken.ThrowIfCancellationRequested();

                if (pNode == null || !argCond.Invoke(pNode.Value))
                {
                    argNodes = null;

                    return false;
                }
                pNodes[i] = pNode;
                pNode = argGetNode(pNode);
            }

            argNodes = pNodes;

            return true;
        }

        public bool Is(int argCount, Func<LinkedListNode<Token>, LinkedListNode<Token>> argGetNode, Func<Token, bool> argCond)
        {
            var pNode = NodeAct;

            for (var i = 0; i < argCount; i++)
            {
                CancelToken.ThrowIfCancellationRequested();

                if (pNode == null || !argCond.Invoke(pNode.Value))
                    return false;

                pNode = argGetNode(pNode);
            }

            return true;
        }

        public bool IsPrev(out LinkedListNode<Token>[] argNodes, int argCount, Func<Token, bool> argCond) => Is(out argNodes, argCount, n => n.Previous, argCond);
        public bool IsPrev(int argCount, Func<Token, bool> argCond) => Is(argCount, n => n.Previous, argCond);


        public bool IsNext(out LinkedListNode<Token>[] argNodes, int argCount, Func<Token, bool> argCond) => Is(out argNodes, argCount, n => n.Next, argCond);
        public bool IsNext(int argCount, Func<Token, bool> argCond) => Is(argCount, n => n.Next, argCond);

        public bool IsNext(out LinkedListNode<Token> argNodeNext, Func<Token, bool> argCond)
        {
            var pNodeNext = NodeAct?.Next;

            if (pNodeNext == null)
            {
                argNodeNext = null;

                return false;
            }

            argNodeNext = pNodeNext;

            return argCond.Invoke(pNodeNext.Value);
        }

        public bool NextIfNodeNextValue(Func<Token, bool> argCond)
        {
            var pNodeNext = NodeAct?.Next;

            if (pNodeNext == null)
                return false;

            return argCond.Invoke(pNodeNext.Value) && Next();
        }

        public bool Next()
        {
            CancelToken.ThrowIfCancellationRequested();

            if (NodeAct == null)
                return false;

            NodeAct = NodeAct.Next;

            return NodeAct != null;
        }
    }
}
