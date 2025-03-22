using NUnit.Framework;
using OpenQA.Selenium.BiDi.Extensions.Permissions;
using OpenQA.Selenium.BiDi.Modules.BrowsingContext;
using OpenQA.Selenium.BiDi.Modules.Script;
using OpenQA.Selenium.Environment;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Permissions
{
    internal class PermissionsTests : BiDiTestFixture
    {
        private PermissionsModule _permissions;
        private Communication.BiDiConnection _connection;

        protected override async Task<BiDi> CreateBiDi(IWebDriver driver)
        {
            _connection = await driver.AsBiDiConnectionAsync();
            _permissions = await PermissionsModule.AttachAsync(_connection);
            var bidi = await BiDi.AttachAsync(_connection);
            await _connection.ConnectAsync(default);
            return bidi;
        }

        [Test]
        public async Task SettingPermissionsTest()
        {
            var userContext = await bidi.Browser.CreateUserContextAsync();
            var window = await bidi.BrowsingContext.CreateAsync(ContextType.Window, new()
            {
                ReferenceContext = context,
                UserContext = userContext.UserContext,
                Background = true
            });

            var newPage = EnvironmentManager.Instance.UrlBuilder.CreateInlinePage(new InlinePage()
                .WithBody("<div>new page</div>"));

            await window.NavigateAsync(newPage);

            var before = await window.Script.CallFunctionAsync("""
            async () => (await navigator.permissions.query({ name: "geolocation" })).state
            """, awaitPromise: true, new() { UserActivation = true, });

            Assert.That(before.Result, Is.EqualTo(new StringRemoteValue("prompt")));

            await _permissions.SetPermissionAsync("geolocation", PermissionState.Denied, newPage, userContext.UserContext);

            var after = await window.Script.CallFunctionAsync("""
            async () => (await navigator.permissions.query({ name: "geolocation" })).state
            """, awaitPromise: true, new() { UserActivation = true });

            Assert.That(after.Result, Is.EqualTo(new StringRemoteValue("denied")));
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
