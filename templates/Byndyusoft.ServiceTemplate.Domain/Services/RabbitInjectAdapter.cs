namespace Byndyusoft.ServiceTemplate.Domain.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using OpenTracing.Propagation;

    public class RabbitInjectAdapter : ITextMap
    {
        private readonly IDictionary<string, object> _dictionary;

        public RabbitInjectAdapter(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Set(string key, string value)
        {
            _dictionary.Add(key, value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}