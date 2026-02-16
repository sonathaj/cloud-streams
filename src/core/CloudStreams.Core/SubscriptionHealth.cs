// Copyright © 2024-Present The Cloud Streams Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace CloudStreams.Core;

/// <summary>
/// Represents an object used to describe the health of a cloud event subscription
/// </summary>
[DataContract]
public record SubscriptionHealth
{

    /// <summary>
    /// Gets/sets the name of the subscription
    /// </summary>
    [Required, JsonRequired, MinLength(1)]
    [DataMember(Order = 1, Name = "name", IsRequired = true), JsonPropertyName("name"), YamlMember(Alias = "name")]
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets the namespace of the subscription, if any
    /// </summary>
    [DataMember(Order = 2, Name = "namespace"), JsonPropertyName("namespace"), YamlMember(Alias = "namespace")]
    public virtual string? Namespace { get; set; }

    /// <summary>
    /// Gets/sets the status phase of the subscription
    /// </summary>
    [Required, DefaultValue(SubscriptionStatusPhase.Inactive)]
    [DataMember(Order = 3, Name = "phase", IsRequired = true), JsonPropertyName("phase"), YamlMember(Alias = "phase")]
    public virtual SubscriptionStatusPhase Phase { get; set; }

    /// <summary>
    /// Gets/sets the partition id the subscription is reading from
    /// </summary>
    [DataMember(Order = 4, Name = "partitionId"), JsonPropertyName("partitionId"), YamlMember(Alias = "partitionId")]
    public virtual string? PartitionId { get; set; }

    /// <summary>
    /// Gets/sets the acknowledged offset in the cloud event stream
    /// </summary>
    [DataMember(Order = 5, Name = "ackedOffset"), JsonPropertyName("ackedOffset"), YamlMember(Alias = "ackedOffset")]
    public virtual ulong? AckedOffset { get; set; }

    /// <summary>
    /// Gets/sets the length of the partition
    /// </summary>
    [DataMember(Order = 6, Name = "partitionLength"), JsonPropertyName("partitionLength"), YamlMember(Alias = "partitionLength")]
    public virtual ulong? PartitionLength { get; set; }

    /// <summary>
    /// Gets/sets the lag between the partition length and the acknowledged offset
    /// </summary>
    [DataMember(Order = 7, Name = "lag"), JsonPropertyName("lag"), YamlMember(Alias = "lag")]
    public virtual long? Lag { get; set; }

    /// <summary>
    /// Gets/sets the subscriber's state
    /// </summary>
    [DataMember(Order = 8, Name = "subscriberState"), JsonPropertyName("subscriberState"), YamlMember(Alias = "subscriberState")]
    public virtual SubscriberState? SubscriberState { get; set; }

    /// <summary>
    /// Gets/sets the reason why the subscriber is in its current state, if any
    /// </summary>
    [DataMember(Order = 9, Name = "subscriberReason"), JsonPropertyName("subscriberReason"), YamlMember(Alias = "subscriberReason")]
    public virtual string? SubscriberReason { get; set; }

}
