using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IniParserLTK;

namespace Tests
{
    [TestClass]
    public class TestIniParser
    {
        [TestMethod]
        public void TestIniParserInitializeFromString()
        {
            IniParser ini = new IniParser();
            ini.Initialize(Properties.Resources.TestIniParser);

            Assert.AreEqual(ini.GetSetting("Abc", "a"), "b");
            Assert.AreEqual(ini.GetSetting("Def", "a"), "d");
            Assert.AreEqual(ini.GetSetting("Abc", "c"), "d");
            Assert.AreEqual(ini.GetSetting("Def", "c"), "d");

        }
    }
}

/*
[Abc]
a=b
c=d

[Def]
Inherit=Abc
a=d
*/