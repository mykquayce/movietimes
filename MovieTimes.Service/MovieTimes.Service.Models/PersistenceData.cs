using Helpers.Cineworld.Models;
using Helpers.Cineworld.Models.Generated;
using Helpers.Cineworld.Models.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MovieTimes.Service.Models
{
	public class PersistenceData
	{
		public ICollection<CinemaType>? Cinemas { get; set; }
		public ICollection<FilmType>? Films { get; set; }
		public DateTime? LocalLastModified { get; set; }
		public DateTime? RemoteLastModified { get; set; }
		public IDictionary<short, Query>? Queries { get; set; }

		public IEnumerable<short> QueryIds => Queries!.Select(kvp => kvp.Key);

		public QueryResults QueryResults
		{
			get => throw new NotImplementedException();
			set => QueryResultsBag.TryAdd(value);
		}

		public IProducerConsumerCollection<QueryResults> QueryResultsBag { get; } = new ConcurrentBag<QueryResults>();

		public IList<QueryResults> QueryResultsCollections
		{
			get => throw new NotImplementedException();
			set => QueryResultsCollectionBag.TryAdd(value);
		}

		public IProducerConsumerCollection<IList<QueryResults>> QueryResultsCollectionBag { get; } = new ConcurrentBag<IList<QueryResults>>();

		public string? Message
		{
			get => throw new NotImplementedException();
			set => MessagesBag.TryAdd(value!);
		}
		public IProducerConsumerCollection<string> MessagesBag { get; } = new ConcurrentBag<string>();
	}
}
