using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace StatsParser
{
    class MonitorParser
    {
        private string m_gameURL;

        public MonitorParser(string gameURL) => m_gameURL = gameURL;

        public struct TeamResult
        {
            private string teamName;
            private TimeSpan timeResult;

            public TeamResult(string tn, TimeSpan tr)
            {
                teamName = tn;
                timeResult = tr;
            }

            public TimeSpan TimeResult { get => timeResult; set => timeResult = value; }
            public string TeamName { get => teamName; set => teamName = value; }
        };

        private DateTime getStartTime()
        {
            System.Net.WebClient client = new System.Net.WebClient();

            var mainPageData = client.DownloadData(m_gameURL);
            var mainPageHtml = Encoding.UTF8.GetString(mainPageData);
            HtmlAgilityPack.HtmlDocument mainPageDoc = new HtmlAgilityPack.HtmlDocument();
            mainPageDoc.LoadHtml(mainPageHtml);
            var spanNodes = mainPageDoc.DocumentNode.SelectNodes("//span[@class='white']");

            List<string> strNodes = new List<string>();
                
            foreach (var node in spanNodes)
                strNodes.Add(node.InnerText);

            string startTimeString = strNodes.Find(x => x.Contains("UTC"));
            startTimeString = startTimeString.Substring(0, startTimeString.IndexOf("(") - 1).Replace(".", "/");

            return Convert.ToDateTime(startTimeString);
        }

        private Dictionary<int, string> getLvlsSpec(string fileName)
        {
            Dictionary<int, string> typesOfLvls = new Dictionary<int, string>();

            //create the Application object we can use in the member functions.
            Microsoft.Office.Interop.Excel.Application _excelApp = new Microsoft.Office.Interop.Excel.Application();
            _excelApp.Visible = true;

            var xlWorkBooks = _excelApp.Workbooks;

            //open the workbook
            Workbook xlWorkBook = xlWorkBooks.Open(fileName,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing);

            //select the first sheet        
            Worksheet xlWorkSheet = xlWorkBook.Worksheets[1];

            //find the used range in worksheet
            Range excelRange = xlWorkSheet.UsedRange;

            //get an object array of all of the cells in the worksheet (their values)
            object[,] valueArray = (object[,])excelRange.get_Value(
                        XlRangeValueDataType.xlRangeValueDefault);

            //access the cells
            for (int row = 1; row <= xlWorkSheet.UsedRange.Rows.Count; ++row)
                typesOfLvls.Add(Convert.ToInt32(valueArray[row, 1]), valueArray[row, 2].ToString());

            //clean up stuffs
            Marshal.ReleaseComObject(xlWorkSheet);
            xlWorkBook.Close(false, Type.Missing, Type.Missing);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlWorkBooks);

            _excelApp.Quit();
            Marshal.FinalReleaseComObject(_excelApp);

            return typesOfLvls;
        }

        public List<List<TeamResult>> ParseStats()
        {
            System.Net.WebClient client = new System.Net.WebClient();

            // Get HTML table as it is
            var gameStatsData = client.DownloadData(m_gameURL.Replace("GameDetails", "GameStat")); //GameWinners   
            var gameStatsHtml = Encoding.UTF8.GetString(gameStatsData);

            HtmlAgilityPack.HtmlDocument gameStatsDoc = new HtmlAgilityPack.HtmlDocument();
            gameStatsDoc.LoadHtml(gameStatsHtml);

            List<List<string>> table = gameStatsDoc.DocumentNode.SelectSingleNode("//table[@id='GameStatObject_DataTable']") //'GameStatObject2_DataTable'
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();
            //List<List<string>> table = gameStatsDoc.DocumentNode.SelectSingleNode("//table[@class='table_light']") //'GameStatObject2_DataTable'
            //            .Descendants("tr")
            //            .Skip(1)
            //            .Where(tr => tr.Elements("td").Count() > 1)
            //            .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
            //            .ToList();

            // Transfer HTML table into List(lvls) of lists(teams)

            List<List<Tuple<string, DateTime>>> LvlsOfTeams = new List<List<Tuple<string,DateTime>>>();
            int numberOfCellsInRow = table[0].Count - 3; // 3 - number of non-functional collumns.

            for (int j = 1; j < numberOfCellsInRow; j++)
            {
                List<Tuple<string, DateTime>> oneLvl = new List<Tuple<string, DateTime>>();

                for (int i = 0; i < table.Count - 1; i++)
                {
                    string cell = table[i][j];

                    if (cell == string.Empty)
                        continue;
                    //................. for personal quest.....................
                    //int indexOfFirstNumber = cell.IndexOfAny("0123456789".ToCharArray());
                    //int timeStartIndex = indexOfFirstNumber;
                    //string cutFromFirstNumber = cell;

                    //while (cutFromFirstNumber[indexOfFirstNumber + 2] != '.')
                    //{
                    //    cutFromFirstNumber = cutFromFirstNumber.Substring(indexOfFirstNumber + 1);
                    //    indexOfFirstNumber = cutFromFirstNumber.IndexOfAny("0123456789".ToCharArray());
                    //    timeStartIndex += indexOfFirstNumber;
                    //    timeStartIndex += 1;
                    //}

                    //string TeamName = cell.Substring(0, timeStartIndex);
                    //string[] separators = new string[] { ")", "(", "))", "((" };
                    //string checkForStupidNames = cell.Split(separators, StringSplitOptions.RemoveEmptyEntries)[0];
                    //string TimeString = checkForStupidNames.Substring(timeStartIndex);

                    //.............................. for team quest...........................................
                    string TeamName = cell.Substring(0, cell.IndexOf("("));
                    
                    string[] separators = new string[] { ")", "(", "))", "((" };
                    string checkForStupidNames = cell.Split(separators, StringSplitOptions.RemoveEmptyEntries)[2];
                    string TimeString = int.TryParse(checkForStupidNames[0].ToString(), out int n) ? cell.Split(separators, StringSplitOptions.RemoveEmptyEntries)[2]
                                                                                                   : cell.Split(separators, StringSplitOptions.RemoveEmptyEntries)[3];
                    ///.......................................................................................
                    var regex = new Regex(Regex.Escape("."));
                    string formatedTimeString = regex.Replace(TimeString, "/", 2).Insert(10, " "); // 10 - index of hour
                    DateTime TimeOfLVLEnd = Convert.ToDateTime(formatedTimeString);
                    
                    oneLvl.Add(new Tuple<string,DateTime>(TeamName, TimeOfLVLEnd));
                }

                LvlsOfTeams.Add(oneLvl);
            }
            
            // Transfer list of lists into list of TimeSpan instead of DateTime.
            List<List<TeamResult>> LvlsOfTeamsFinal = new List<List<TeamResult>>();
            DateTime startTime = getStartTime();

            for (int i = 0; i < LvlsOfTeams.Count; i++)
            {
                List<TeamResult> oneLvlFinal = new List<TeamResult>();

                for (int j = 0; j < LvlsOfTeams[i].Count; j++)
                {
                    string TeamName = LvlsOfTeams[i][j].Item1;

                    TimeSpan lvlTimeForCurTeam;

                    if (i == 0)
                    {
                        lvlTimeForCurTeam = LvlsOfTeams[i][j].Item2.Subtract(startTime);
                    }
                    else
                    {
                        DateTime timeOfFinishPrevLvlByCurTeam = LvlsOfTeams[i - 1].Find(x => x.Item1 == TeamName).Item2;
                        lvlTimeForCurTeam = LvlsOfTeams[i][j].Item2.Subtract(timeOfFinishPrevLvlByCurTeam);
                    }

                    oneLvlFinal.Add(new TeamResult(TeamName, lvlTimeForCurTeam));
                }

                LvlsOfTeamsFinal.Add(oneLvlFinal);
            }

            var finalTable = GetFinalTable(LvlsOfTeamsFinal);

            WriteToExcel(finalTable);
            return LvlsOfTeamsFinal;
        }

        // GetFinalTable in format: dict key: type of lvl -> value: list of tuples(team, team's result on this lvl)
        public Dictionary<string, List<TeamResult>> GetFinalTable(List<List<TeamResult>> LvlsOfTeamsFinal)
        {
            Dictionary<string, List<TeamResult>> finalTable = new Dictionary<string, List<TeamResult>>();

            // Get types of lvls
            string loadDirectory = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\";
            var typesOfLvls = getLvlsSpec(loadDirectory + "TypesOfLvls.xlsx");

            for (int i = 0; i < LvlsOfTeamsFinal.Count; i++)
            {
                if (!typesOfLvls.ContainsKey(i + 1))
                    continue;

                typesOfLvls.TryGetValue(i + 1, out string curType);

                if (!finalTable.ContainsKey(curType))
                {
                    finalTable.Add(curType, LvlsOfTeamsFinal[i]);
                }
                else
                {
                    var curReadyData = new List<TeamResult>();
                    finalTable.TryGetValue(curType, out curReadyData);

                    for (int j = 0; j < curReadyData.Count; j++)
                    {
                        string teamName = curReadyData[j].TeamName;
                        TimeSpan spanToAdd = LvlsOfTeamsFinal[i].Find(x => x.TeamName == teamName).TimeResult;
                        TeamResult TrToAdd = new TeamResult(curReadyData[j].TeamName, curReadyData[j].TimeResult + spanToAdd);
                        curReadyData[j] = TrToAdd;
                    }
                }
            }

            return finalTable;
        }

        public void WriteToExcel(Dictionary<string, List<TeamResult>> finalTable)
        {
            Application _excelApp = new Application();
            _excelApp.Visible = true;

            var xlWorkBooks = _excelApp.Workbooks;
            var xlWorkBook = xlWorkBooks.Add(Type.Missing);
            var xlWorkSheets = xlWorkBook.Worksheets;
            Worksheet xlWorkSheet = xlWorkSheets.get_Item(1);

            for (int i = 0; i < finalTable.Count; i++)
            {
                xlWorkSheet.Cells[1, i + 2] = finalTable.ElementAt(i).Key.ToString();

                for (int j = 0; j < finalTable.ElementAt(i).Value.Count; j++)
                {
                    if (i == 0)
                    {
                        xlWorkSheet.Cells[j + 2, 1] = finalTable.ElementAt(i).Value[j].TeamName.ToString();
                        xlWorkSheet.Cells[j + 2, 2] = finalTable.ElementAt(i).Value[j].TimeResult.ToString();
                    }
                    else
                    {
                        int columnIndex = finalTable.ElementAt(0).Value.FindIndex(x => x.TeamName == finalTable.ElementAt(i).Value[j].TeamName);
                        xlWorkSheet.Cells[columnIndex + 2, i + 2] = finalTable.ElementAt(i).Value[j].TimeResult.ToString();
                    }
                }

            }


            xlWorkSheet.get_Range("A1", "Z1").Font.Bold = true;
            xlWorkSheet.get_Range("A1", "Z1").VerticalAlignment = XlVAlign.xlVAlignCenter;
            xlWorkSheet.get_Range("A1", "A105").Font.Bold = true;
            xlWorkSheet.get_Range("A1", "A105").VerticalAlignment = XlVAlign.xlVAlignCenter;
            xlWorkSheet.get_Range("A2", "Z100").NumberFormat = "h:mm:ss";

            // Mark best and worst time with green or red brush
            for (char c = 'B'; c <= 'Z'; c++)
            {
                if (c - 'A' > finalTable.Count)
                    break;

                // Hope all of the teams passed 0-index type of lvls
                int numberOfTeams = finalTable.ElementAt(0).Value.Count;
                string bestTimeFormula = string.Format("=${0}2=МИН(${0}$2:${0}${1})", c, numberOfTeams + 1);
                string worstTimeFormula = bestTimeFormula.Replace("МИН", "МАКС");

                Range range = xlWorkSheet.get_Range(c + "2", c + (numberOfTeams + 1).ToString());
                FormatConditions fcs = range.FormatConditions;
                FormatCondition fcW = (FormatCondition)fcs.Add
                    (XlFormatConditionType.xlExpression, Type.Missing, worstTimeFormula);
                Interior interiorW = fcW.Interior;
                interiorW.Color = ColorTranslator.ToOle(Color.Red);

                FormatCondition fcB = (FormatCondition)fcs.Add
                    (XlFormatConditionType.xlExpression, Type.Missing, bestTimeFormula);
                Interior interiorB = fcB.Interior;
                interiorB.Color = ColorTranslator.ToOle(Color.Green);
            }

            // Save xls
            string safeDirectory = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\Statistics\\";
            xlWorkBook.SaveAs(string.Format(safeDirectory + "Game_ID{0}_stars.xls", m_gameURL.Split('=')[1]));

            // Clean up stuffs
            Marshal.ReleaseComObject(xlWorkSheet);
            xlWorkBook.Close(false, Type.Missing, Type.Missing);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlWorkBooks);

            _excelApp.Quit();
            Marshal.FinalReleaseComObject(_excelApp);
        }
    }
}
