using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CodeEndeavors.ServiceHost.Common.Serialization
{
    //http://stackoverflow.com/questions/6700053/json-net-read-only-properties-support-for-ignoredatamember
    //http://james.newtonking.com/projects/json/help/
    public class SerializeIgnoreContractResolver : DefaultContractResolver
    {
        private string _ignoreType;
        public SerializeIgnoreContractResolver(string ignoreType)
        {
            _ignoreType = ignoreType;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.Ignored |= ShouldIgnore(member);// member.GetCustomAttributes(typeof(SerializeIgnoreAttribute), true).Length > 0;
            return property;
        }

        private bool ShouldIgnore(MemberInfo member)
        {
            var attr = member.GetCustomAttributes(typeof(SerializeIgnoreAttribute), true).SingleOrDefault();
            if (attr != null)
                return (attr as SerializeIgnoreAttribute).IgnoreTypes.Contains(_ignoreType);
            return false;
        }

    }

}
