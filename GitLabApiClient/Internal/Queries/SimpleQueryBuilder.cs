using GitLabApiClient.Models.MileStones.Request;

namespace GitLabApiClient.Internal.Queries
{
    internal class SimpleQueryBuilder : QueryBuilder<MileStonesQueryOptions>
    {
        protected override void BuildCore(MileStonesQueryOptions options)
        {
        }
    }
}
