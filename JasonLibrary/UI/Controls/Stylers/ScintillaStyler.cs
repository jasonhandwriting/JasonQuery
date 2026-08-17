using ScintillaNET;

namespace JasonLibrary.UI.Controls.Stylers
{
    public abstract class ScintillaStyler
    {
        private Lexer _lexer;
        private bool _showLineNumbers;
        private bool _codeFolding;
        private bool _braceMatching;
        private bool _autoIndent;

        protected ScintillaStyler(Lexer lex, bool lineNumbers, bool codeFolding, bool braceMatching, bool autoIndent)
        {
            _lexer = lex;
            _showLineNumbers = lineNumbers;
            _codeFolding = codeFolding;
            _braceMatching = braceMatching;
            _autoIndent = autoIndent;
        }

        public abstract void ApplyStyle(Scintilla scintilla);
        public abstract void RemoveStyle(Scintilla scintilla);
        public abstract void SetKeywords(Scintilla scintilla);

        public virtual Lexer Lexer
        {
            get => _lexer;
            set => _lexer = value;
        }

        public virtual bool ShowLineNumbers
        {
            get => _showLineNumbers;
            set => _showLineNumbers = value;
        }

        public virtual bool CodeFolding
        {
            get => _codeFolding;
            set => _codeFolding = value;
        }

        public virtual bool BraceMatching
        {
            get => _braceMatching;
            set => _braceMatching = value;
        }

        public virtual bool AutoIndent
        {
            get => _autoIndent;
            set => _autoIndent = value;
        }

        public virtual bool IsBrace(int c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                case '<':
                case '>':
                    {
                        return true;
                    }
            }

            return false;
        }

        public virtual char IndentChar => '{';

        public virtual char OutdentChar => '}';
    }
}