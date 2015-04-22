using CodeEndeavors.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using System.Web.Caching;
using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;

namespace CodeEndeavors.ServiceHost.Common.Services
{
	public class CacheState
	{
		public delegate T PullCacheData<T>(string suffix, ref TimeSpan timeSpan);
		public delegate Dictionary<TID, T> PullMultipleCacheDelegate<TID, T>(List<TID> ids, ref TimeSpan timeSpan);
		private static SafeDictionary<string, CacheItem> _cacheItems = new SafeDictionary<string, CacheItem>();
		private static SafeDictionary<string, bool> _staleCacheKeys = new SafeDictionary<string, bool>();
		public static System.Web.Caching.Cache Cache
		{
			get
			{
				bool flag = HttpContext.Current != null;
                System.Web.Caching.Cache Cache;
				if (flag)
				{
					Cache = HttpContext.Current.Cache;
				}
				else
				{
					Cache = HttpRuntime.Cache;
				}
				return Cache;
			}
		}
		[DebuggerNonUserCode]
		public CacheState()
		{
		}
		public static void ExpireCache(string key)
		{
			CacheState.ExpireCache(key, "");
		}
		public static void ExpireCache(string key, string suffix)
		{
			string cacheKey = CacheState.GetKey(key, suffix);
			foreach (var itemKey in CacheState._cacheItems.Keys)
			{
				bool flag = itemKey.StartsWith(cacheKey + "~");
				if (itemKey.StartsWith(cacheKey + "~"))
					CacheState.ExpireCacheItem(itemKey);
			}
		}
		private static void ExpireCacheItem(string key)
		{
			CacheItem cacheItem = CacheState.GetCacheItem(key);
            if (cacheItem.UsesStale)
			{
				cacheItem.LastUpdate = DateTime.MinValue;
				cacheItem.AbsoluteExpiration = DateTime.MinValue;
			}
			else
			{
				CacheState.RemoveCache(key);
			}
		}
		public static CacheItem GetCacheItem(string key)
		{
			CacheItem item = CacheState._cacheItems[key];
			bool flag = item == null;
			if (flag)
			{
				item = new CacheItem(key);
				CacheState._cacheItems[key] = item;
			}
			return item;
		}
		public static string GetDependencyKey(string key, string suffix, Type valueType)
		{
			return CacheState.GetKey(key, suffix, valueType);
		}
		private static string GetKey(string key, string suffix)
		{
			bool flag = !string.IsNullOrEmpty(suffix);
			string GetKey;
			if (flag)
			{
				GetKey = string.Format("{0}_{1}", key, suffix);
			}
			else
			{
				GetKey = key;
			}
			return GetKey;
		}
		private static string GetKey(string key, string suffix, Type valueType)
		{
			return string.Format("{0}~{1}", CacheState.GetKey(key, suffix), valueType);
		}
		public static T GetState<T>(string key, T defaultValue)
		{
			object obj2 = RuntimeHelpers.GetObjectValue(CacheState.Cache[key]);
			bool flag = obj2 == null;
			T GetState;
			if (flag)
			{
				GetState = defaultValue;
			}
			else
			{
				GetState = RuntimeHelpers.GetObjectValue(obj2).ToType<T>();
			}
			return GetState;
		}
		public static bool HasStaleData(string key)
		{
			return CacheState.GetCacheItem(key).HasStaleData;
		}
		private static void LogCache(string logMsg, string counter, string counterItem)
		{
			Log.Info(logMsg);
		}
		public static T PullCache<T>(string key, bool useCache, CacheState.PullCacheData<T> pullFunc, CacheItemPriority priority)
		{
			return CacheState.PullCache<T>(key, "", useCache, pullFunc, priority);
		}
		public static T PullCache<T>(string key, string suffix, bool useCache, CacheState.PullCacheData<T> pullFunc, CacheItemPriority priority)
		{
			T t = default(T);
			string str = CacheState.GetKey(key, suffix, typeof(T));
			bool flag = CacheState.UsesStaleCache(key);
			if (flag)
			{
				CacheState.RegisterStaleCache(str);
			}
			T state;
			T PullCache;
			if (useCache)
			{
				state = CacheState.GetState<T>(str, default(T));
				flag = (state != null && !CacheState.StaleCacheShouldUpdate(str, true));
				if (flag)
				{
					PullCache = state;
					return PullCache;
				}
				CacheItem cacheItem = CacheState.GetCacheItem(str);
				CacheItem obj = cacheItem;
				Monitor.Enter(obj);
				try
				{
					state = CacheState.GetState<T>(str, default(T));
					flag = (state == null || CacheState.StaleCacheShouldUpdate(str, false));
					if (flag)
					{
						try
						{
							cacheItem.HasPendingRequest = true;
							CacheState.LogCache(string.Format("PullCache<{0}> not found in cache[{1}_{2}]. Retrieving from service.", typeof(T).ToString(), key.ToString(), suffix), "PullCache: " + key.ToString(), "");
							TimeSpan timeSpan = TimeSpan.MinValue;
							state = pullFunc(suffix, ref timeSpan);
							flag = (timeSpan != TimeSpan.MinValue);
							if (flag)
							{
								CacheState.SetState(str, state, timeSpan, priority, null);
							}
							PullCache = state;
							return PullCache;
						}
						finally
						{
							cacheItem.HasPendingRequest = false;
						}
					}
					PullCache = state;
					return PullCache;
				}
				finally
				{
					Monitor.Exit(obj);
				}
			}
			CacheState.LogCache(string.Format("PullCache<{0}> not USING cache[{1}_{2}]. Retrieving from service.", typeof(T).ToString(), key.ToString(), suffix), "PullCache: " + key.ToString(), "");
			TimeSpan minValue = TimeSpan.MinValue;
			state = pullFunc(suffix, ref minValue);
			flag = (minValue != TimeSpan.MinValue);
			if (flag)
			{
				CacheState.SetState(str, state, minValue, priority, null);
			}
			PullCache = state;
			return PullCache;
		}
		public static Dictionary<TID, T> PullMultipleCache<TID, T>(string key, List<TID> ids, bool useCache, CacheState.PullMultipleCacheDelegate<TID, T> pullFunc)
		{
            throw new NotImplementedException();
		}
		public static void RegisterStaleCache(string key)
		{
			CacheState._staleCacheKeys[key] = true;
		}
		public static void RemoveCache(string key)
		{
			CacheState.Cache.Remove(key);
			CacheState.RemoveCacheItem(key);
		}
		public static void RemoveCacheItem(string key)
		{
			CacheState._cacheItems.Remove(key);
		}
		public static void SetState(string key, object value, string fileName)
		{
			bool flag = value == null;
			if (flag)
			{
				CacheState.RemoveCache(key);
			}
			else
			{
				CacheItem cacheItem = CacheState.GetCacheItem(key);
				cacheItem.LastUpdate = DateTime.Now;
				cacheItem.DependencyName = fileName;
				CacheState.Cache.Insert(key, RuntimeHelpers.GetObjectValue(value), new CacheDependency(fileName));
			}
		}
		public static void SetState(string key, object value, DateTime expiryTime, CacheItemPriority priority)
		{
			CacheState.SetState(key, RuntimeHelpers.GetObjectValue(value), expiryTime, priority, null);
		}
		public static void SetState(string key, object value, TimeSpan duration, CacheItemPriority priority)
		{
			CacheState.SetState(key, RuntimeHelpers.GetObjectValue(value), duration, priority, null);
		}
		public static void SetState(string key, object value, DateTime expiryTime, CacheItemPriority priority, string dependencyKey)
		{
			bool flag = value == null;
			if (flag)
			{
				CacheState.RemoveCache(key);
			}
			else
			{
				CacheItem cacheItem = CacheState.GetCacheItem(key);
				cacheItem.LastUpdate = DateTime.Now;
				cacheItem.AbsoluteExpiration = expiryTime;
				cacheItem.Priority = priority;
				cacheItem.UsesStale = CacheState.UsesStaleCache(key);
				CacheDependency dependencies = null;
				flag = !string.IsNullOrEmpty(dependencyKey);
				if (flag)
				{
					string[] cachekeys = dependencyKey.Split(new char[]
					{
						'|'
					});
					dependencies = new CacheDependency(null, cachekeys);
					cacheItem.DependencyName = dependencyKey;
				}
				CacheState.Cache.Insert(key, RuntimeHelpers.GetObjectValue(value), dependencies, cacheItem.AbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, priority, null);
			}
		}
		public static void SetState(string key, object value, TimeSpan duration, CacheItemPriority priority, string dependencyKey)
		{
			DateTime noAbsoluteExpiration = System.Web.Caching.Cache.NoAbsoluteExpiration;
			bool flag = duration < TimeSpan.MaxValue;
			if (flag)
			{
				noAbsoluteExpiration = DateTime.Now.Add(duration);
			}
			CacheState.SetState(key, RuntimeHelpers.GetObjectValue(value), noAbsoluteExpiration, priority, dependencyKey);
		}
		private static bool StaleCacheShouldUpdate(string key, bool logIt)
		{
			bool flag = false;
			CacheItem cacheItem = CacheState.GetCacheItem(key);
			bool flag2 = !cacheItem.HasStaleData;
			bool StaleCacheShouldUpdate;
			if (flag2)
			{
				StaleCacheShouldUpdate = flag;
			}
			else
			{
				if (logIt)
				{
					CacheState.LogCache("HasStaleData: " + key.ToString(), "HasStaleData", "HasStaleData: " + key.ToString());
				}
				flag2 = cacheItem.HasPendingRequest;
				if (flag2)
				{
					if (logIt)
					{
						CacheState.LogCache("UsingStaleData: " + key.ToString(), "UsingStaleData", "UsingStaleData: " + key.ToString());
					}
					StaleCacheShouldUpdate = flag;
				}
				else
				{
					StaleCacheShouldUpdate = true;
				}
			}
			return StaleCacheShouldUpdate;
		}
		public static void UnregisterStaleCache(string key)
		{
			CacheState.RemoveCache(key);
			SafeDictionary<string, bool> staleCacheKeys = CacheState._staleCacheKeys;
			Monitor.Enter(staleCacheKeys);
			try
			{
				bool flag = CacheState._staleCacheKeys.ContainsKey(key);
				if (flag)
				{
					CacheState._staleCacheKeys.Remove(key);
				}
			}
			finally
			{
				Monitor.Exit(staleCacheKeys);
			}
		}
		public static bool UsesStaleCache(string key)
		{
			return CacheState._staleCacheKeys.ContainsKey(key);
		}
	}
}