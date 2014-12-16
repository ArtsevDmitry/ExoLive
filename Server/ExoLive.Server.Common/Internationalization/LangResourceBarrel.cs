using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ExoLive.Server.Common.Models;

namespace ExoLive.Server.Common.Internationalization
{
    public class LangResourceBarrel
    {
        private readonly object _syncObject = new object();
        private readonly string _defaultLanguageId;
        private readonly Dictionary<string, List<LangResource>> _barrel;

        public LangResourceBarrel(string defaultLanguageId)
        {
            _defaultLanguageId = defaultLanguageId;
            _barrel = new Dictionary<string, List<LangResource>>();
        }

        public void ClearBarrel()
        {
            lock (_syncObject)
            {
                _barrel.Clear();
            }
        }

        public void MergeResource(LangResource resource)
        {
            lock (_syncObject)
            {
                var existingResource = Get(resource.OwnerId, resource.LanguageId, resource.Key);
                if (existingResource != null && existingResource.Id.NoCaseCompare(resource.Id)) return;

                var items = _barrel[resource.OwnerId];
                if (items == null)
                {
                    items = new List<LangResource>();
                    _barrel[resource.OwnerId] = items;
                }

                items.Add(resource);
            }
        }

        public void SetResources(IEnumerable<LangResource> resources)
        {
            lock (_syncObject)
            {
                _barrel.Clear();

                foreach (var langResource in resources)
                {
                    List<LangResource> items;
                    if (!_barrel.ContainsKey(langResource.OwnerId))
                    {
                        items = new List<LangResource>();
                        _barrel[langResource.OwnerId] = items;
                    }
                    else
                    {
                        items = _barrel[langResource.OwnerId];
                    }

                    if (items != null) items.Add(langResource);
                }
            }
        }

        public LangResource Get(string ownerId, string languageId, string key)
        {
            List<LangResource> items;
            lock (_syncObject)
            {
                if (!_barrel.ContainsKey(ownerId)) return null;
                items = _barrel[ownerId];
            }

            IEnumerable<LangResource> langItems = (from LangResource res in items
                                                   where (res.LanguageId == languageId || res.LanguageId == _defaultLanguageId) &&
                                                   (string.Compare(res.Key, key, StringComparison.InvariantCultureIgnoreCase) == 0)
                                                   select res).ToList();
            if (!langItems.Any()) return null;

            var nativeItem = (from LangResource res in langItems
                              where res.LanguageId == languageId
                              select res).FirstOrDefault();
            if (nativeItem != null) return nativeItem;

            var fallbackItem = (from LangResource res in langItems
                                where res.LanguageId == _defaultLanguageId
                                select res).FirstOrDefault();
            return fallbackItem;
        }

        public void Set(string ownerId, string languageId, string key, string value)
        {
            lock (_syncObject)
            {
                List<LangResource> items;
                if (!_barrel.ContainsKey(ownerId))
                {
                    items = new List<LangResource>();
                    _barrel[ownerId] = items;
                }
                items = _barrel[ownerId];
                
                LangResource langItem = (from LangResource res in items
                                         where (res.LanguageId == languageId) && res.Key.NoCaseCompare(key)
                                         select res).FirstOrDefault();

                if (langItem == null)
                {
                    items.Add(new LangResource
                    {
                        Id = Guid.NewGuid().ToString(),
                        Key = key,
                        LanguageId = languageId,
                        OwnerId = ownerId,
                        Value = value
                    });
                }
                else
                {
                    langItem.Value = value;
                }
            }
        }

        public string DefaultLanguageId
        {
            get { return _defaultLanguageId; }
        }

        public IEnumerable<LangResource> GetResources(string ownerId)
        {
            if (!_barrel.ContainsKey(ownerId)) return null;
            var items = _barrel[ownerId];

            var result = from LangResource res in items
                         select res;

            return result.ToList();
        }

    }
}
