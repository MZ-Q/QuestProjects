using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StatsParser
{
    public class StatsParser
    {
        private readonly Dictionary<int, string> m_typesOfLvls;
        private readonly string m_gameURL;
        private DateTime m_startTime;
        private string m_gameType; // "В одиночку" || "Командами"

        public StatsParser(string gameURL, Dictionary<int, string> lvlsSpec)
        {
            m_gameURL = gameURL;
            m_typesOfLvls = lvlsSpec;

            // get game description from the main page:
            System.Net.WebClient client = new System.Net.WebClient();

            var mainPageData = client.DownloadData(m_gameURL);
            var mainPageHtml = Encoding.UTF8.GetString(mainPageData);
            HtmlAgilityPack.HtmlDocument mainPageDoc = new HtmlAgilityPack.HtmlDocument();
            mainPageDoc.LoadHtml(mainPageHtml);
            var spanNodes = mainPageDoc.DocumentNode.SelectNodes(xpath: "//span[@class='white']").ToList();

            string startTimeString = spanNodes.Find(x => x.InnerText.Contains("UTC")).InnerText;
            m_startTime = Convert.ToDateTime(startTimeString.Substring(0, startTimeString.IndexOf("(") - 1).Replace(".", "/"));

            string gameTypeString = spanNodes.Find(x => x.InnerText.Contains("Командами") || x.InnerText.Contains("В одиночку")).InnerText;
            m_gameType = new string(gameTypeString.Where(c => !char.IsControl(c)).ToArray());
        }

        // GetFinalTable in format: dict key: type of lvl -> value: list of tuples(team, team's result on this lvl)
        public Dictionary<string, List<(string TeamName, TimeSpan TimeResult)>> GetFinalTable()
        {
            // get table with timespans first
            List<List<(string TeamName, TimeSpan TimeResult)>> LvlsOfTeamsFinal = ParseStats();

            Dictionary<string, List<(string TeamName, TimeSpan TimeResult)>> finalTable = new Dictionary<string, List<(string, TimeSpan)>>();

            for (int i = 0; i < LvlsOfTeamsFinal.Count; i++)
            {
                // skip lvl if we have no spec for it
                if (!m_typesOfLvls.ContainsKey(i + 1))
                    continue;

                m_typesOfLvls.TryGetValue(i + 1, out string curType);

                if (!finalTable.ContainsKey(curType))
                {
                    finalTable.Add(curType, LvlsOfTeamsFinal[i]);
                }
                else
                {
                    var curReadyData = new List<(string TeamName, TimeSpan TimeResult)>();
                    finalTable.TryGetValue(curType, out curReadyData);

                    for (int j = 0; j < curReadyData.Count; j++)
                    {
                        string teamName = curReadyData[j].TeamName;
                        TimeSpan spanToAdd = LvlsOfTeamsFinal[i].Find(x => x.TeamName == teamName).TimeResult;
                        curReadyData[j] = (curReadyData[j].TeamName, curReadyData[j].TimeResult + spanToAdd);
                    }
                }
            }

            // get bounuses and penalties from the winners after-game table and combine them with existing stats
            var bonusesAndPenalties = GetBonusesAndPenalties();

            // we could guarantee that there is no duplicate key in bonuses and penalties table
            foreach (var bonusesPair in bonusesAndPenalties)
                finalTable.Add(bonusesPair.Key, bonusesPair.Value);

            return finalTable;
        }

        #region Private helpers
        private List<List<(string, TimeSpan)>> ParseStats()
        {
            System.Net.WebClient client = new System.Net.WebClient();

            // Get HTML table as it is
            var gameStatsData = client.DownloadData(m_gameURL.Replace("GameDetails", "GameStat"));
            var gameStatsHtml = Encoding.UTF8.GetString(gameStatsData);

            HtmlAgilityPack.HtmlDocument gameStatsDoc = new HtmlAgilityPack.HtmlDocument();
            gameStatsDoc.LoadHtml(gameStatsHtml);

            List<List<string>> table = gameStatsDoc.DocumentNode.SelectSingleNode(xpath: "//table[@id='GameStatObject_DataTable']") // "//table[@id='GameStatObject2_DataTable']"
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();

            // Transfer HTML table into List(lvls) of lists(teams)
            List<List<(string, DateTime)>> LvlsOfTeams = new List<List<(string, DateTime)>>();
            int numberOfCellsInRow = table[0].Count - 3; // 3 - number of non-functional collumns in the end of the table.

            for (int j = 1; j < numberOfCellsInRow; j++)
            {
                List<(string, DateTime)> oneLvl = new List<(string, DateTime)>();

                for (int i = 0; i < table.Count - 1; i++)
                {
                    string cell = table[i][j];

                    if (cell == string.Empty)
                        continue;

                    // if cell is not empty - we could be sure that it has proper format
                    string allButTeamName = (m_gameType == "В одиночку") ? Regex.Match(cell, @"\d{2}\.\d{2}\.\d{6}:\d{2}:\d{2}\.\d{1,3}.+", RegexOptions.Singleline).Value
                                                                         : Regex.Match(cell, @"\(.+\)\d{2}\.\d{2}\.\d{6}:\d{2}:\d{2}\.\d{1,3}.+", RegexOptions.Singleline).Value;

                    string TimeString = Regex.Match(cell, @"\d{2}\.\d{2}\.\d{6}:\d{2}:\d{2}\.\d{1,3}", RegexOptions.Singleline).Value;
                    string TeamName = cell.Replace(allButTeamName, string.Empty);
                    Regex regexForTimeString = new Regex(Regex.Escape("."));
                    string formatedTimeString = regexForTimeString.Replace(TimeString, "/", 2).Insert(10, " "); // 10 - index of hour
                    DateTime TimeOfLVLEnd = Convert.ToDateTime(formatedTimeString);

                    oneLvl.Add((TeamName, TimeOfLVLEnd));
                }

                LvlsOfTeams.Add(oneLvl);
            }

            // Transfer list of lists into list of TimeSpan instead of DateTime.
            List<List<(string, TimeSpan)>> LvlsOfTeamsFinal = new List<List<(string, TimeSpan)>>();

            for (int i = 0; i < LvlsOfTeams.Count; i++)
            {
                List<(string, TimeSpan)> oneLvlFinal = new List<(string, TimeSpan)>();

                for (int j = 0; j < LvlsOfTeams[i].Count; j++)
                {
                    string TeamName = LvlsOfTeams[i][j].Item1;

                    TimeSpan lvlTimeForCurTeam;

                    if (i == 0)
                    {
                        lvlTimeForCurTeam = LvlsOfTeams[i][j].Item2.Subtract(m_startTime);
                    }
                    else
                    {
                        DateTime timeOfFinishPrevLvlByCurTeam = LvlsOfTeams[i - 1].Find(x => x.Item1 == TeamName).Item2;
                        lvlTimeForCurTeam = LvlsOfTeams[i][j].Item2.Subtract(timeOfFinishPrevLvlByCurTeam);
                    }

                    oneLvlFinal.Add((TeamName, lvlTimeForCurTeam));
                }

                LvlsOfTeamsFinal.Add(oneLvlFinal);
            }

            return LvlsOfTeamsFinal;
        }

        private Dictionary<string, List<(string, TimeSpan)>> GetBonusesAndPenalties()
        {
            System.Net.WebClient client = new System.Net.WebClient();

            // get HTML table as it is
            var gameStatsData = client.DownloadData(m_gameURL.Replace("GameDetails", "GameWinners"));   
            var gameStatsHtml = Encoding.UTF8.GetString(gameStatsData);

            HtmlAgilityPack.HtmlDocument gameStatsDoc = new HtmlAgilityPack.HtmlDocument();
            gameStatsDoc.LoadHtml(gameStatsHtml);

            List<List<string>> table = gameStatsDoc.DocumentNode.SelectSingleNode(xpath: "//table[@class='table_light']")
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();

            List<(string, TimeSpan)> finalBonuses = new List<(string, TimeSpan)>();
            List<(string, TimeSpan)> finalPenalties = new List<(string, TimeSpan)>();

            for (int i = 0; i < table.Count; i++)
            {
                //1 - team name; 3 - bonus; 4 - penalty;
                TimeSpan curBonus = (table[i][3] == "0") ? new TimeSpan(0, 0, 0, 0)
                                                         : ParseBonusPenaltyTimeStr(table[i][3]);
                finalBonuses.Add((table[i][1], curBonus));

                TimeSpan curPenalty = (table[i][4] == "0") ? new TimeSpan(0, 0, 0, 0)
                                                           : ParseBonusPenaltyTimeStr(table[i][4]);
                finalPenalties.Add((table[i][1], curPenalty));
            }

            Dictionary<string, List<(string, TimeSpan)>> bonusesPenaltiesDict = new Dictionary<string, List<(string, TimeSpan)>>
            {
                { "бонусы", finalBonuses },
                { "штрафы", finalPenalties }
            };

            return bonusesPenaltiesDict;
        }

        private TimeSpan ParseBonusPenaltyTimeStr(string timeStr)
        {
            List<string> words = timeStr.Split(' ').ToList();

            int seconds = GetTimePartFromStrList(new List<string>(new string[] { "секунда", "секунд", "секунды" }));
            int minutes = GetTimePartFromStrList(new List<string>(new string[] { "мунута", "минут", "минуты" }));
            int hours   = GetTimePartFromStrList(new List<string>(new string[] { "час", "часов", "часа" }));
            int days    = GetTimePartFromStrList(new List<string>(new string[] { "день", "дней", "дня" }));

            // need to check manually because any of time part could be absent and also could contains days
            int GetTimePartFromStrList(List<string> timePartNames)
            {
                int timePart = 0;
                string timePartStr = words.FirstOrDefault(word => timePartNames.Contains(word));

                if (timePartStr != null)
                    timePart = int.Parse(words[words.IndexOf(timePartStr) - 1]);

                return timePart;
            }

            return new TimeSpan(days, hours, minutes, seconds);
        }
        #endregion
    }
}
