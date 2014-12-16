namespace ExoLive.Server.Common.Internationalization
{
    public class LangString : LangDataType<string>
    {
        public LangString(LangResourceBarrel barrel, string ownerId, string key)
            : base(barrel, ownerId, key)
        {

        }

        public string this[string languageId]
        {
            get
            {
                var item = _barrel.Get(_ownerId, languageId, _key);

                if (item == null) return null;

                return item.Value;
            }
            set
            {
                _barrel.Set(_ownerId, languageId, _key, value);
            }
        }

        public string Value
        {
            get
            {
                var item = _barrel.Get(_ownerId, _barrel.DefaultLanguageId, _key);

                if (item == null) return null;

                return item.Value;
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
