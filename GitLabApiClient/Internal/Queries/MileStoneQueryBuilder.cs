using System;
using System.Collections.Generic;
using System.Text;
using GitLabApiClient.Internal.Utilities;
using GitLabApiClient.Models.MileStones.Request;

namespace GitLabApiClient.Internal.Queries
{
    internal class MileStoneQueryBuilder : QueryBuilder<MileStonesQueryOptions>
    {
        protected override void BuildCore(MileStonesQueryOptions options)
        {
            if (!(options is MileStonesQueryOptions mileStonesQueryOptions))
            {
                return;
            }

            string stateQueryValue = GetStateQueryValue(options.State);
            if (!stateQueryValue.IsNullOrEmpty())
                Add("state", stateQueryValue);
        }

        private static string GetStateQueryValue(MileStoneState state)
        {
            switch (state)
            {
                case MileStoneState.Active:
                    return "active";
                case MileStoneState.Closed:
                    return "closed";
                default:
                    throw new NotSupportedException($"State {state} is not supported");
            }
        }
    }
}
