using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenQA.Selenium
{
    // DELETE IT IN FINAL MERGE
    [Explicit]
    class _TempSharedDriverServiceTest
    {


        [OneTimeSetUp]
        public void Setup()
        {
            Internal.Logging.Log.SetLevel(Internal.Logging.LogEventLevel.Trace);
        }

        [Test]
        public void ChromeImplicitly()
        {
            using (var driver = new ChromeDriver())
            {
                driver.Close();
                driver.Quit();
                driver.Dispose();
            }

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }

        [Test]
        public void ChromeNormal()
        {
            using (var service = ChromeDriverService.CreateDefaultService())
            {
                using var driver = new ChromeDriver(service);
            }

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }

        [Test]
        public void ChromeShared()
        {
            using (var service = ChromeDriverService.CreateDefaultService())
            {
                using (var driver1 = new ChromeDriver(service))
                {
                    
                }

                using (var driver2 = new ChromeDriver(service))
                {
                    
                }
            }

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }

        [Test]
        public void ChromeSharedConcurrently()
        {
            using (var service = ChromeDriverService.CreateDefaultService())
            {
                var tasks = new List<Task>();
                for (int i = 0; i < 3; i++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        using (var driver = new ChromeDriver(service))
                        {

                        }
                    }));
                }

                Task.WaitAll([.. tasks]);
            }

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }

        [Test]
        public void FirefoxImplicitly()
        {
            using (var driver = new FirefoxDriver())
            {
                driver.Close();
                driver.Quit();
                driver.Dispose();
            }

            Assert.That(Process.GetProcessesByName("geckodriver"), Is.Empty);
        }

        [Test]
        public void FirefoxNormal()
        {
            using (var service = FirefoxDriverService.CreateDefaultService())
            {
                using var driver = new FirefoxDriver(service);
            }

            Assert.That(Process.GetProcessesByName("geckodriver"), Is.Empty);
        }

        [Test]
        public void FirefoxShared()
        {
            using (var service = FirefoxDriverService.CreateDefaultService())
            {
                using (var driver1 = new FirefoxDriver(service))
                {
                    
                }

                using (var driver2 = new FirefoxDriver(service))
                {
                    
                }
            }

            Assert.That(Process.GetProcessesByName("geckodriver"), Is.Empty);
        }

        [Test]
        public void FirefoxSharedConcurrently()
        {
            using (var service = FirefoxDriverService.CreateDefaultService())
            {
                var tasks = new List<Task>();
                for (int i = 0; i < 3; i++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        using (var driver = new FirefoxDriver(service))
                        {

                        }
                    }));
                }

                Task.WaitAll([.. tasks]);
            }

            Assert.That(Process.GetProcessesByName("geckodriver"), Is.Empty);
        }

        [Test]
        public void SafariImplicitly()
        {
            using (var driver = new SafariDriver())
            {
                driver.Close();
                driver.Quit();
                driver.Dispose();
            }

            Assert.That(Process.GetProcessesByName("safaridriver"), Is.Empty);
        }

        [Test]
        public void SafariNormal()
        {
            using (var service = SafariDriverService.CreateDefaultService())
            {
                using var driver = new SafariDriver(service);
            }

            Assert.That(Process.GetProcessesByName("safaridriver"), Is.Empty);
        }

        [Test]
        public void SafariShared()
        {
            using (var service = SafariDriverService.CreateDefaultService())
            {
                using (var driver1 = new SafariDriver(service))
                {
                    
                }

                using (var driver2 = new SafariDriver(service))
                {
                    
                }
            }

            Assert.That(Process.GetProcessesByName("safaridriver"), Is.Empty);
        }

        [Test]
        public void SafariSharedConcurrently()
        {
            using (var service = SafariDriverService.CreateDefaultService())
            {
                var tasks = new List<Task>();
                for (int i = 0; i < 3; i++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        using (var driver = new SafariDriver(service))
                        {

                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }

            Assert.That(Process.GetProcessesByName("safaridriver"), Is.Empty);
        }

        [Test]
        public void InternetExplorerImplicitly()
        {
            using (var driver = new InternetExplorerDriver())
            {
                driver.Close();
                driver.Quit();
                driver.Dispose();
            }

            Assert.That(Process.GetProcessesByName("IEDriverServer"), Is.Empty);
        }

        [Test]
        public void InternetExplorerNormal()
        {
            using (var service = InternetExplorerDriverService.CreateDefaultService())
            {
                using var driver = new InternetExplorerDriver(service);
            }

            Assert.That(Process.GetProcessesByName("IEDriverServer"), Is.Empty);
        }

        [Test]
        public void InternetExplorerShared()
        {
            using (var service = InternetExplorerDriverService.CreateDefaultService())
            {
                using (var driver1 = new InternetExplorerDriver(service))
                {
                    
                }

                using (var driver2 = new InternetExplorerDriver(service))
                {
                    
                }
            }

            Assert.That(Process.GetProcessesByName("IEDriverServer"), Is.Empty);
        }

        [Test]
        public void InternetExplorerSharedConcurrently()
        {
            using (var service = InternetExplorerDriverService.CreateDefaultService())
            {
                var tasks = new List<Task>();
                for (int i = 0; i < 3; i++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        using (var driver = new InternetExplorerDriver(service))
                        {

                        }
                    }));
                }

                Task.WaitAll([.. tasks]);
            }

            Assert.That(Process.GetProcessesByName("IEDriverServer"), Is.Empty);
        }
    }
}
