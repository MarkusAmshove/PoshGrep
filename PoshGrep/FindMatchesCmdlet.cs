using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.PowerShell.Commands;

namespace PoshGrep
{
    [Cmdlet(VerbsCommon.Find, "Matches")]
    public class FindMatchesCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromRemainingArguments = true, Position = 3)]
        public List<string> Pattern { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Recursive { get; set; } = false;

        [Parameter(Mandatory = false)]
        public SwitchParameter IgnoreCase { get; set; } = false;

        [Parameter(Mandatory = false)]
        public string Filter { get; set; } = "*.*";

        private Regex _pattern;

        private string GetPattern() => IgnoreCase ? "(?i)" + Pattern.First() : Pattern.First();

        private SearchOption GetFileSearchOption() => Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        protected override void BeginProcessing()
        {
            _pattern = new Regex(GetPattern(), RegexOptions.Compiled);
            WriteDebug($"Pattern: {Pattern}");
            WriteDebug($"Recursive: {Recursive}");
            WriteDebug($"IgnoreCase: {IgnoreCase}");
            WriteDebug($"Filter: {Filter}");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var files = Directory.GetFiles(SessionState.Path.CurrentLocation.Path, Filter, GetFileSearchOption());
            foreach (var file in files)
            {
                FindMatchesInFile(file);
            }
        }

        private void FindMatchesInFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                var lineNumber = 1;
                foreach (var line in lines)
                {
                    if (_pattern.IsMatch(line))
                    {
                        Write($"{ConvertToRelative(Path.GetFullPath(filePath))}:{lineNumber}\t");
                        FindMatchInLine(line);
                        WriteLine();
                    }
                    lineNumber++;
                }
            }
            catch (IOException e)
            {
                WriteLine(e.Message, ConsoleColor.Red);
            }
        }

        private void FindMatchInLine(string lineContent)
        {
            var index = 0;
            while (index < lineContent.Length)
            {
                var match = _pattern.Match(lineContent, index);
                if (match.Success && match.Length > 0)
                {
                    Write(lineContent.Substring(index, match.Index - index));
                    Write(match.Value, ConsoleColor.Yellow);
                    index = match.Index + match.Length;
                }
                else
                {
                    Write(lineContent.Substring(index));
                    index = lineContent.Length;
                }
            }
        }

        private string ConvertToRelative(string path) => path.Replace(SessionState.Path.CurrentFileSystemLocation.Path, ".");

        private static void Write(string message = "", ConsoleColor? color = null)
        {
            var newColor = color ?? Console.ForegroundColor;
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = newColor;
            Console.Write(message);
            Console.ForegroundColor = currentColor;
        }

        private static void WriteLine(string message = "", ConsoleColor? color = null) => Write(message + Environment.NewLine, color);
    }
}
