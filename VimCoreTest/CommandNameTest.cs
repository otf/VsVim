﻿using System.Linq;
using NUnit.Framework;
using Vim;
using Vim.Extensions;
using Vim.UnitTest;

namespace VimCore.Test
{
    [TestFixture]
    public class CommandNameTest
    {
        private KeyInputSet CreateOne(char c)
        {
            return KeyInputSet.NewOneKeyInput(KeyInputUtil.CharToKeyInput(c));
        }

        private KeyInputSet CreateTwo(char c1, char c2)
        {
            return KeyInputSet.NewTwoKeyInputs(KeyInputUtil.CharToKeyInput(c1), KeyInputUtil.CharToKeyInput(c2));
        }

        private KeyInputSet CreateMany(params char[] all)
        {
            return KeyInputSet.NewManyKeyInputs(all.Select(KeyInputUtil.CharToKeyInput).ToFSharpList());
        }

        [Test]
        public void Add1()
        {
            var name1 = KeyInputSet.NewOneKeyInput(KeyInputUtil.CharToKeyInput('c'));
            var name2 = name1.Add(KeyInputUtil.CharToKeyInput('a'));
            Assert.AreEqual("ca", name2.Name);
        }

        [Test]
        public void Name1()
        {
            var name1 = KeyInputSet.NewOneKeyInput(KeyInputUtil.CharToKeyInput('c'));
            Assert.AreEqual("c", name1.Name);
        }

        [Test]
        public void Equality()
        {
            EqualityUtil.RunAll(
                (left, right) => left == right,
                (left, right) => left != right,
                false,
                true,
                EqualityUnit.Create(CreateOne('a')).WithEqualValues(CreateOne('a')),
                EqualityUnit.Create(CreateOne('a')).WithNotEqualValues(CreateOne('b')),
                EqualityUnit.Create(CreateOne('a')).WithEqualValues(CreateMany('a')),
                EqualityUnit.Create(CreateOne('D')).WithEqualValues(KeyNotationUtil.StringToKeyInputSet("D")),
                EqualityUnit.Create(KeyInputSet.NewOneKeyInput(KeyInputUtil.CharToKeyInput('D'))).WithEqualValues(KeyNotationUtil.StringToKeyInputSet("D")));
        }
    }
}
