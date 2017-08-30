# QuestProjects
All personal projects related to the "quest" and "encounter" engines.
## StatsParser

Below is short brief info:
Based on game URL, *StatsParser* provides next main opportunities:
- parse after-game statistic
- group result time of the similar levels for each team
- mark best and worst time for each group of game levels
- support bonuses and penalties
- support both team and personal quest-games
- deserialization of the results into the .xls in comfortable view

All known game domains are supported. The most intensive usage and testing was made on:
- Quest.ua
- Kharkov.en.cx

File `TypesOfLvls.xlsx` right now formated for the next game [Игра - Вода](http://kharkov.en.cx/GameDetails.aspx?gid=59218).
For other games - you should edit it according to the game-scenario, which could be found on the main page of the game after it's finish.
All parsed stats will be saved into `Statistic` folder of the repo.

Tech requirements for proper usage:
- OS Windows
- .NET Framework 4.7
- Russian MS Excel


Note: right now commited stable dev version of the project, which has been implemented for personal use.
Exception handling is absent (might be added further).
