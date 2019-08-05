﻿/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.EH;
using Koromo_Copy.Interface;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Component.Hitomi
{
    [MessagePackObject]
    public class HitomiTitleModel
    {
        [Key(0)]
        public int[] id;
        [Key(1)]
        public string[] origin_title;
    }

    public class HitomiTitle : ILazy<HitomiTitle>
    {
        HitomiTitleModel model;

        public int Count { get { return model.id.Length; } }

        public static void MakeTitle()
        {
            var xxx = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("ex-hentai-archive3.json"));

            var result = new HitomiTitleModel();

            var id = new List<int>();
            var origin_title = new List<string>();

            foreach (var xx in xxx)
            {
                id.Add(xx.URL.Split('/')[4].ToInt32());
                origin_title.Add(xx.Title);
            }

            result.id = id.ToArray();
            result.origin_title = origin_title.ToArray();

            Array.Sort(result.id, result.origin_title);

            var bbb = MessagePackSerializer.Serialize(result);
            using (FileStream fsStream = new FileStream(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "origin-title.json"), FileMode.Create))
            using (BinaryWriter sw = new BinaryWriter(fsStream))
            {
                sw.Write(bbb);
            }
        }

        public void Load()
        {
            if (CheckExist())
            {
                model = MessagePackSerializer.Deserialize<HitomiTitleModel>(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "origin-title.json")));
            }
        }

        public async Task DownloadTitle()
        {
            Monitor.Instance.Push("Download Titles...");
            ServicePointManager.DefaultConnectionLimit = 999999999;

            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);
            var data = await client.GetByteArrayAsync("https://raw.githubusercontent.com/dc-koromo/e-archive/master/origin-title.json");
            var tm = MessagePackSerializer.Deserialize<HitomiTitleModel>(data);
            Monitor.Instance.Push($"Download complete: [1/1] 1");
            if (!Settings.Instance.Hitomi.AutoSync)
            {
                var bbb = MessagePackSerializer.Serialize(tm);
                using (FileStream fsStream = new FileStream(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "origin-title.json"), FileMode.Create))
                using (BinaryWriter sw = new BinaryWriter(fsStream))
                {
                    sw.Write(bbb);
                }
            }
        }

        public bool CheckExist()
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "origin-title.json"));
        }

        public bool Exists(int id)
        {
            return Array.BinarySearch(model.id, id) >= 0;
        }

        public string GetOriginalTitle(int id)
        {
            return model.origin_title[Array.BinarySearch(model.id, id)];
        }

        public void ReplaceToOriginTitle()
        {
            for (int i = 0; i < HitomiIndex.Instance.metadata_collection.Count; i++)
            {
                if (Exists(HitomiIndex.Instance.metadata_collection[i].ID))
                {
                    var item = HitomiIndex.Instance.metadata_collection[i];
                    item.Name = GetOriginalTitle(HitomiIndex.Instance.metadata_collection[i].ID);
                    HitomiIndex.Instance.metadata_collection[i] = item;
                }
            }
        }
    }
}