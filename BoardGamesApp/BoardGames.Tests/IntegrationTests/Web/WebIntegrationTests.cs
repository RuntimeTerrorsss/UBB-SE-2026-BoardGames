using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Web
{
    [TestFixture]
    [Category("Integration")]
    public sealed class WebIntegrationTests
    {
        private WebAppFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public void SetUp()
        {
            factory = new WebAppFactory();
            client = factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task Home_Index_ReturnsHtml()
        {
            var response = await client.GetAsync("/");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Does.Contain("<html").IgnoreCase);
        }
    }
}
