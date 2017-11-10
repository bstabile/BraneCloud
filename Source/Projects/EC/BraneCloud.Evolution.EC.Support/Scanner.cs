using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BraneCloud.Evolution.EC.Support
{
    public class Scanner : StreamReader
    {
        string _currentWord;

        public Scanner(Stream source)
            : base(source)
        {
            ReadNextWord();
        }

        private void ReadNextWord()
        {
            var sb = new StringBuilder();
            do
            {
                var next = Read();
                if (next < 0)
                    break;
                var nextChar = (char)next;
                if (char.IsWhiteSpace(nextChar))
                    break;
                sb.Append(nextChar);
            } while (true);
            while ((Peek() >= 0) && (char.IsWhiteSpace((char)Peek())))
                Read();
            if (sb.Length > 0)
                _currentWord = sb.ToString();
            else
                _currentWord = null;
        }

        public bool HasNextInt()
        {
            if (_currentWord == null)
                return false;
            int dummy;
            return int.TryParse(_currentWord, out dummy);
        }

        public int NextInt()
        {
            try
            {
                return int.Parse(_currentWord);
            }
            finally
            {
                ReadNextWord();
            }
        }

        public bool HasNextDouble()
        {
            if (_currentWord == null)
                return false;
            double dummy;
            return double.TryParse(_currentWord, out dummy);
        }

        public double NextDouble()
        {
            try
            {
                return double.Parse(_currentWord);
            }
            finally
            {
                ReadNextWord();
            }
        }

        public bool HasNext()
        {
            return _currentWord != null;
        }
    }
}
