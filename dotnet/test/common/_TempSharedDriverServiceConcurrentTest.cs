using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;

namespace OpenQA.Selenium
{
    // DELETE IT IN FINAL MERGE
    [Explicit]
    class _TempSharedDriverServiceSharedTest
    {
        ChromeDriverService _service;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _service = ChromeDriverService.CreateDefaultService();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _service.Dispose();
        }

        [Test, Repeat(50)]
        public void ChromeImplicitly()
        {
            using (var driver = new ChromeDriver())
            {

            }
        }

        [Test, Repeat(50)]
        public void ChromeServiceShared()
        {
            using (var driver = new ChromeDriver(_service))
            {

            }
        }
    }
}
