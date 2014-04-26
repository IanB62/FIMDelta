using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FimDelta.Xml
{
    [Serializable]
    public class ExclusionAttribueValue
    {
        [XmlElement]
        public string Operation { get; set; }

        [XmlElement]
        public string AttributeName { get; set; }

        [XmlElement]
        public string AttributeValue { get; set; }


        public ExclusionAttribueValue()
        {
        }

        public ExclusionAttribueValue(string operation, string attributeName, string attributeValue)
        {
            this.Operation = operation;
            this.AttributeName = attributeName;
            this.AttributeValue = attributeValue;
        }
    }
}


