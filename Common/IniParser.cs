using System;
using System.Collections.Generic;
using System.IO;

namespace Common
{
    class INIParser
    {
        private readonly Dictionary<string, Section> _sections;

        public INIParser(string fileName)
        {
            _sections = new Dictionary<string, Section>(StringComparer.OrdinalIgnoreCase);

            ReadFromFile(fileName);
        }


        public bool TryGetValue<T>(string sectionName, string key, out T value) where T : IConvertible
        {
            _sections.TryGetValue(sectionName, out var section);

            if (section != null && section.Values.TryGetValue(key, out var tmp))
            {
                try
                {
                    value = (T)Convert.ChangeType(tmp, typeof(T));
                    return true;
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException) { }
            }

            value = default;
            return false;
        }


        private void ReadFromFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            using (var reader = new StreamReader(fileName))
            {
                Section currentSection = null;

                string line, sectionname;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
                    {
                        sectionname = line.Substring(1, line.LastIndexOf(']') - 1);

                        currentSection = new Section(sectionname);
                        _sections.Add(sectionname, currentSection);
                    }

                    if (line.Contains("=") && currentSection != null)
                    {
                        string[] keyValuePair = line.Split('=', 2);
                        currentSection.Values.Add(keyValuePair[0].Trim(), keyValuePair[1].Trim());
                    }
                }
            }
        }

        private class Section
        {
            public readonly string Name;
            public readonly Dictionary<string, string> Values;

            public Section(string name)
            {
                Name = name;
                Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            public string this[string key]
            {
                get => Values.TryGetValue(key, out string value) ? value : "";
                set => Values[key] = value;
            }
        }
    }
}
