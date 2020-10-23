﻿// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftCircuits.Parsing.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParsingHelperTests
{
    [TestClass]
    public class ParsingHelperTests
    {
        private const string ShortTest = "Four score and seven years ago";
        private const string LongTest = @"Four score and seven years ago our fathers brought forth on this continent,
a new nation, conceived in Liberty, and dedicated to the proposition that all men are created equal.

Now we are engaged in a great civil war, testing whether that nation, or any nation so conceived and so
dedicated, can long endure. We are met on a great battle-field of that war. We have come to dedicate a
portion of that field, as a final resting place for those who here gave their lives that that nation might
live. It is altogether fitting and proper that we should do this.

But, in a larger sense, we can not dedicate -- we can not consecrate -- we can not hallow -- this ground.
The brave men, living and dead, who struggled here, have consecrated it, far above our poor power to add or
detract. The world will little note, nor long remember what we say here, but it can never forget what they
did here. It is for us the living, rather, to be dedicated here to the unfinished work which they who fought
here have thus far so nobly advanced. It is rather for us to be here dedicated to the great task remaining
before us -- that from these honored dead we take increased devotion to that cause for which they gave the
last full measure of devotion -- that we here highly resolve that these dead shall not have died in vain --
that this nation, under God, shall have a new birth of freedom -- and that government of the people, by the
people, for the people, shall not perish from the earth.";

        [TestMethod]
        public void BasicTests()
        {
            ParsingHelper helper = new ParsingHelper(ShortTest);

            // Initial state
            Assert.AreEqual('\0', ParsingHelper.NullChar);
            Assert.AreEqual(ShortTest, helper.Text);
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(ShortTest.Length, helper.Remaining);

            // Peek
            Assert.AreEqual('F', helper.Peek());
            Assert.AreEqual('o', helper.Peek(1));
            Assert.AreEqual('u', helper.Peek(2));
            Assert.AreEqual('r', helper.Peek(3));
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek(1000));
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek(-1000));
            Assert.AreEqual(0, helper.Index);   // Index didn't change

            // Next
            helper.Next();
            Assert.AreEqual(1, helper.Index);
            Assert.AreEqual('o', helper.Peek());
            helper.Next(2);
            Assert.AreEqual(3, helper.Index);
            Assert.AreEqual('r', helper.Peek());
            helper.Next(-2);
            Assert.AreEqual(1, helper.Index);
            Assert.AreEqual('o', helper.Peek());
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(ShortTest.Length - helper.Index, helper.Remaining);

            helper.Next(10000);
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);
            helper.Next(-10000);
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(ShortTest.Length, helper.Remaining);

            helper.Index = 10000;
            Assert.AreEqual(ShortTest.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);
            helper.Index = -10000;
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(ShortTest.Length, helper.Remaining);

            helper.Index = 0;
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(ShortTest.Length, helper.Remaining);
            Assert.AreEqual(false, helper.EndOfText);
            helper.Index = helper.Text.Length;
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(0, helper.Remaining);
            Assert.AreEqual(true, helper.EndOfText);
            helper.Index = 5;
            Assert.AreEqual(5, helper.Index);
            Assert.AreEqual(ShortTest.Length - 5, helper.Remaining);
            Assert.AreEqual(false, helper.EndOfText);

            helper.Reset();
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(ShortTest, helper.Text);

            helper.Reset(null);
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(string.Empty, helper.Text);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);
        }

        [TestMethod]
        public void SkipTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);

            // SkipTo
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual('s', helper.Peek());
            Assert.AreEqual('c', helper.Peek(1));
            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score", includeToken: true));
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual('a', helper.Peek(1));
            helper.Reset();
            Assert.IsTrue(helper.SkipTo("SCORE", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual('s', helper.Peek());
            Assert.AreEqual('c', helper.Peek(1));
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('v'));
            Assert.AreEqual('v', helper.Peek());
            Assert.AreEqual('e', helper.Peek(1));
            Assert.IsFalse(helper.SkipTo("XxXxXxX"));
            Assert.AreEqual(LongTest.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);

            // SkipWhiteSpace
            helper.Reset();
            Assert.IsTrue(helper.SkipTo(' '));
            helper.SkipWhiteSpace();
            Assert.AreEqual('s', helper.Peek());

            // SkipWhiteSpace with options
            ParsingHelper helper2 = new ParsingHelper("    \r\nxyz");
            helper2.SkipWhiteSpace(SkipWhiteSpaceOption.StopAtEol);
            Assert.IsTrue(helper2.MatchesCurrentPosition("\r\nxyz"));
            helper2.Reset();
            helper2.SkipWhiteSpace(SkipWhiteSpaceOption.StopAtNextLine);
            Assert.IsTrue(helper2.MatchesCurrentPosition("xyz"));

            // SkipWhile
            helper.SkipWhile(c => "score".Contains(c));
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual('a', helper.Peek(1));

            // SkipToNextLine/SkipToEndOfLine
            helper.Reset();
            helper.SkipToEndOfLine();
            Assert.AreEqual('\r', helper.Peek());
            Assert.AreEqual('\n', helper.Peek(1));
            helper.SkipToNextLine();
            Assert.AreEqual('a', helper.Peek());
            Assert.AreEqual(' ', helper.Peek(1));
            helper.SkipToNextLine();
            helper.SkipToNextLine();
            Assert.AreEqual('N', helper.Peek());
            Assert.AreEqual('o', helper.Peek(1));

            // Skip
            helper.Skip('N', 'o', 'w', ' ', 'e');
            Assert.AreEqual('a', helper.Peek());
            Assert.AreEqual('r', helper.Peek(1));
        }

        [TestMethod]
        public void ParseTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);

            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score and seven years ago our ", helper.ParseTo("fathers"));
            Assert.AreEqual('f', helper.Peek());

            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score and seven years ago our ", helper.ParseTo("FATHERS", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual('f', helper.Peek());

            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score and se", helper.ParseTo('v', 'X', 'Y', 'Z'));
            Assert.AreEqual('v', helper.Peek());

            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score", helper.Parse('e', 'r', 'o', 'c', 's'));
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual(" ", helper.Parse(' '));
            Assert.AreEqual('a', helper.Peek());

            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score and seven years ago our fathers brought forth on this continent", helper.ParseWhile(c => c != ','));
            Assert.AreEqual(',', helper.Peek());

            helper.Next();  // Skip comma
            Assert.AreEqual("a", helper.ParseToken(char.IsWhiteSpace));
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual('n', helper.Peek(1));

            helper.Reset();
            Assert.AreEqual("Four", helper.ParseToken(' ', '\r', '\n'));
            Assert.AreEqual(' ', helper.Peek());

            string parseAllText = "  \t\tthe \r\n\t\t  rain in\t\t    spain\r\n   falls\r\nmainly  on\tthe\r\nplain.    ";
            string[] parseAllResults = new[] { "the", "rain", "in", "spain", "falls", "mainly", "on", "the", "plain" };

            helper.Reset(parseAllText);
            CollectionAssert.AreEqual(parseAllResults, helper.ParseTokens(' ', '\t', '\r', '\n', '.').ToList());

            helper.Reset();
            CollectionAssert.AreEqual(parseAllResults, helper.ParseTokens(c => " \t\r\n.".Contains(c)).ToList());

            // ParseCharacter
            helper.Reset("abc");
            Assert.AreEqual("a", helper.ParseCharacter());
            Assert.AreEqual("b", helper.ParseCharacter());
            Assert.AreEqual("c", helper.ParseCharacter());
            Assert.AreEqual("", helper.ParseCharacter());
        }

        [TestMethod]
        public void QuotedTextTests()
        {
            // Quoted text
            ParsingHelper helper = new ParsingHelper(" This is a \"test.\" ");
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("test.", helper.ParseQuotedText());
            Assert.AreEqual(' ', helper.Peek());

            // Two quotes escapes
            helper = new ParsingHelper(" This is a \"te\"\"st.\" ");
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("te\"st.", helper.ParseQuotedText());
            Assert.AreEqual(' ', helper.Peek());

            // No escape
            helper = new ParsingHelper(" This is a \"test.\" ");
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("test.", helper.ParseQuotedText(null, false, false));
            Assert.AreEqual(' ', helper.Peek());

            // No escape, include escape character
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("test.", helper.ParseQuotedText(null, true, false));
            Assert.AreEqual(' ', helper.Peek());

            // No escape, include enclosing quotes
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("\"test.\"", helper.ParseQuotedText(null, false, true));
            Assert.AreEqual(' ', helper.Peek());

            // No escape, include escape character and enclosing quotes
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("\"test.\"", helper.ParseQuotedText(null, true, true));
            Assert.AreEqual(' ', helper.Peek());

            // Explicit two quotes escapes
            helper = new ParsingHelper(" This is a \"te\"\"st.\" ");
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("te\"st.", helper.ParseQuotedText('\"', false, false));
            Assert.AreEqual(' ', helper.Peek());

            // Explicit two quotes escapes, include escape character
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("te\"\"st.", helper.ParseQuotedText('\"', true, false));
            Assert.AreEqual(' ', helper.Peek());

            // Explicit two quotes escapes, include enclosing quotes
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("\"te\"st.\"", helper.ParseQuotedText('\"', false, true));
            Assert.AreEqual(' ', helper.Peek());

            // Explicit two quotes escapes, include escape character and enclosing quotes
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("\"te\"\"st.\"", helper.ParseQuotedText('\"', true, true));
            Assert.AreEqual(' ', helper.Peek());

            // Custom escape
            helper = new ParsingHelper(" This is a \"te\\\"st.\" ");
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("te\"st.", helper.ParseQuotedText('\\', false, false));
            Assert.AreEqual(' ', helper.Peek());

            // Custom escape, include escape character
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("te\\\"st.", helper.ParseQuotedText('\\', true, false));
            Assert.AreEqual(' ', helper.Peek());

            // Custom escape, include enclosing quotes
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("\"te\"st.\"", helper.ParseQuotedText('\\', false, true));
            Assert.AreEqual(' ', helper.Peek());

            // Custom escape, include escape character and enclosing quotes
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("\"te\\\"st.\"", helper.ParseQuotedText('\\', true, true));
            Assert.AreEqual(' ', helper.Peek());

            // Handles end of text
            helper.Reset("");
            Assert.AreEqual(string.Empty, helper.ParseQuotedText('\"', true, true));
        }

        [TestMethod]
        public void MatchesCurrentPositionTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);
            Assert.IsTrue(helper.SkipTo("consecrated it"));
            Assert.AreEqual(true, helper.MatchesCurrentPosition("consecrated it"));
            Assert.AreEqual(true, helper.MatchesCurrentPosition("CONSECRATED IT", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(false, helper.MatchesCurrentPosition("consecrated_it"));
            Assert.AreEqual(false, helper.MatchesCurrentPosition("CONSECRATED_IT", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ExtractTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);
            string s = "consecrated it";
            Assert.IsTrue(helper.SkipTo(s));
            int start = helper.Index;
            helper.Next(s.Length);
            Assert.AreEqual(s, helper.Extract(start, helper.Index));
            Assert.AreEqual(@"consecrated it, far above our poor power to add or
detract. The world will little note, nor long remember what we say here, but it can never forget what they
did here. It is for us the living, rather, to be dedicated here to the unfinished work which they who fought
here have thus far so nobly advanced. It is rather for us to be here dedicated to the great task remaining
before us -- that from these honored dead we take increased devotion to that cause for which they gave the
last full measure of devotion -- that we here highly resolve that these dead shall not have died in vain --
that this nation, under God, shall have a new birth of freedom -- and that government of the people, by the
people, for the people, shall not perish from the earth.", helper.Extract(start));
            Assert.AreEqual(LongTest, helper.Extract(0, LongTest.Length));
            Assert.AreEqual("score", helper.Extract(5, 10));
            Assert.AreNotEqual("score", helper.Extract(5, 11));
            Assert.AreNotEqual("score", helper.Extract(4, 10));
            Assert.AreEqual(string.Empty, helper.Extract(0, 0));
            Assert.AreEqual(string.Empty, helper.Extract(LongTest.Length, LongTest.Length));
        }

        [TestMethod]
        public void OperatorOverloadTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);

            for (int i = 0; !helper.EndOfText; i++, helper++)
            {
                Assert.AreEqual(i, helper.Index);
                Assert.AreEqual(LongTest[i], helper.Peek());
            }

            helper.Reset();
            helper++;
            Assert.AreEqual(1, helper);
            helper += 2;
            Assert.AreEqual(3, helper);
            helper = helper + 2;
            Assert.AreEqual(5, helper);
            helper -= 2;
            Assert.AreEqual(3, helper);
            helper = helper - 2;
            Assert.AreEqual(1, helper);
            helper--;
            Assert.AreEqual(0, helper);
            helper += 10000;
            Assert.AreEqual(LongTest.Length, helper);
            helper -= 10000;
            Assert.AreEqual(0, helper);
        }

        [TestMethod]
        public void RegExTests()
        {
            string text = "summer side creature toothpaste dime wind harbor cake nail attention opinion railway horses garden alley quicksand knot servant fight form park polish toad rub hall";
            ParsingHelper helper = new ParsingHelper(text);
            string s = helper.ParseTokenRegEx(@"\b[d]\w+");
            Assert.AreEqual("dime", s);
            Assert.AreEqual(36, helper.Index);

            helper.Reset();
            IEnumerable<string> results = helper.ParseTokensRegEx(@"\b[s]\w+");
            CollectionAssert.AreEqual(new[] {
                "summer",
                "side",
                "servant" }, results.ToList());
            Assert.AreEqual(127, helper.Index);

            helper.Reset();
            s = helper.ParseTokenRegEx(@"\b[x]\w+");
            Assert.AreEqual(string.Empty, s);
            Assert.AreEqual(text.Length, helper.Index);

            helper.Reset();
            results = helper.ParseTokensRegEx(@"\b[x]\w+");
            CollectionAssert.AreEqual(new List<string>(), results.ToList());
            Assert.AreEqual(text.Length, helper.Index);

            helper.Reset();
            helper.SkipToRegEx(@"\b[a]\w+");
            Assert.IsTrue(helper.MatchesCurrentPosition("attention"));

            s = helper.ParseToRegEx(@"\b[r]\w+");
            Assert.AreEqual("attention opinion ", s);
            Assert.IsTrue(helper.MatchesCurrentPosition("railway "));

            helper.Reset();
            helper.SkipToRegEx(@"\b[a]\w+", true);
            Assert.IsTrue(helper.MatchesCurrentPosition(" opinion"));
        }

        [TestMethod]
        public void ParseLineTests()
        {
            List<(string, List<string>)> tests = new List<(string, List<string>)>
            {
                ("a", new List<string>(new[] { "a" })),
                ("ab", new List<string>(new[] { "ab" })),
                ("abc", new List<string>(new[] { "abc" })),
                ("abc\r", new List<string>(new[] { "abc" })),
                ("abc\r\n", new List<string>(new[] { "abc" })),
                ("abc\r\nd", new List<string>(new[] { "abc", "d" })),
                ("abc\r\nde", new List<string>(new[] { "abc", "de" })),
                ("abc\r\ndef", new List<string>(new[] { "abc", "def" })),
                ("abc\r\ndef\n", new List<string>(new[] { "abc", "def" })),
                ("abc\r\ndef\n\r", new List<string>(new[] { "abc", "def", "" })),
                ("abc\r\ndef\n\rg", new List<string>(new[] { "abc", "def", "", "g" })),
                ("abc\r\ndef\n\rgh", new List<string>(new[] { "abc", "def", "", "gh" })),
                ("abc\r\ndef\n\rghi", new List<string>(new[] { "abc", "def", "", "ghi" })),
                ("abc\r\ndef\n\rghi\n", new List<string>(new[] { "abc", "def", "", "ghi" })),
                ("abc\r\ndef\n\rghi\nx", new List<string>(new[] { "abc", "def", "", "ghi", "x" })),
                ("abc\r\ndef\n\rghi\nxy", new List<string>(new[] { "abc", "def", "", "ghi", "xy" })),
                ("abc\r\ndef\n\rghi\nxyz", new List<string>(new[] { "abc", "def", "", "ghi", "xyz" })),
                ("abc\r\ndef\n\rghi\nxyz\r", new List<string>(new[] { "abc", "def", "", "ghi", "xyz" })),
                ("abc\r\ndef\n\rghi\nxyz\r\r", new List<string>(new[] { "abc", "def", "", "ghi", "xyz", "" })),
            };

            ParsingHelper helper = new ParsingHelper(null);
            List<string> lines = new List<string>();

            foreach (var test in tests)
            {
                helper.Reset(test.Item1);
                lines.Clear();
                while (helper.ParseLine(out string line))
                    lines.Add(line);
                CollectionAssert.AreEqual(test.Item2, lines);
            }
        }

        [TestMethod]
        public void ParsePositionTests()
        {
            ParsePosition pos;
            string text = "abc\r\ndef\rghi\nxyz\n";

            List<(int Line, int Column)> values = new List<(int, int)>
            {
                (1, 1), // 0
                (1, 2), // 1
                (1, 3), // 2
                (1, 4), // 3
                (1, 5), // 4
                (2, 1), // 5
                (2, 2), // 6
                (2, 3), // 7
                (2, 4), // 8
                (3, 1), // 9
                (3, 2), // 10
                (3, 3), // 11
                (3, 4), // 12
                (4, 1), // 13
                (4, 2), // 14
                (4, 3), // 15
                (4, 4), // 16
                (5, 1), // 17
            };

            for (int i = 0; i < values.Count; i++)
            {
                pos = ParsePosition.CalculatePosition(text, i);
                Assert.AreEqual(values[i].Line, pos.Line);
                Assert.AreEqual(values[i].Column, pos.Column);
            }

            ParsingHelper helper = new ParsingHelper(text);
            helper.SkipTo("ghi");
            pos = helper.CalculatePosition();
            Assert.AreEqual(3, pos.Line);
            Assert.AreEqual(1, pos.Column);
        }
    }
}
