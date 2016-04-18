using System;
using System.Collections.Generic;
using System.Text;

namespace CodeEndeavors.ServiceHost.Common.Serialization
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SerializeIgnoreAttribute : System.Attribute
    {
        public string[] IgnoreTypes { get; set; }
        public SerializeIgnoreAttribute(string[] ignoreTypes)
        {
            IgnoreTypes = ignoreTypes ?? new string[] { };
        }
        public SerializeIgnoreAttribute(string ignoreType)
        {
            IgnoreTypes = new string[] { ignoreType };
        }

    }
}
