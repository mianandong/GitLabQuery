﻿using Newtonsoft.Json;

namespace GitLabApiClient.Models
{
    public sealed class Milestone : ModifiableObject
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("due_date")]
        public string DueDate { get; set; }

        [JsonProperty("project_id")]
        public int ProjectId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
