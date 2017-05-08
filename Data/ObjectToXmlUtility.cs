using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Data
{
    public static class ObjectToXmlUtility
    {
        public static string Serialize<T>(this T value)
        {
            if (value == null) { return string.Empty; }
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, value);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex) { throw new Exception("An error occurred", ex); }
        }

        public static T Clone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) { return default(T); }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings =
                new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.Objects};
            var serialized = JsonConvert.SerializeObject(source, Formatting.Indented,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects, TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple});
            return JsonConvert.DeserializeObject<T>(serialized, deserializeSettings);
        }
    }
}