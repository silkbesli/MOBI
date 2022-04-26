using NUnit.Framework;
using System.IO;

namespace HelperPackLibrary.NUnitTests
{
    public class PackerTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Pack()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\") + "\\DATA\\Packer\\Pack\\"; 

            foreach (string file in Directory.EnumerateFiles(path + "\\Input\\", "*.txt"))
            {
                var Result = HelperPackLibrary.Packer.Pack(file);

                var testResult = File.ReadAllText(path + "\\Output\\" + Path.GetFileName(file));
                Assert.AreEqual(Result, testResult);

            }
        }
    }
}