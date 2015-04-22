using System;
using System.Web.Caching;

namespace CodeEndeavors.ServiceHost.Common.Services
{
	public class CacheItem
	{
		public string DependencyName;
		public bool HasPendingRequest;
		public string Key;
		public DateTime LastUpdate;
		public CacheItemPriority Priority;
		public bool UsesStale;
		private DateTime _absoluteExpiration;
		public DateTime AbsoluteExpiration
		{
			get
			{
				bool usesStale = this.UsesStale;
				DateTime AbsoluteExpiration;
				if (usesStale)
					AbsoluteExpiration = System.Web.Caching.Cache.NoAbsoluteExpiration;
				else
					AbsoluteExpiration = this._absoluteExpiration;
				return AbsoluteExpiration;
			}
			set
			{
				this._absoluteExpiration = value;
			}
		}
		public bool HasStaleData
		{
			get
			{
				bool flag = !this.UsesStale;
				return !flag && DateTime.Compare(this.StaleExpiration, DateTime.Now) < 0;
			}
		}
		public DateTime StaleExpiration
		{
			get
			{
				bool usesStale = this.UsesStale;
				DateTime StaleExpiration;
				if (usesStale)
				{
					StaleExpiration = this._absoluteExpiration;
				}
				else
				{
					StaleExpiration = DateTime.MaxValue;
				}
				return StaleExpiration;
			}
		}
		public CacheItem(string key)
		{
			this._absoluteExpiration = System.Web.Caching.Cache.NoAbsoluteExpiration;
			this.Key = key;
			this.LastUpdate = DateTime.MinValue;
            this.AbsoluteExpiration = System.Web.Caching.Cache.NoAbsoluteExpiration;
		}
	}
}
