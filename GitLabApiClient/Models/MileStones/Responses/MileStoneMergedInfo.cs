using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GitLabApiClient.Models.MileStones.Responses
{
    [DataContract]
    public class MileStoneMergedInfo
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "merged_by")]
        public MergedAuthorInfo MergeBy { get; set; }
        [DataMember(Name = "author")]
        public OpendAuthorInfo Opend { get; set; }
        [DataMember(Name = "labels")]
        public List<string> Labels { get; set; }
        [DataMember(Name = "milestone")]
        public MileStoneInfo MileStone { get; set; }
        [DataMember(Name = "state")]
        public string State { get; set; }

        public MileStoneMergedInfo()
        {
            Labels = new List<string>();
        }
    }

    [DataContract]
    public class MergedAuthorInfo
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    [DataContract]
    public class OpendAuthorInfo
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
