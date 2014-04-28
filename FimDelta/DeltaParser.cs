using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using System.Collections;
using FimDelta.Xml;

namespace FimDelta
{

    class DeltaParser
    {

        public static Delta ReadDelta(string sourceFile, string targetFile, string deltaFile)
        {
            var exportSerializer = new XmlSerializer(typeof(Export));
            var deltaSerializer = new XmlSerializer(typeof(Delta));

            Export source, target;
            Delta delta;
            using (var r = XmlReader.Create(sourceFile))
                source = (Export)exportSerializer.Deserialize(r);

            using (var r = XmlReader.Create(targetFile))
                target = (Export)exportSerializer.Deserialize(r);

            using (var r = XmlReader.Create(deltaFile))
                delta = (Delta)deltaSerializer.Deserialize(r);

            delta.Source = source;
            delta.Target = target;

            return delta;
        }

        public static void SaveDelta(Delta delta, string file)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("xsd", "http://www.w3.org/2001/XMLSchema");

            var serializer = new XmlSerializer(typeof(Delta));

            var list = new List<ImportObject>();
            foreach (var obj in delta.Objects.Where(x => x.NeedsInclude()))
            {
                var newObj = new ImportObject();
                newObj.SourceObjectIdentifier = obj.SourceObjectIdentifier;
                newObj.TargetObjectIdentifier = obj.TargetObjectIdentifier;
                newObj.ObjectType = obj.ObjectType;
                newObj.State = obj.State;
                newObj.Changes = obj.Changes != null ? obj.Changes.Where(x => x.IsIncluded).ToArray() : null;
                newObj.AnchorPairs = obj.AnchorPairs;
                list.Add(newObj);
            }

            var newDelta = new Delta();
            newDelta.Objects = list.ToArray();

            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Indent = true;
            using (var w = XmlWriter.Create(file, settings))
                serializer.Serialize(w, newDelta, ns);
        }

        public static void SaveExclusions(Delta delta, string file)
        {
            List<ExclusionObject> objectList = new List<ExclusionObject>();

            foreach (var deltaObject in delta.Objects)
            {

                List<ExclusionAttribueValue> excludedChangeList = new List<ExclusionAttribueValue>();
                bool allChangesExcluded = false;

                if (deltaObject.Changes != null)
                {
                    foreach (var change in deltaObject.Changes)
                    {
                        if (change.IsIncluded == false)
                        {
                            excludedChangeList.Add(new ExclusionAttribueValue(change.Operation, change.AttributeName, change.AttributeValue));
                        }
                    }

                    allChangesExcluded = (deltaObject.Changes.Length == excludedChangeList.Count);
                   
                }

                if (excludedChangeList.Count > 0 || deltaObject.IsIncluded == false)
                {
                    ExclusionObject eo = new ExclusionObject(deltaObject.SourceObjectIdentifier, deltaObject.TargetObjectIdentifier, deltaObject.ObjectType);

                    //TODO: Consider not writing out the changes if they are all excluded. We would
                    // need to make sure that the load process would properly exclude all sub objects
                    eo.Changes = excludedChangeList.ToArray();

                    eo.AllChangesExcluded = allChangesExcluded;
                    objectList.Add(eo);
                }
            }

            var serializer = new XmlSerializer(typeof(List<ExclusionObject>));

            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Indent = true;
            using (var w = XmlWriter.Create(file, settings))
                serializer.Serialize(w, objectList);

            System.Windows.MessageBox.Show(string.Format("Saved {0} object exclusions.", objectList.Count));


        }

        public static void LoadExclusions(Delta delta, string file)
        {
            var serializer = new XmlSerializer(typeof(List<ExclusionObject>));

            List<ExclusionObject> objectList;

            try
            {
                using (var r = XmlReader.Create(file))
                    objectList = (List<ExclusionObject>)serializer.Deserialize(r);
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.MessageBox.Show(string.Format("Unable to deserialize XML. Verify that you loaded an exclusion file."));
                return;
            }

            int foundObjectCount = 0;
            int notFoundObjectCount = 0;
            int foundAttributeCount = 0;
            int notFoundAttributeCount = 0;

            foreach (ExclusionObject exclusion_object in objectList)
            {
                try
                {
                    ImportObject o = delta.Objects.First(x => x.SourceObjectIdentifier == exclusion_object.SourceObjectIdentifier && x.TargetObjectIdentifier == exclusion_object.TargetObjectIdentifier);
                    foundObjectCount++;

                    try
                    {
                        foreach (var change in exclusion_object.Changes)
                        {
                            ImportChange c = o.Changes.First(x => x.Operation == change.Operation && x.AttributeName == change.AttributeName && x.AttributeValue == change.AttributeValue);
                            c.IsIncluded = false;
                            foundAttributeCount++;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        notFoundAttributeCount++;
                    }

                    // If there were no sub changes, exclude the parent
                    if (exclusion_object.Changes.Length == 0)
                    {
                        o.IsIncluded = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    notFoundObjectCount++;
                }
            }

            System.Windows.MessageBox.Show(string.Format("Found and excluded {0} object and {1} attributes. {2} objects and {3} attributes were not found", foundObjectCount, foundAttributeCount, notFoundObjectCount, notFoundAttributeCount));
        }

    }
}
