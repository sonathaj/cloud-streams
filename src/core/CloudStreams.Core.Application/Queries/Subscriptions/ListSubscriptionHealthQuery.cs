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

using CloudStreams.Core.Application.Services;
using CloudStreams.Core.Resources;
using System.Runtime.CompilerServices;

namespace CloudStreams.Core.Application.Queries.Subscriptions;

/// <summary>
/// Represents the <see cref="IQuery{TResult}"/> used to list the health of all <see cref="Subscription"/>s
/// </summary>
public class ListSubscriptionHealthQuery
    : Query<IAsyncEnumerable<SubscriptionHealth>>
{

    /// <summary>
    /// Initializes a new <see cref="ListSubscriptionHealthQuery"/>
    /// </summary>
    /// <param name="namespace">The namespace the <see cref="Subscription"/>s to list belong to, if any</param>
    /// <param name="labelSelectors">A <see cref="List{T}"/> containing the <see cref="LabelSelector"/>s used to filter <see cref="Subscription"/>s by</param>
    public ListSubscriptionHealthQuery(string? @namespace = null, List<LabelSelector>? labelSelectors = null)
    {
        this.Namespace = @namespace;
        this.LabelSelectors = labelSelectors;
    }

    /// <summary>
    /// Gets the namespace the <see cref="Subscription"/>s to list belong to, if any
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Gets a <see cref="List{T}"/> containing the <see cref="LabelSelector"/>s used to filter <see cref="Subscription"/>s by
    /// </summary>
    public List<LabelSelector>? LabelSelectors { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="ListSubscriptionHealthQuery"/> instances
/// </summary>
public class ListSubscriptionHealthQueryHandler(IResourceRepository repository, ICloudEventStore eventStore)
    : IQueryHandler<ListSubscriptionHealthQuery, IAsyncEnumerable<SubscriptionHealth>>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<IAsyncEnumerable<SubscriptionHealth>>> HandleAsync(ListSubscriptionHealthQuery query, CancellationToken cancellationToken)
    {
        var collection = await repository.ListAsync(Subscription.ResourceDefinition.Group, Subscription.ResourceDefinition.Version, Subscription.ResourceDefinition.Plural, query.Namespace, query.LabelSelectors, null, null, cancellationToken).ConfigureAwait(false);
        var subscriptions = collection.Items.OfType<Subscription>();
        return this.Ok(this.ComputeHealthAsync(subscriptions, cancellationToken));
    }

    /// <summary>
    /// Computes the health information for the specified <see cref="Subscription"/>s
    /// </summary>
    /// <param name="subscriptions">An <see cref="IEnumerable{T}"/> containing the <see cref="Subscription"/>s to compute health for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="SubscriptionHealth"/> objects</returns>
    protected virtual async IAsyncEnumerable<SubscriptionHealth> ComputeHealthAsync(IEnumerable<Subscription> subscriptions, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var subscription in subscriptions)
        {
            if (cancellationToken.IsCancellationRequested) yield break;
            
            var health = new SubscriptionHealth
            {
                Name = subscription.GetName(),
                Namespace = subscription.GetNamespace(),
                Phase = subscription.Status?.Phase ?? SubscriptionStatusPhase.Inactive,
                PartitionId = subscription.Spec.Partition?.Id,
                AckedOffset = subscription.Status?.Stream?.AckedOffset,
                SubscriberState = subscription.Status?.Subscriber?.State,
                SubscriberReason = subscription.Status?.Subscriber?.Reason
            };

            if (subscription.Spec.Partition != null)
            {
                try
                {
                    var partitionMetadata = await eventStore.GetPartitionMetadataAsync(subscription.Spec.Partition, cancellationToken).ConfigureAwait(false);
                    health.PartitionLength = partitionMetadata.Length;
                    if (health.AckedOffset.HasValue)
                    {
                        health.Lag = (long)partitionMetadata.Length - (long)health.AckedOffset.Value;
                    }
                }
                catch (Exception)
                {
                    // If we can't get partition metadata, leave length and lag as null
                    // This allows the health check to still return other useful information
                }
            }

            yield return health;
        }
    }

}
