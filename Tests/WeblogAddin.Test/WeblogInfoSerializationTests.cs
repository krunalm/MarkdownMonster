using MarkdownMonster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace WeblogAddin.Test
{
    /// <summary>
    /// Verifies that WeblogInfo persists the blog password encrypted
    /// rather than as plaintext, while still round-tripping transparently
    /// and accepting legacy plaintext configs.
    /// </summary>
    [TestClass]
    public class WeblogInfoSerializationTests
    {
        [TestMethod]
        public void Password_IsEncryptedInJson()
        {
            var info = new WeblogInfo
            {
                Name = "Test",
                Username = "rick",
                Password = "sup3rS3cret!",
                ApiUrl = "https://example.com/xmlrpc.php"
            };

            string json = JsonConvert.SerializeObject(info);

            Assert.IsFalse(json.Contains("sup3rS3cret!"),
                "Serialized JSON must not contain the plaintext password.");
            StringAssert.Contains(json, "\"Password\":\"enc:",
                "Serialized JSON should emit the encrypted Password field.");
        }

        [TestMethod]
        public void Password_RoundTripsThroughJson()
        {
            var original = new WeblogInfo { Password = "round-trip-me" };
            var json = JsonConvert.SerializeObject(original);
            var restored = JsonConvert.DeserializeObject<WeblogInfo>(json);

            Assert.AreEqual("round-trip-me", restored.Password);
        }

        [TestMethod]
        public void LegacyPlaintextPassword_IsLoadedUnchanged()
        {
            // Guards: configs written before the encryption change have
            // a bare "Password":"plain" field. Loading must return the
            // plaintext so existing users don't lose their credentials.
            const string legacyJson = "{\"Id\":\"abc\",\"Name\":\"Legacy\",\"Password\":\"oldplain\"}";
            var info = JsonConvert.DeserializeObject<WeblogInfo>(legacyJson);

            Assert.AreEqual("oldplain", info.Password);
        }

        [TestMethod]
        public void EmptyPassword_StaysEmptyThroughRoundTrip()
        {
            var info = new WeblogInfo { Password = "" };
            var json = JsonConvert.SerializeObject(info);
            var restored = JsonConvert.DeserializeObject<WeblogInfo>(json);

            Assert.AreEqual(string.Empty, restored.Password ?? string.Empty);
        }
    }
}
