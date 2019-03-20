using System.Runtime.Serialization;

namespace GitLabApiClient.Models.MileStones.Responses
{
    [DataContract]
    public class MileStoneInfo
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "title")]
        public string Titlt { get; set; }
    }
}
