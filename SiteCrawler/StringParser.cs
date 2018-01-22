using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteCrawler
{
    public class StringParser
    {
        private readonly string _stringToParse;
        private readonly string _parseAnchor;
        public StringParser(string stringToParse, string parseAnchor = "a href")
        {
            _stringToParse = stringToParse;
            _parseAnchor = parseAnchor;
        }

        public IEnumerable<string> ParseHrefs()
        {
            var hrefIndexes = _stringToParse.AllIndexesOf("a href");
            for (var i = 0; i < hrefIndexes.Count; i++)
            {
                var index = hrefIndexes[i] + _parseAnchor.Length;
                var tagClosePosition = _stringToParse.IndexOf(">", index);
                var lastIndex = tagClosePosition;
                var substr = _stringToParse.Substring(index, lastIndex-index);
                var strips = substr.Split(' ');
                var result = strips.First(t => t != "=" && !String.IsNullOrWhiteSpace(t));
                result = result.Trim(' ', '=', '"', Char.Parse("'"));
                yield return result;
            }
        }
    }
}