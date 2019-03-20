using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using GitLabApiClient.Internal.Http;
using GitLabApiClient.Internal.Queries;

namespace GitLabApiClient
{
    public sealed class SimpleQueryClient
    {
        private readonly GitLabHttpFacade _httpFacade;
        private readonly SimpleQueryBuilder _simpleQueryBuilder;

        internal SimpleQueryClient(GitLabHttpFacade httpFacade, SimpleQueryBuilder simpleQueryBuilder)
        {
            _httpFacade = httpFacade;
            _simpleQueryBuilder = simpleQueryBuilder;
        }

        public async Task<IList<CommitCo>> GetCommitCount(string projectId)
        {
            string query = _simpleQueryBuilder.Build($"projects/{projectId}/repository/commits", null);
            return await _httpFacade.GetPagedList<CommitCo>(query);
        }

        public async Task CreateLabels(List<LabelData> labels, string color, string projectId)
        {
            foreach (var label in labels)
            {
                var data = new CreateLabelData {Id = projectId, Name = label.Name, Color = color, Description = label.Description};
                await _httpFacade.Post($"projects/{projectId}/labels", data);
            }
        }
    }

    public class LabelData
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    [DataContract]
    public class CreateLabelData
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "color")]
        public string Color { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
    }

    [DataContract]
    public class CommitCo
    {
        [DataMember(Name = "committed_date")]
        public string Date { get; set; }

        [DataMember(Name = "author_name")]
        public string AuthorName { get; set; }
    }
}
