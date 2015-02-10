using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IniParserLTK;

namespace IniParserLTK.Tests
{
    [TestClass]
    public class TestIniParser
    {
        [TestMethod]
        public void TestIniParserInitializeFromString()
        {
            var resourcePath = @"..\..\Resources\TestIniParser.txt";
            // Test initialize methods
            IniParser ini = new IniParser();
            ini.Initialize(Properties.Resources.TestIniParser);
            AssertEqual(ref ini);

            // Test constructor
            ini = new IniParser(resourcePath);
            AssertEqual(ref ini);

            // Test text reader
            ini = new IniParser();
            ini.Read(resourcePath);
            AssertEqual(ref ini);
        }

        public void AssertEqual(ref IniParser ini)
        {
            Assert.AreEqual(ini.GetSetting("Abc", "a"), "b");
            Assert.AreEqual(ini.GetSetting("Def", "a"), "d");
            Assert.AreEqual(ini.GetSetting("Abc", "c"), "d");
            Assert.AreEqual(ini.GetSetting("Def", "c"), "d");
            Assert.AreEqual(ini.GetSetting("xxx", "yyy"), null);
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