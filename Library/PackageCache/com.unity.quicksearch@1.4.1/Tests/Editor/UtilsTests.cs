using NUnit.Framework;

namespace Unity.QuickSearch
{
    internal class UtilsTests
    {
        [Test]
        public void GetPackagesPaths()
        {
            var packagePaths = Utils.GetPackagesPaths();
            Assert.Contains("Packages/com.unity.quicksearch", packagePaths);
        }
    }
}
