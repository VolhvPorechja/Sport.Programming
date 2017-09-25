using System;
using System.Collections.Generic;
using System.Linq;

namespace YContest
{
    struct MatchResult
    {
        public MatchResult(char symbol, int points)
        {
            Symbol = symbol;
            Points = points;
        }

        public char Symbol;
        public int Points;
    }

    class Row
    {
        public Row(string owner)
        {
            Results.Add(owner, new MatchResult('X', 0));
        }

        public Dictionary<string, MatchResult> Results { get; set; } = new Dictionary<string, MatchResult>();

        public PlayerResult Result { get; set; } = new PlayerResult();
    }

    class PlayerResult
    {
        public int Count { get; set; }
        public int WinsCount { get; set; }

        private sealed class CountWinsCountEqualityComparer : IEqualityComparer<PlayerResult>
        {
            public bool Equals(PlayerResult x, PlayerResult y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Count == y.Count && x.WinsCount == y.WinsCount;
            }

            public int GetHashCode(PlayerResult obj)
            {
                unchecked
                {
                    return (obj.Count*397) ^ obj.WinsCount;
                }
            }
        }

        public static IEqualityComparer<PlayerResult> CountWinsCountComparer { get; } =
            new CountWinsCountEqualityComparer();
    }


    class Program
    {
        private static Dictionary<string, Row> Results =
            new Dictionary<string, Row>();

        private static Dictionary<string, int> Places = new Dictionary<string, int>();

        private static int MaxWidth;

        private static void AddTeamResult(string resultLine)
        {
            var splitted = resultLine.Split('-').Select(el => el.Trim()).ToArray();

            if (!Results.ContainsKey(splitted[0]))
                Results.Add(splitted[0], new Row(splitted[0]));
            if (!Results.ContainsKey(splitted[1]))
                Results.Add(splitted[1], new Row(splitted[1]));

            if (splitted[0].Length > MaxWidth)
                MaxWidth = splitted[0].Length;

            if (splitted[1].Length > MaxWidth)
                MaxWidth = splitted[1].Length;

            var count = splitted[2].Split(':').Select(int.Parse).ToArray();

            var teamResult = CountOnSymbol(count[0], count[1]);

            Results[splitted[0]].Results.Add(splitted[1], teamResult);
            Results[splitted[0]].Result.Count += teamResult.Points;
            if (teamResult.Symbol == 'W')
                Results[splitted[0]].Result.WinsCount += 1;

            var otherTeamResult = CountOnSymbol(count[1], count[0]);
            Results[splitted[1]].Results.Add(splitted[0], otherTeamResult);
            Results[splitted[1]].Result.Count += otherTeamResult.Points;
            if (otherTeamResult.Symbol == 'W')
                Results[splitted[1]].Result.WinsCount += 1;
        }

        private static MatchResult CountOnSymbol(int teamA, int teamB)
        {
            return teamA == teamB
                ? new MatchResult('D', 1)
                : teamA > teamB ? new MatchResult('W', 3) : new MatchResult('L', 0);
        }

        static void Main(string[] args)
        {
            string line;
            while ((line = Console.ReadLine()) != "")
                AddTeamResult(line);

            CountPlaces();
            ShowTable();
        }

        private static void CountPlaces()
        {
            SortedDictionary<PlayerResult, List<string>> temp =
                new SortedDictionary<PlayerResult, List<string>>(
                    Comparer<PlayerResult>.Create(
                        (l, r) => (r.Count == l.Count) ? r.WinsCount.CompareTo(l.WinsCount) : r.Count.CompareTo(l.Count)));

            foreach (var result in Results)
            {
                foreach (var resultsKey in Results.Keys)
                    if (!result.Value.Results.ContainsKey(resultsKey))
                        result.Value.Results.Add(resultsKey, new MatchResult(' ', 0));

                if (!temp.ContainsKey(result.Value.Result))
                    temp.Add(result.Value.Result, new List<string> {result.Key});
                else
                    temp[result.Value.Result].Add(result.Key);
            }

            foreach (var winners in temp.Take(3).Select((el, num) => new {el, num}))
                foreach (var winner in winners.el.Value)
                    Places.Add(winner, winners.num + 1);
        }

        private static void ShowTable()
        {
            var separator = TableRowSep();

            Console.WriteLine(separator);

            var sortedKeys = Results.Keys.ToList();
            sortedKeys.Sort();

            foreach (var result in sortedKeys.Select((el, num) => new {el, num}))
            {
                var rowRes = Results[result.el];
                var sortedResultsKeys = rowRes.Results.Keys.ToList();
                sortedResultsKeys.Sort();

                Console.WriteLine(
                    "|" + result.num.ToString().PadLeft(Results.Keys.Count/10 + 1, ' ') + "|" +
                    result.el.PadRight(MaxWidth, ' ') + " |" +
                    string.Join("|", sortedResultsKeys.Select(k => $"{rowRes.Results[k].Symbol}")) + "|" + rowRes.Result.Count +
                    "|" + (Places.ContainsKey(result.el) ? Places[result.el].ToString() : " ") + "|");
                Console.WriteLine(separator);
            }
        }

        private static string TableRowSep()
        {
            return
                "+" + new string('-', Results.Keys.Count/10 + 1) + "+" + new string('-', MaxWidth + 1) + "+" +
                string.Concat(Enumerable.Repeat("-+", Results.Keys.Count + 2));
        }
    }
}