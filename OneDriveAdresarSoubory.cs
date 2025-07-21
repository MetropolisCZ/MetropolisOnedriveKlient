using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetropolisOnedriveKlient
{
    public class OneDriveAdresarSoubory
    {
        [JsonProperty("@odata.etag")]
        public string Odataetag { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public DateTime LastModifiedDateTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("folder")]
        public FolderTrida Folder { get; set; }

        [JsonProperty("thumbnails")]
        public List<ThumbnailSet> Thumbnails { get; set; }

        [JsonProperty("file")]
        public FileTrida File { get; set; }

        [JsonProperty("shared")]
        public Shared Shared { get; set; }

    }

    public class Shared
    {
        [JsonProperty("owner")]
        public Owner Owner { get; set; }
    }

    public class Owner
    {
        [JsonProperty("user")]
        public User User { get; set; }
    }

    public class User
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

    public class FolderTrida
    {
        [JsonProperty("childCount")]
        public int ChildCount { get; set; }

        [JsonProperty("view")]
        public ViewTrida View { get; set; }
    }

    public class ViewTrida
    {
        [JsonProperty("sortBy")]
        public string SortBy { get; set; }

        [JsonProperty("sortOrder")]
        public string SortOrder { get; set; }

        [JsonProperty("viewType")]
        public string ViewType { get; set; }
    }

    public class ThumbnailSet
    {
        [JsonProperty("small")]
        public Thumbnail Small { get; set; }

        [JsonProperty("medium")]
        public Thumbnail Medium { get; set; }

        [JsonProperty("large")]
        public Thumbnail Large { get; set; }
    }

    public class Thumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }

    public class FileTrida
    {
        [JsonProperty("mimeType")]
        public string MimeType { get; set; }
    }
}
