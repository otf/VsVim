﻿using System;
using System.Linq;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Moq;
using NUnit.Framework;
using Vim;
using Vim.Extensions;
using Vim.UnitTest.Mock;

namespace VimCore.Test
{
    [TestFixture]
    public class MotionCaptureTest
    {
        private MockRepository _factory;
        private Mock<IMotionCaptureGlobalData> _data;
        private Mock<ITextView> _textView;
        private Mock<ITextViewMotionUtil> _util;
        private Mock<IVimHost> _host;
        private MotionCapture _captureRaw;
        private IMotionCapture _capture;

        [SetUp]
        public void Create()
        {
            _factory = new MockRepository(MockBehavior.Strict);
            _util = _factory.Create<ITextViewMotionUtil>();
            _textView = MockObjectFactory.CreateTextView(factory: _factory);
            _host = _factory.Create<IVimHost>();
            _data = _factory.Create<IMotionCaptureGlobalData>(MockBehavior.Loose);
            _captureRaw = new MotionCapture(_host.Object, _textView.Object, _util.Object, _data.Object);
            _capture = _captureRaw;
        }

        internal MotionResult Process(string input, int? count)
        {
            var realCount = count.HasValue
                ? FSharpOption.Create(count.Value)
                : FSharpOption<int>.None;
            var res = _capture.GetMotion(
                KeyInputUtil.CharToKeyInput(input[0]),
                realCount);
            foreach (var cur in input.Skip(1))
            {
                Assert.IsTrue(res.IsNeedMoreInput);
                var needMore = (MotionResult.NeedMoreInput)res;
                res = needMore.Item.Invoke(KeyInputUtil.CharToKeyInput(cur));
            }

            return res;
        }

        internal void ProcessComplete(string input, int? count = null)
        {
            Assert.IsTrue(Process(input, count).IsComplete);
        }

        internal MotionData CreateMotionData()
        {
            var point = MockObjectFactory.CreateSnapshotPoint(42);
            return new MotionData(
                new SnapshotSpan(point, point),
                true,
                MotionKind.Inclusive,
                OperationKind.CharacterWise,
                FSharpOption.Create(42));
        }

        [Test]
        public void Word1()
        {
            _util
                .Setup(x => x.WordForward(WordKind.NormalWord, 1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("w", 1);
            _factory.Verify();
        }

        [Test]
        public void Word2()
        {
            _util
                .Setup(x => x.WordForward(WordKind.NormalWord, 2))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("w", 2);
        }

        [Test]
        public void BadInput()
        {
            var res = Process("z", 1);
            Assert.IsTrue(res.IsError);
        }

        [Test]
        public void Motion_Dollar1()
        {
            _util
                .Setup(x => x.EndOfLine(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("$", 1);
            _factory.Verify();
        }

        [Test]
        public void Motion_Dollar2()
        {
            _util
                .Setup(x => x.EndOfLine(2))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("$", 2);
            _factory.Verify();
        }

        [Test]
        public void Motion_End()
        {
            _util
                .Setup(x => x.EndOfLine(1))
                .Returns(CreateMotionData())
                .Verifiable();
            _capture.GetMotion(KeyInputUtil.VimKeyToKeyInput(VimKey.End), FSharpOption<int>.None);
            _factory.Verify();
        }

        [Test]
        public void BeginingOfLine1()
        {
            _util
                .Setup(x => x.BeginingOfLine())
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("0", 1);
            _factory.Verify();
        }

        [Test]
        public void FirstNonWhitespaceOnLine1()
        {
            _util
                .Setup(x => x.FirstNonWhitespaceOnLine())
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("^", 1);
            _factory.Verify();
        }


        [Test]
        public void Motion_aw1()
        {
            _util
                .Setup(x => x.AllWord(WordKind.NormalWord, 1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("aw", 1);
            _factory.Verify();
        }

        [Test]
        public void Motion_aw2()
        {
            _util
                .Setup(x => x.AllWord(WordKind.NormalWord, 2))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("aw", 2);
            _factory.Verify();
        }

        [Test]
        public void Motion_H()
        {
            _util
                .Setup(x => x.LineFromTopOfVisibleWindow(FSharpOption<int>.None))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("H");
            _factory.Verify();
        }

        [Test]
        public void Motion_aW1()
        {
            _util
                .Setup(x => x.AllWord(WordKind.BigWord, 1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("aW", 1);
            _factory.Verify();
        }

        [Test]
        public void CharLeft1()
        {
            _util
                .Setup(x => x.CharLeft(1))
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete("h", 1);
            _factory.Verify();
        }

        [Test]
        public void CharLeft2()
        {
            _util
                .Setup(x => x.CharLeft(2))
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete("2h", 1);
            _factory.Verify();
        }

        [Test]
        public void CharRight1()
        {
            _util
                .Setup(x => x.CharRight(2))
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete("2l", 1);
            _factory.Verify();
        }

        [Test]
        public void LineUp1()
        {
            _util
                .Setup(x => x.LineUp(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("k", 1);
            _factory.Verify();
        }

        [Test]
        public void EndOfWord1()
        {
            _util
                .Setup(x => x.EndOfWord(WordKind.NormalWord, 1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("e", 1);
            _factory.Verify();
        }

        public void EndOfWord2()
        {
            _util
                .Setup(x => x.EndOfWord(WordKind.BigWord, 1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("E", 1);
            _factory.Verify();
        }

        [Test]
        public void ForwardChar1()
        {
            _util
                .Setup(x => x.ForwardChar('c', 1))
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete("fc", 1);
            _factory.Verify();
        }

        [Test]
        public void ForwardTillChar1()
        {
            _util
                .Setup(x => x.ForwardTillChar('c', 1))
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete("tc", 1);
            _factory.Verify();
        }

        [Test]
        public void BackwardCharMotion1()
        {
            _util
                .Setup(x => x.BackwardChar('c', 1))
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete("Fc", 1);
            _factory.Verify();
        }

        [Test]
        public void BackwardTillCharMotion1()
        {
            _util
                .Setup(x => x.BackwardTillChar('c', 1))
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete("Tc", 1);
            _factory.Verify();
        }

        [Test]
        public void Motion_G1()
        {
            _util
                .Setup(x => x.LineOrLastToFirstNonWhitespace(FSharpOption<int>.None))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("G");
            _factory.Verify();
        }

        [Test]
        public void Motion_G2()
        {
            _util
                .Setup(x => x.LineOrLastToFirstNonWhitespace(FSharpOption.Create(1)))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("1G");
            _factory.Verify();
        }

        [Test]
        public void Motion_G3()
        {
            _util
                .Setup(x => x.LineOrLastToFirstNonWhitespace(FSharpOption.Create(42)))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("42G");
            _factory.Verify();
        }

        [Test]
        public void Motion_gg1()
        {
            _util
                .Setup(x => x.LineOrFirstToFirstNonWhitespace(FSharpOption<int>.None))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("gg");
            _factory.Verify();
        }

        [Test]
        public void Motion_gg2()
        {
            _util
                .Setup(x => x.LineOrFirstToFirstNonWhitespace(FSharpOption.Create(2)))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("2gg");
            _factory.Verify();
        }

        [Test]
        public void Motion_g_1()
        {
            _util
                .Setup(x => x.LastNonWhitespaceOnLine(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("g_");
            _factory.Verify();
        }

        [Test]
        public void Motion_g_2()
        {
            _util
                .Setup(x => x.LastNonWhitespaceOnLine(2))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("2g_");
            _factory.Verify();
        }

        [Test]
        public void Motion_M_1()
        {
            _util
                .Setup(x => x.LineInMiddleOfVisibleWindow())
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("M");
            _factory.Verify();
        }

        [Test]
        public void Motion_L_1()
        {
            _util
                .Setup(x => x.LineFromBottomOfVisibleWindow(FSharpOption.Create(2)))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("2L");
            _factory.Verify();
        }

        [Test]
        public void Motion_L_2()
        {
            _util
                .Setup(x => x.LineFromBottomOfVisibleWindow(FSharpOption<int>.None))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("L");
            _factory.Verify();
        }

        [Test]
        public void Motion_underscore1()
        {
            _util
                .Setup(x => x.LineDownToFirstNonWhitespace(0))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("_");
            _factory.Verify();
        }

        [Test]
        public void Motion_underscore2()
        {
            _util
                .Setup(x => x.LineDownToFirstNonWhitespace(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("2_");
            _factory.Verify();
        }

        [Test]
        public void Motion_lastCharForward1()
        {
            _host.Setup(x => x.Beep()).Verifiable();
            _data.MakeLastCharSearchNone();
            var res = Process(";", null);
            Assert.IsTrue(res.IsError);
            _factory.Verify();
        }

        [Test]
        public void Motion_lastCharForward2()
        {
            int? count = null;
            _data.MakeLastCharSearch(
                c => { count = c; },
                _ => { throw new Exception(); });
            var res = Process(";", null);
            Assert.IsTrue(res.IsError);
            Assert.AreEqual(1, count.Value);
        }

        [Test]
        public void Motion_lastCharForward3()
        {
            int? count = null;
            _data.MakeLastCharSearch(
                c => { count = c; return CreateMotionData(); },
                _ => { throw new Exception(); });
            ProcessComplete("3;");
            Assert.AreEqual(3, count.Value);
        }

        [Test]
        public void Motion_lastCharBackward1()
        {
            _host.Setup(x => x.Beep()).Verifiable();
            _data.MakeLastCharSearchNone();
            var res = Process(",", null);
            Assert.IsTrue(res.IsError);
            _factory.Verify();
        }

        [Test]
        public void Motion_lastCharBackward2()
        {
            int? count = null;
            _data.MakeLastCharSearch(
                _ => { throw new Exception(); },
                c => { count = c; return CreateMotionData(); });
            ProcessComplete(",");
            Assert.AreEqual(1, count.Value);
        }

        [Test]
        public void Motion_lastCharBackward3()
        {
            int? count = null;
            _data.MakeLastCharSearch(
                _ => { throw new Exception(); },
                c => { count = c; return CreateMotionData(); });
            ProcessComplete("3,");
            Assert.AreEqual(3, count.Value);
        }

        [Test]
        public void Motion_SentenceForward1()
        {
            _util
                .Setup(x => x.SentenceForward(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete(")");
            _factory.Verify();
        }

        [Test]
        public void Motion_SentenceForward2()
        {
            _util
                .Setup(x => x.SentenceForward(2))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("2)");
            _factory.Verify();
        }

        [Test]
        public void Motion_SentenceBackward1()
        {
            _util
                .Setup(x => x.SentenceBackward(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("(");
            _factory.Verify();
        }

        [Test]
        public void Motion_SentenceBackward2()
        {
            _util
                .Setup(x => x.SentenceBackward(3))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("3(");
            _factory.Verify();
        }

        [Test]
        public void Motion_SentenceForwardFull1()
        {
            _util
                .Setup(x => x.SentenceFullForward(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("as");
            _factory.Verify();
        }

        [Test]
        public void Motion_SentenceForwardFull2()
        {
            _util
                .Setup(x => x.SentenceFullForward(3))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("3as");
            _factory.Verify();
        }

        [Test]
        public void Motion_ParagraphForward1()
        {
            _util
                .Setup(x => x.ParagraphForward(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("}");
            _factory.Verify();
        }

        [Test]
        public void Motion_ParagraphForward2()
        {
            _util
                .Setup(x => x.ParagraphForward(3))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("3}");
            _factory.Verify();
        }

        [Test]
        public void Motion_ParagraphBackward1()
        {
            _util
                .Setup(x => x.ParagraphBackward(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("{");
            _factory.Verify();
        }

        [Test]
        public void Motion_ParagraphBackward2()
        {
            _util
                .Setup(x => x.ParagraphBackward(3))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("3{");
            _factory.Verify();
        }

        [Test]
        public void Motion_SectionForward1()
        {
            _util
                .Setup(x => x.SectionForward(MotionArgument.ConsiderCloseBrace, 1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("]]");
            _factory.Verify();
        }

        [Test]
        public void Motion_SectionForward2()
        {
            _util
                .Setup(x => x.SectionForward(MotionArgument.None, 1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("][");
            _factory.Verify();
        }

        [Test]
        public void Motion_SectionBackwardOrOpenBrace1()
        {
            _util
                .Setup(x => x.SectionBackwardOrOpenBrace(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("[[");
            _factory.Verify();
        }

        [Test]
        public void Motion_SectionBackwardOrCloseBrace1()
        {
            _util
                .Setup(x => x.SectionBackwardOrCloseBrace(1))
                .Returns(CreateMotionData())
                .Verifiable();
            ProcessComplete("[]");
            _factory.Verify();
        }

        [Test]
        public void Motion_QuotedString1()
        {
            _util
                .Setup(x => x.QuotedString())
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete(@"a""");
            _factory.Verify();
        }

        [Test]
        public void Motion_QuotedString2()
        {
            _util
                .Setup(x => x.QuotedString())
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete(@"a'");
            _factory.Verify();
        }

        [Test]
        public void Motion_QuotedString3()
        {
            _util
                .Setup(x => x.QuotedString())
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete(@"a`");
            _factory.Verify();
        }

        [Test]
        public void Motion_QuotedStringContents1()
        {
            _util
                .Setup(x => x.QuotedStringContents())
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete(@"i""");
            _factory.Verify();
        }

        [Test]
        public void Motion_QuotedStringContents2()
        {
            _util
                .Setup(x => x.QuotedStringContents())
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete(@"i'");
            _factory.Verify();
        }

        [Test]
        public void Motion_QuotedStringContents3()
        {
            _util
                .Setup(x => x.QuotedStringContents())
                .Returns(FSharpOption.Create(CreateMotionData()))
                .Verifiable();
            ProcessComplete(@"i`");
            _factory.Verify();
        }
    }

}
