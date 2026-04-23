using MarkdownMonster;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace WeblogAddin
{
    public class WeblogInfo
    {
        public WeblogInfo()
        {
            Id = DataUtils.GenerateUniqueId(8);
            BlogId = "1";
        }

        public string Id { get; set; }

        public string Name { get; set; }
        public string Username { get; set; }

        /// <summary>
        /// In-memory plaintext password. Not serialized directly - the
        /// SerializedPassword property below is what ends up on disk and
        /// handles encryption/decryption transparently.
        /// </summary>
        [JsonIgnore]
        public string Password { get; set; }

        /// <summary>
        /// JSON representation of Password. The property is written under
        /// the "Password" name so existing config files continue to load;
        /// legacy plaintext values are detected by DecryptString and
        /// passed through, then re-saved encrypted on the next Write().
        /// </summary>
        [JsonProperty("Password")]
        private string SerializedPassword
        {
            get { return mmApp.EncryptString(Password); }
            set { Password = mmApp.DecryptString(value); }
        }

        public string ApiUrl { get; set; }

        public object BlogId { get; set; }

        public WeblogTypes Type { get; set; } = WeblogTypes.MetaWeblogApi;

        /// <summary>
        /// Url used to preview the post. The postId can be embedded into 
        /// the value by using {0}.
        /// </summary>
        public string PreviewUrl { get; set; }        
    }

    public enum WeblogTypes
    {
        MetaWeblogApi,
        Wordpress,
        Unknown
    }
}