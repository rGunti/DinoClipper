using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DinoClipper.Ffmpeg
{
    public static class FilterScriptGenerator
    {
        public static string RenderFilterToText(FilterDefinition filterDefinition)
        {
            var sb = new StringBuilder();

            if (filterDefinition.Inputs != null && filterDefinition.Inputs.Any())
            {
                sb.Append(filterDefinition.Inputs.Select(i => $"[{i}]").JoinToString());
            }

            sb.Append(filterDefinition.Filter);

            if (filterDefinition.Parameters.Any())
            {
                sb.AppendLine(" =");
                string paramText = filterDefinition.Parameters
                    .Select(p => $"\t{p.Key} = {p.Value}")
                    .JoinToString(" :\n")
                    .Trim();
                sb.AppendLine($"\t{paramText}");
            }
            else
            {
                sb.AppendLine();
            }

            if (filterDefinition.Outputs != null && filterDefinition.Outputs.Any())
            {
                sb.Append("\t" + filterDefinition.Outputs.Select(i => $"[{i}]").JoinToString());
            }

            return sb.ToString();
        }

        public static string RenderFilterScriptToText(IEnumerable<FilterDefinition> filterDefinitions)
        {
            return filterDefinitions
                .Select(RenderFilterToText)
                .JoinToString(";\n");
        }
    }

    public class FilterDefinition
    {
        public FilterDefinition()
        {
        }

        public FilterDefinition(string filter)
        {
            Filter = filter;
        }

        public FilterDefinition(string filter, string input, string output = null)
            : this(filter)
        {
            if (!string.IsNullOrWhiteSpace(input))
                Inputs = new List<string> { input };
            if (!string.IsNullOrWhiteSpace(output))
                Outputs = new List<string> { output };
        }

        public FilterDefinition(string filter, IEnumerable<string> inputs, IEnumerable<string> outputs)
            : this(filter)
        {
            Inputs = inputs.ToList();
            Outputs = outputs.ToList();
        }

        public FilterDefinition(string filter, IEnumerable<string> inputs, string outputs = null)
            : this(filter, inputs, new []{ outputs }.Where(i => !string.IsNullOrWhiteSpace(i)))
        {
        }

        public FilterDefinition(string filter, string input, string output, Dictionary<string, string> filterParams)
            : this(filter, input, output)
        {
            Parameters = filterParams;
        }

        public string Filter { get; set; }
        public List<string> Inputs { get; set; }
        public List<string> Outputs { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();

        public FilterDefinition AddParam(string key, string value)
        {
            Parameters.Add(key, value);
            return this;
        }

        public FilterDefinition AddParam(string key, int value) => AddParam(key, $"{value}");

        public FilterDefinition AddFileParam(string key, string value, string basePath = ".")
        {
            string path = Path.Combine(basePath, $"{key}_{Guid.NewGuid().ToString().Replace("-", "")}.txt");
            File.WriteAllText(path, value);
            return AddParam(key, path);
        }
    }
}