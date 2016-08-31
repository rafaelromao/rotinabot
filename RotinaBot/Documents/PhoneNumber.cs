using System;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace RotinaBot.Documents
{
    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    public class PhoneNumber : Document
    {
        private const string OWNER_KEY = "owner";
        private const string VALUE_KEY = "value";

        [DataMember(Name = OWNER_KEY)]
        public string Owner { get; set; }
        [DataMember(Name = VALUE_KEY)]
        public string Value { get; set; }
        public PhoneNumber() : base(MediaType.Parse("application/x-phonenumber+json"))
        {
        }
    }
}
