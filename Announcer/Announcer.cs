using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Announcer
{
    public class AnnSys : ModSystem
    {
        ICoreServerAPI api;
        bool announcedToday = false;
        DateTime LstAnn;

        List<Announcement> announcements = new();

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;

            api.Event.RegisterGameTickListener(GmTck, 1000);

            LstAnn = DateTime.Now;

            LdCon();
        }

        private static string GetConfigPath()
        {
            string AppDat = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string ConDir = Path.Combine(AppDat, "vintagestorydata", "ModConfig");
            Directory.CreateDirectory(ConDir);
            return Path.Combine(ConDir, "AnnouncerConfig.json");
        }

        private void LdCon()
        {
            string ConPth = GetConfigPath();

            if (File.Exists(ConPth))
            {
                try
                {
                    string json = File.ReadAllText(ConPth);
                    announcements = System.Text.Json.JsonSerializer.Deserialize<List<Announcement>>(json);

                    if (announcements == null || announcements.Count == 0)
                    {
                        announcements = new List<Announcement>
                        {
                            new Announcement { Hour = 6, Minute = 0, Message = "placeholder anouncement(u should totally set this up it'd be v cool of you to use this)" }
                        };
                        SvCon();
                    }
                }
                catch (Exception e)
                {
                    api.Logger.Error("Failed to load Announcer config: " + e.Message);
                }
            }
            else
            {
                announcements = new List<Announcement>
                {
                    new Announcement { Hour = 6, Minute = 0, Message = "placeholder anouncement(u should totally set this up it'd be v cool of you to use this)" }
                };
                SvCon();
            }
        }

        private void SvCon()
        {
            string ConPth = GetConfigPath();

            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(announcements, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConPth, json);
            }
            catch (Exception e)
            {
                api.Logger.Error("Failed to save Announcer config: " + e.Message);
            }
        }

        private void GmTck(float deltaTime)
        {
            DateTime now = DateTime.Now;

            if (now.Date > LstAnn)
            {
                announcedToday = false;
                LstAnn = now.Date;
            }

            foreach (var announcement in announcements)
            {
                DateTime NxtAnn = new DateTime(now.Year, now.Month, now.Day, announcement.Hour, announcement.Minute, 0);

                if (NxtAnn < now)
                {
                    NxtAnn = NxtAnn.AddDays(1);
                }

                TimeSpan TmAnn = NxtAnn - now;

                if (TmAnn.TotalMilliseconds <= deltaTime * 1000)
                {
                    api.InjectConsole(announcement.Message);

                    announcedToday = true;
                }
            }
        }

        private class Announcement
        {
            public int Hour { get; set; }
            public int Minute { get; set; }
            public string Message { get; set; }
        }
    }
}
