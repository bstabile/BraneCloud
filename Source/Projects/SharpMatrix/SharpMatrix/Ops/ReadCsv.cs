using System;
using System.Collections.Generic;
using System.IO;

namespace BraneCloud.Evolution.EC.MatrixLib.Ops
{
    /**
     * <p>
     * Base class for reading CSV formatted files.  CSV stands for column-space-value where text strings are separated
     * by a space character.  The values are typically stored in a human readable format.  The encoded text for a single
     * variable is referred to as a word.
     * </p>
     *
     * <p>
     * Comments are allowed and identified by starting a line with the comment character.  The comment character is user
     * configurable.  By default there is no comment character.
     * </p>
     *
     * @author Peter Abeles
     */
    public class ReadCsv
    {
        // if there is a comment character
        private bool hasComment = false;

        // what the comment character is
        private char comment;

        // reader for the input stream
        private StreamReader input;

        // number of lines that have been read
        private int lineNumber = 0;

        /**
         * Constructor for ReadCsv
         *
         * @param in Where the input comes from.
         */
        public ReadCsv(Stream input)
        {
            this.input = new StreamReader(input);
        }

        /**
         * Sets the comment character.  All lines that start with this character will be ignored.
         *
         * @param comment The new comment character.
         */
        public void setComment(char comment)
        {
            hasComment = true;
            this.comment = comment;
        }

        /**
         * Returns how many lines have been read.
         *
         * @return Line number
         */
        public int getLineNumber()
        {
            return lineNumber;
        }

        /**
         * Returns the reader that it is using internally.
         * @return The reader.
         */
        public StreamReader getReader()
        {
            return input;
        }

        /**
         * Finds the next valid line of words in the stream and extracts them.
         *
         * @return List of valid words on the line.  null if the end of the file has been reached.
         * @throws java.io.IOException
         */
        protected List<string> extractWords()
        {
            while (true)
            {
                lineNumber++;
                string line = input.ReadLine();
                if (line == null)
                {
                    return null;
                }

                // skip comment lines
                if (hasComment)
                {
                    if (line[0] == comment)
                        continue;
                }

                // extract the words, which are the variables encoded
                return parseWords(line);
            }
        }

        /**
         * Extracts the words from a string.  Words are seperated by a space character.
         *
         * @param line The line that is being parsed.
         * @return A list of words contained on the line.
         */
        protected List<string> parseWords(string line)
        {
            List<string> words = new List<string>();
            bool insideWord = !isSpace(line[0]);
            int last = 0;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (insideWord)
                {
                    // see if its at the end of a word
                    if (isSpace(c))
                    {
                        words.Add(line.Substring(last, i));
                        insideWord = false;
                    }
                }
                else
                {
                    if (!isSpace(c))
                    {
                        last = i;
                        insideWord = true;
                    }
                }
            }

            // if the line ended add the word
            if (insideWord)
            {
                words.Add(line.Substring(last));
            }
            return words;
        }

        /**
         * Checks to see if 'c' is a space character or not.
         *
         * @param c The character being tested.
         * @return if it is a space character or not.
         */
        private bool isSpace(char c)
        {
            return c == ' ' || c == '\t';
        }
    }
}