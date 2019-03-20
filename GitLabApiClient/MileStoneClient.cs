using System.Collections.Generic;
using System.Threading.Tasks;
using GitLabApiClient.Internal.Http;
using GitLabApiClient.Internal.Queries;
using GitLabApiClient.Models.MileStones.Request;
using GitLabApiClient.Models.MileStones.Responses;

namespace GitLabApiClient
{
    public sealed class MileStoneClient
    {
        private readonly GitLabHttpFacade _httpFacade;
        private readonly MileStoneQueryBuilder _mileStoneQueryBuilder;

        internal MileStoneClient(
            GitLabHttpFacade httpFacade,
            MileStoneQueryBuilder mileStoneQueryBuilder)
        {
            _httpFacade = httpFacade;
            _mileStoneQueryBuilder = mileStoneQueryBuilder;
        }

        public async Task<IList<MileStoneInfo>> GetMileStonesInfo(string projectId, MileStonesQueryOptions options)
        {
            string query = _mileStoneQueryBuilder.Build($"projects/{projectId}/milestones", options);
            return await _httpFacade.GetPagedList<MileStoneInfo>(query);
        }

        public async Task<IList<MileStoneMergedInfo>> GetMileStoneMergedInfos(string projectId, string mileStoneId)
        {
            string query = _mileStoneQueryBuilder.Build($"projects/{projectId}/milestones/{mileStoneId}/merge_requests", null);
            return await _httpFacade.GetPagedList<MileStoneMergedInfo>(query);
        }
    }
}
