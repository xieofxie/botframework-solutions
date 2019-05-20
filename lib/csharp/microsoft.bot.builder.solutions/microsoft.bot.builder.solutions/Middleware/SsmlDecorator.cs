using System;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Bot.Builder.Solutions.Middleware
{
    public static class SsmlDecorator
    {
        private const string DefaultLocale = "EN-US";

        private const string DefaultVoiceFont = "Microsoft Server Speech Text to Speech Voice (en-US, Jessa24kRUS)";

        private static readonly XNamespace NS = @"https://www.w3.org/2001/10/synthesis";

        public static string Decorate(string spokenText, string locale, string voiceFont)
        {
            if (string.IsNullOrWhiteSpace(spokenText))
            {
                return spokenText?.Trim();
            }

            if (string.IsNullOrWhiteSpace(locale))
            {
                locale = DefaultLocale;
            }

            if (string.IsNullOrWhiteSpace(voiceFont))
            {
                voiceFont = DefaultVoiceFont;
            }

            XElement rootElement = null;
            try
            {
                rootElement = XElement.Parse(spokenText);
            }
            catch (XmlException)
            {
                // Ignore any exceptions. This is effectively a "TryParse", except that XElement doesn't
                // have a TryParse method.
            }

            if (rootElement == null || rootElement.Name.LocalName != "speak")
            {
                // If the text is not valid XML, or if it's not a <speak> node, treat it as plain text.
                rootElement = new XElement(NS + "speak", spokenText);
            }

            AddAttributeIfMissing(rootElement, "version", "1.0");
            AddAttributeIfMissing(rootElement, XNamespace.Xml + "lang", locale);
            AddAttributeIfMissing(rootElement, XNamespace.Xmlns + "mstts", "https://www.w3.org/2001/mstts");

            var sayAsElements = rootElement.Elements("say-as");
            foreach (var element in sayAsElements)
            {
                EditAttributeIfPresent(element, "interpret-as", "digits", "number_digit");
            }

            // add voice element if absent
            AddVoiceElementIfMissing(rootElement, voiceFont);

            return rootElement.ToString(SaveOptions.DisableFormatting);
        }

        private static void AddAttributeIfMissing(XElement element, XName attributeName, string attributeValue)
        {
            var existingAttribute = element.Attribute(attributeName);
            if (existingAttribute == null)
            {
                element.Add(new XAttribute(attributeName, attributeValue));
            }
        }

        private static void AddVoiceElementIfMissing(XElement parent, string attributeValue)
        {
            try
            {
                var existingVoiceElement = parent.Element("voice") ?? parent.Element(NS + "voice");

                // if null (absent), then add. If at least 1 present -- assume author knows what they are doing
                if (existingVoiceElement == null)
                {
                    var existingNodes = parent.DescendantNodes();

                    XElement voiceElement = new XElement("voice", new XAttribute("name", attributeValue));
                    voiceElement.Add(existingNodes);
                    parent.RemoveNodes();
                    parent.Add(voiceElement);
                }
                else
                {
                    existingVoiceElement.SetAttributeValue("name", attributeValue);
                }
            }
            catch (Exception c)
            {
                System.Diagnostics.Trace.TraceError(c.ToString());
            }
        }

        private static void EditAttributeIfPresent(XElement element, XName attributeName, string currentAttributeValue, string newAttributeValue)
        {
            var existingAttribute = element?.Attribute(attributeName);
            if (existingAttribute?.Value == currentAttributeValue)
            {
                existingAttribute?.SetValue(newAttributeValue);
            }
        }
    }
}
