using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using ExoLive.Server.Common.Internationalization;

namespace ExoLive.Server.Common.Models
{
    public class Company
    {
        private LangResourceBarrel _barrel;

        public Company()
        {
            Id = Guid.NewGuid().ToString();
            _barrel = new LangResourceBarrel(null);
            _name = new LangString(_barrel, Id, "Name");
        }

        public string Id { get; set; }

        public void SetBarrel(LangResourceBarrel barrel)
        {
            _barrel = barrel;
            _name = new LangString(_barrel, Id, "Name");
        }

        public List<LangResource> GetLangResources()
        {
            if (_barrel == null) return null;
            return (List<LangResource>)_barrel.GetResources(Id);
        }

        private LangString _name;

        public LangString Name
        {
            get { return _name; }
        }
    }
}
