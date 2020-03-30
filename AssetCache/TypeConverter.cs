using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace AssetCache {
    public class TypeConverter : IYamlTypeConverter {
        private static readonly Type _acceptedType = typeof(Component);
        private static readonly Type _mappingStartType = typeof(MappingStart);
        private static readonly Type _mappingEndType = typeof(MappingEnd);
        private static readonly Type _sequenceEndType = typeof(SequenceEnd);
        private static readonly Type _sequenceStartType = typeof(SequenceStart);

        public bool Accepts(Type type) {
            return type == _acceptedType;
        }

        public object ReadYaml(IParser parser, Type type) {
            KeyValuePair<string, Component> result;

            if (parser.Current.GetType() != _mappingStartType) {
                throw new InvalidDataException("Invalid YAML content.");
            }

            string key = GetScalarValue(parser);
            Console.WriteLine(key);
            parser.MoveNext(); // skip the scalar property name

            do {
                parser.MoveNext();
                parser.MoveNext();
                Console.WriteLine(GetScalarValue(parser));

                // do something with the current node

                parser.MoveNext();
            } while (parser.Current.GetType() != _mappingEndType);

            parser.MoveNext();
            return null;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type) {
            throw new NotImplementedException();
        }

        private string GetScalarValue(IParser parser) {
            Scalar scalar = (Scalar) parser.Current;

            if (scalar == null) {
                throw new InvalidDataException("Failed to retrieve scalar value.");
            }

            return scalar.Value;
        }
    }
}