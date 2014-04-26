using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FimDelta.Xml
{
    [Serializable]
    public class ExclusionObject
    {
        [XmlElement]
        public string SourceObjectIdentifier { get; set; }

        [XmlElement]
        public string TargetObjectIdentifier { get; set; }

        [XmlElement]
        public string ResourceType { get; set; }

        [XmlElement]
        public bool AllChangesExcluded { get; set; }

        //[XmlArray("AnchorPairs"), XmlArrayItem("JoinPair")]
        [XmlArray]
        public ExclusionAttribueValue[] Changes { get; set; }

        public ExclusionObject()
        {
            this.AllChangesExcluded = false;
        }

        public ExclusionObject(string sourceObjectIdentifier, string targetObjectIdentifier, string resourceType)
        {
            this.SourceObjectIdentifier = sourceObjectIdentifier;
            this.TargetObjectIdentifier = targetObjectIdentifier;
            this.ResourceType = resourceType;
            this.AllChangesExcluded = false;
        }
    }
}


