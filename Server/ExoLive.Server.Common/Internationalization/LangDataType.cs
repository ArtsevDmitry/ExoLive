using System;
using System.Collections.Generic;

namespace ExoLive.Server.Common.Internationalization
{
    public class LangDataType<TNative> //where TNative : IComparable<TNative>, IEquatable<TNative>
    {
        protected readonly LangResourceBarrel _barrel;
        protected string _key;
        protected string _ownerId;
        //private readonly Dictionary<string, TNative> _resources;

        public LangDataType(LangResourceBarrel barrel, string ownerId, string key)
        {
            _barrel = barrel;
            _key = key;
            _ownerId = ownerId;
            //_resources = new Dictionary<string, TNative>();
        }

        //public LangDataType()
        //{
        //    _resources = new Dictionary<string, TNative>();
        //}

        //public TNative this[string languageId]
        //{
        //    get
        //    {
        //        var item = _barrel.Get(_ownerId, languageId, _key);

        //        if (typeof(TNative) == typeof(string))
        //        {
        //            if (item == null) return default(TNative);
                    
        //            return (TNative)Convert.ChangeType(item.Value, Type.GetTypeCode(typeof(TNative)));
        //        }

        //        return default(TNative);
        //    }
        //    set
        //    {
        //        var item = _barrel.Get(_ownerId, languageId, _key);
        //        if (typeof (TNative) == typeof (string))
        //        {

        //        }

        //        //_resources[languageId] = value;
        //    }
        //}


    }
}
