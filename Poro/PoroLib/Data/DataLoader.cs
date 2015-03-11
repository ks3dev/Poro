﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoroLib.Data.JSON;
using PoroLib.Data.SQLite;
using PoroLib.Structures;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PoroLib.Data
{
    public class DataLoader
    {
        public bool _hasLoaded;
        public List<Champions> Champions;
        public List<ChampionSkins> ChampionSkins;
        public List<TalentGroup> TalentTree;

        private Dictionary<string, int> _masterySort = new Dictionary<string, int> { { "Offense", 1 }, { "Defense", 2 }, { "Utility", 3 } };

        public void LoadData()
        {
            if (_hasLoaded)
                return;

            _hasLoaded = true;

            /*SQLiteConnection conn = new SQLiteConnection(Path.Combine(PoroServer.ClientLocation, "assets", "data", "gameStats", "gameStats_en_US.sqlite"));

            Champions = (from s in conn.Table<Champions>() orderby s.name select s).ToList();

            ChampionSkins = (from s in conn.Table<ChampionSkins>() select s).ToList();

            conn.Close();*/

            using (WebClient client = new WebClient())
            {
                string Versions = client.DownloadString("http://ddragon.leagueoflegends.com/realms/euw.js"); //Download latest ddragoon
                Versions = Versions.Replace(";", "").Replace("Riot.DDragon.m=", "");
                JObject DDragonObject = JsonConvert.DeserializeObject<JObject>(Versions);
                JObject DDragonVersions = DDragonObject["n"] as JObject;

                string RuneVersion = DDragonVersions["rune"].ToString();
                string MasteryVersion = DDragonVersions["mastery"].ToString();

                string RuneData = client.DownloadString(string.Format("http://ddragon.leagueoflegends.com/cdn/{0}/data/en_US/rune.json", RuneVersion));
                string MasteryData = client.DownloadString(string.Format("http://ddragon.leagueoflegends.com/cdn/{0}/data/en_US/mastery.json", MasteryVersion));

                Masteries mData = JsonConvert.DeserializeObject<Masteries>(MasteryData);
                TalentTree = new List<TalentGroup>();
                foreach (KeyValuePair<string, List<List<MasteryLite>>> mastery in mData.tree)
                {
                    TalentGroup group = new TalentGroup
                    {
                        Name = mastery.Key,
                        TalentRows = new List<TalentRow>(),
                        TltGroupId = _masterySort[mastery.Key],
                        Index = _masterySort[mastery.Key] - 1
                    };

                    for (int i = 0; i < mastery.Value.Count; i++ )
                    {
                        TalentRow row = new TalentRow
                        {
                            Index = i
                        };
                    }

                    TalentTree.Add(group);
                }
            }

            Console.WriteLine("[LOG] Loaded League of Legends Data");
        }
    }
}
