using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SO6
{
    enum TokenType
    {
        Ident,
        Comment,
        Keyword,
        Punct,
        String,
        Number
    }

    class QualifiedToken
    {
        public TokenType Type;
        public TextPointer StartPosition;
        public int StartOffset;
        public TextPointer EndPosition;
        public int EndOffset;
    }

    class RawText
    {
        public string Text;
        public TextPointer Start;
    }
}
