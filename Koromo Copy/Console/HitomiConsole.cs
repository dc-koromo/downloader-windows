﻿/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Analysis;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 히토미 콘솔 옵션입니다.
    /// </summary>
    public class HitomiConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-article", CommandType.ARGUMENTS, Help = "use -article <Hitomi Number>", 
            Info = "Download article page and parse html.")]
        public string[] Article;
        [CommandLine("-image", CommandType.ARGUMENTS, Help = "use -image <Hitomi Number> [-type=small | big]", 
            Info = "Download article images link list.")]
        public string[] ImageLink;
        [CommandLine("-type", CommandType.EQUAL, Help = "use [-type=small | big], must be used with -image", 
            Info = "Set download image size type.")]
        public string Type;

        [CommandLine("-downloadmetadata", CommandType.OPTION, Info = "Download metadata and save with json file.")]
        public bool DownloadMetadata;
        [CommandLine("-loadmetadata", CommandType.OPTION, Info = "Load metadata.json file for hitomi metadata.")]
        public bool LoadMetadata;

        [CommandLine("-downloadhidden", CommandType.OPTION, 
            Info = "Download hidden metadata and save with json file. After processing, rebuild tag datas (used for auto complete).")]
        public bool DownloadHidden;
        [CommandLine("-loadhidden", CommandType.OPTION, Info = "Load hiddendata.json file for hitomi metadata.")]
        public bool LoadHidden;

        [CommandLine("-sync", CommandType.OPTION, 
            Info = "Syncronize hitomi metadatas. This command is equivalent to -metadata + -hiddendata.")]
        public bool Sync;
        [CommandLine("-load", CommandType.OPTION, Info = "Load metadata by metadata.json, hiddendata.json.")]
        public bool Load;

        [CommandLine("-all", CommandType.OPTION, 
            Info = "Show search list with detail informations. This commend must be used with -search.")]
        public bool ShowAllSearchList;
        [CommandLine("-search", CommandType.ARGUMENTS, Help = "use -search <Search Content>",
            Info = "Search on hitomi articles. The search method depends on settings algorithm.")]
        public string[] Search;
        [CommandLine("-setsearch", CommandType.ARGUMENTS, Info = "Search token include automatically in searching.")]
        public string[] SetSearchToken;

        [CommandLine("-syncdate", CommandType.OPTION, Info = "Synchronize hitomi article upload dates based on magic number.")]
        public bool SyncDate;
        [CommandLine("-latest", CommandType.OPTION, Info = "Get latest article uploaded dates.")]
        public bool Latest;

        [CommandLine("-rank", CommandType.OPTION, Info = "Show artists recommendation artist ranking list.")]
        public bool Rank;
        [CommandLine("-taglist", CommandType.OPTION, Info = "Show downloaded articles tags list.")]
        public bool TagList;

        [CommandLine("-forbidden", CommandType.ARGUMENTS, Info = "use -forbidden <Hitomi Number>.")]
        public string[] Forbidden;

        [CommandLine("-test", CommandType.ARGUMENTS, Info = "Tester for hitomi.", Help = "Check developer manual.")]
        public string[] Test;
    }

    /// <summary>
    /// 코로모 카피에 구현된 모든 히토미 도구를 사용할 수 있는 콘솔 명령 집합입니다.
    /// </summary>
    public class HitomiConsole : ILazy<HitomiConsole>, IConsole
    {
        public string setter = "";

        /// <summary>
        /// 히토미 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            HitomiConsoleOption option = CommandLineParser<HitomiConsoleOption>.Parse(arguments);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.Instance.WriteLine(option.HelpMessage);
                return false;
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Article != null)
            {
                ProcessArticle(option.Article);
            }
            else if (option.ImageLink != null)
            {
                ProcessImage(option.ImageLink, option.Type);
            }
            //
            //  다운로드 관련
            //
            else if (option.DownloadMetadata)
            {
                ProcessDownloadMetadata();
            }
            else if (option.LoadMetadata)
            {
                ProcessLoadMetadata();
            }
            else if (option.DownloadHidden)
            {
                ProcessDownloadHidden();
            }
            else if (option.LoadHidden)
            {
                ProcessLoadHidden();
            }
            //
            //  로드 및 동기화
            //
            else if (option.Sync)
            {
                ProcessSync();
            }
            else if (option.Load)
            {
                ProcessLoad();
            }
            //
            //  검색
            //
            else if (option.Search != null)
            {
                ProcessSearch(option.Search, option.ShowAllSearchList);
            }
            else if (option.SetSearchToken != null)
            {
                Instance.setter = option.SetSearchToken[0];
            }
            //
            //  Date 동기화
            //
            else if (option.SyncDate)
            {
                HitomiDate.print_datetime_data();
            }
            else if (option.Latest)
            {
                ProcessLatest();
            }
            //
            //  작가 추천
            //
            else if (option.Rank)
            {
                ProcessRank();
            }
            else if (option.TagList)
            {
                ProcessTagList();
            }
            //
            //  Forbidden 데이터
            //
            else if (option.Forbidden != null)
            {
                ProcessForbidden(option.Forbidden);
            }
            //
            //  테스트
            //
            else if (option.Test != null)
            {
                ProcessTest(option.Test);
            }

            return true;
        }

        bool IConsole.Redirect(string[] arguments, string contents)
        {
            return Redirect(arguments, contents);
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "Hitomi Console Core\r\n" + 
                "\r\n"
                //" -article <Hitomi Number> : Show article info.\r\n" +
                //" -image <Hitomi Number> [-type=small | big]: Get Image Link.\r\n" +
                //" -downloadmetadata, -loadmetadata, -downloadhidden, -loadhidden, -sync, -load : Manage Metadata.\r\n" +
                //" -search <Search What> [-all] : Language Dependent metadata seraching.\r\n" +
                //" -setsearch <Place What> : Fix specific search token.\r\n" +
                //" -syncdate : Synchronize HitomiDate data.\r\n" +
                //" -rank : Show artists recommendation artist list\r\n" +
                //" -taglist : Show downloaded article's tag list"
                );

            var builder = new StringBuilder();
            CommandLineParser<HitomiConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} ({x.Value.Item2.Help}) : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                    else
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                });
            Console.Instance.WriteLine(builder.ToString());
        }

        /// <summary>
        /// 아티클 정보를 다운로드하고 정보를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessArticle(string[] args)
        {
            Console.Instance.WriteLine(HitomiDispatcher.Collect(args[0]));
        }

        /// <summary>
        /// 이미지 링크를 다운로드하고 정보를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="dl">다운로드 가능한 이미지 링크를 출력할지의 여부를 설정합니다.</param>
        static void ProcessImage(string[] args, string type)
        {
            string html_source = NetCommon.DownloadString($"{HitomiCommon.HitomiGalleryAddress}{args[0]}.js");
            var image_link = HitomiParser.GetImageLink(html_source);

            if (type == null)
            {
                Console.Instance.WriteLine(image_link.Select(x => HitomiCommon.GetDownloadImageAddress(args[0], x)));
            }
            else if (type == "small")
            {
                Console.Instance.WriteLine(image_link.Select(x => $"{HitomiCommon.HitomiThumbnailSmall}{args[0]}/{x}.jpg"));
            }
            else if (type == "big")
            {
                Console.Instance.WriteLine(image_link.Select(x => $"{HitomiCommon.HitomiThumbnailBig}{args[0]}/{x}.jpg"));
            }
            else
            {
                Console.Instance.WriteErrorLine($"'{type}' is not correct type. Please input 'small' or 'big'.");
            }
        }

        /// <summary>
        /// 메타데이터를 다운로드합니다.
        /// </summary>
        static void ProcessDownloadMetadata()
        {
#if false
            Console.Instance.GlobalTask.Add(HitomiData.Instance.DownloadMetadata());
#endif
        }

        /// <summary>
        /// 메타데이터를 로드합니다..
        /// </summary>
        static void ProcessLoadMetadata()
        {
            HitomiIndex.Instance.Load();
            
            if (HitomiIndex.Instance.metadata_collection != null)
            {
                Console.Instance.WriteLine($"Loaded metadata: '{HitomiIndex.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
            }
            else
            {
                Console.Instance.WriteErrorLine("'metadata.json' file does not exist or is a incorrect file.");
            }
        }

        /// <summary>
        /// 히든데이터를 다운로드합니다.
        /// </summary>
        static void ProcessDownloadHidden()
        {
            //Console.Instance.GlobalTask.Add(HitomiData.Instance.DownloadHiddendata());
        }

        /// <summary>
        /// 히든데이터를 로드합니다..
        /// </summary>
        static void ProcessLoadHidden()
        {
            //HitomiData.Instance.LoadHiddendataJson();

            //if (HitomiData.Instance.metadata_collection != null)
            //{
            //    Console.Instance.WriteLine($"Loaded metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
            //}
            //else
            //{
            //    Console.Instance.WriteErrorLine("'hidden.json' file does not exist or is a incorrect file.");
            //}
        }

        /// <summary>
        /// 데이터를 동기화합니다.
        /// </summary>
        static void ProcessSync()
        {
            //Console.Instance.GlobalTask.Add(HitomiData.Instance.Synchronization());
        }

        /// <summary>
        /// 데이터를 로드합니다.
        /// </summary>
        static void ProcessLoad()
        {
            ProcessLoadMetadata();
            ProcessLoadHidden();
        }

        /// <summary>
        /// 작품을 검색합니다.
        /// </summary>
        static void ProcessSearch(string[] args, bool show_all)
        {
            if (HitomiIndex.Instance.metadata_collection == null)
            {
                Console.Instance.WriteErrorLine($"Please load metadatas before searching!.");
                return;
            }

            Console.Instance.GlobalTask.Add(Task.Run(async () =>
            {
                //var result = await HitomiDataParser.SearchAsync(args[0] + " " + Instance.setter);
                //result.Reverse();
                //if (result.Count == 0)
                //{
                //    Console.Instance.WriteLine("No results were found for your search.");
                //    return;
                //}
                //foreach (var metadata in result)
                //{
                //    if (show_all)
                //    {
                //        string artists = metadata.Artists != null ? string.Join(", ", metadata.Artists) : "N/A";
                //        string tags = metadata.Tags != null ? string.Join(", ", metadata.Tags) : "";
                //        string series = metadata.Parodies != null ? string.Join(", ", metadata.Parodies) : "";
                //        string character = metadata.Characters != null ? string.Join(", ", metadata.Characters) : "";
                //        string group = metadata.Groups != null ? string.Join(", ", metadata.Groups) : "";
                //        string lang = metadata.Language != null ? metadata.Language : "";
                //        string type = metadata.Type != null ? metadata.Type : "";
                //
                //        Console.Instance.WriteLine($"{metadata.ID.ToString().PadLeft(8)} | {artists.PadLeft(15)} | {metadata.Name} | {lang} | {type} | {series} | {character} | {group} | {tags}");
                //    }
                //    else
                //    {
                //        string artist = metadata.Artists != null ? metadata.Artists[0] : "N/A";
                //        Console.Instance.WriteLine($"{metadata.ID.ToString().PadLeft(8)} | {artist.PadLeft(15)} | {metadata.Name}");
                //    }
                //}
                //Console.Instance.WriteLine($"Found {result.Count} results.");
            }));
        }

        /// <summary>
        /// 가장 최근 작품의 업로드 시간을 가져옵니다.
        /// </summary>
        static void ProcessLatest()
        {
            Console.Instance.WriteLine($"{HitomiIndex.Instance.metadata_collection[0].ID}");

            using (var wc = new System.Net.WebClient())
            {
                string target = wc.DownloadString("https://hitomi.la/galleries/" + HitomiIndex.Instance.metadata_collection[0].ID + ".html");
                string date_text = Regex.Split(Regex.Split(target, @"<span class=""date"">")[1], @"</span>")[0];
                Console.Instance.WriteLine(DateTime.Parse(date_text).Ticks.ToString());
                Console.Instance.WriteLine(DateTime.Parse(date_text).ToString());
            }
        }

        /// <summary>
        /// 추천된 작가들을 보여줍니다.
        /// </summary>
        static void ProcessRank()
        {
            for (int i = 0; i < HitomiAnalysis.Instance.Rank.Count; i++)
            {
                Console.Instance.WriteLine($"{(i + 1).ToString().PadLeft(5)}: {HitomiAnalysis.Instance.Rank[i].Item1} ({HitomiAnalysis.Instance.Rank[i].Item2})");
            }
        }

        /// <summary>
        /// 다운로드된 작품들의 태그 리스트를 보여줍니다.
        /// </summary>
        static void ProcessTagList()
        {
            Dictionary<string, int> tags_map = new Dictionary<string, int>();

            if (!HitomiAnalysis.Instance.UserDefined)
            {
                foreach (var log in HitomiLog.Instance.GetEnumerator().Where(log => log.Tags != null))
                {
                    foreach (var tag in log.Tags)
                    {
                        if (Settings.Instance.HitomiAnalysis.UsingOnlyFMTagsOnAnalysis &&
                            !tag.StartsWith("female:") && !tag.StartsWith("male:")) continue;
                        if (tags_map.ContainsKey(HitomiLegalize.LegalizeTag(tag)))
                            tags_map[HitomiLegalize.LegalizeTag(tag)] += 1;
                        else
                            tags_map.Add(HitomiLegalize.LegalizeTag(tag), 1);
                    }
                }
            }

            var list = tags_map.ToList();
            if (HitomiAnalysis.Instance.UserDefined)
                list = HitomiAnalysis.Instance.CustomAnalysis.Select(x => new KeyValuePair<string, int>(x.Item1, x.Item2)).ToList();
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < list.Count; i++)
            {
                Console.Instance.WriteLine($"{(i + 1).ToString().PadLeft(5)}: {list[i].Key} ({list[i].Value})");
            }
        }

        /// <summary>
        /// 403 Forbidden, 404 Not Found 데이터에대한 탐색을 시험합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessForbidden(string[] args)
        {
            Console.Instance.WriteLine(Monitor.SerializeObject(HCommander.GetArticleData(Convert.ToInt32(args[0]))));
        }
        
        /// <summary>
        /// 각종 기능을 테스트합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessTest(string[] args)
        {
            switch (args[0].ToInt32())
            {
                //
                //  Save and beautify metadatas
                //
                case 0:
                    {
                        var hiddendata = JsonConvert.DeserializeObject<List<HitomiArticle>>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "hiddendata.json")));
                        Func<HitomiArticle, NHitomiArticle> r2n = (ha) =>
                        {
                            return new NHitomiArticle
                            {
                                Artists = ha.Artists,
                                Characters = ha.Characters,
                                Groups = ha.Groups,
                                Language = ha.Language,
                                Tags = ha.Tags,
                                Type = ha.Type,
                                DateTime = ha.DateTime,
                                Thumbnail = ha.Thumbnail,
                                Magic = ha.Magic,
                                Title = ha.Title
                            };
                        };

                        var jj = hiddendata.Select(x => r2n(x));
                        var json = JsonConvert.SerializeObject(jj, Formatting.Indented);
                        using (var fs = new StreamWriter(new FileStream("hiddendata_beautify.json", FileMode.Create, FileAccess.Write)))
                        {
                            fs.Write(json);
                        }

                        var json2 = JsonConvert.SerializeObject(jj, Formatting.None);
                        using (var fs = new StreamWriter(new FileStream("hiddendata_nonbeautify.json", FileMode.Create, FileAccess.Write)))
                        {
                            var bytes = json2.Zip();
                            fs.BaseStream.Write(json2.Zip(), 0, bytes.Length);
                        }
                    }
                    break;

                //
                //  Load metadatas
                //
                case 1:
                    {
                        var bytes = File.ReadAllBytes("hiddendata_nonbeautify.json");
                        var json = JsonConvert.DeserializeObject<List<NHitomiArticle>>(bytes.Unzip());
                        Console.Instance.Write($"{json.Count}");
                    }
                    break;

                case 2:
                    {
                        var str = File.ReadAllText("hiddendata.json");
                        File.WriteAllBytes("hiddendata.compress", str.Zip());
                    }
                    break;
                case 3:
                    {
                        var str = File.ReadAllText("metadata.json");
                        File.WriteAllBytes("metadata.compress", str.Zip());
                    }
                    break;

                case 4:
                    {
                        var str = JsonConvert.DeserializeObject<List<HitomiArticle>>(File.ReadAllText("hiddendata.json"));
                        using (var fs = new StreamWriter(new FileStream("hiddendata.json", FileMode.Create, FileAccess.Write)))
                        {
                            fs.Write(JsonConvert.SerializeObject(str, Formatting.None));
                        }
                    }
                    break;

                case 5:
                    {
                        var bytes = File.ReadAllBytes("metadata.compress");
                        File.WriteAllText("metadata.json", bytes.Unzip());
                    }
                    break;
                case 6:
                    {
                        var bytes = File.ReadAllBytes("hiddendata.compress");
                        File.WriteAllText("hiddendata.json", bytes.Unzip());
                    }
                    break;

                case 7:
                    {
                        HitomiExplore.exploreNullSpace().ForEach(x => Console.Instance.WriteLine($"{x.Item1} {x.Item2} {x.Item3}"));
                    }
                    break;

                case 8:
                    {
                        HitomiData.Instance.LoadMetadataJson();
                        HitomiData.Instance.LoadHiddendataJson();
                        HitomiIndex.MakeIndex();
                        var str = File.ReadAllBytes("index-metadata.json");
                        File.WriteAllBytes("index-metadata.compress", str.ZipByte());
                    }
                    break;

                case 9:
                    {
                        //var hidden = JsonConvert.DeserializeObject<List<HitomiArticle>>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "hiddendata.json")));
                        //var gall = JsonConvert.DeserializeObject<List<HitomiArticle>>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "galleries.json")));


                        //for (int i = 0; i < gall.Count; i++)
                        //    for (int j = 0; j < hidden.Count; j++)
                        //        if (gall[i].Magic == hidden[j].Magic)
                        //        {
                        //            hidden[j].Groups = gall[i].Groups;
                        //            hidden[j].Characters = gall[i].Characters;
                        //        }

                        //JsonSerializer serializer = new JsonSerializer();
                        //serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
                        //serializer.NullValueHandling = NullValueHandling.Ignore;

                        //using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "hiddendata2.json")))
                        //using (JsonWriter writer = new JsonTextWriter(sw))
                        //{
                        //    serializer.Serialize(writer, hidden);
                        //}

                        var x = new HitomiIndexDataModel();
                        x.index = HitomiIndex.Instance.index;
                        x.metadata = HitomiIndex.Instance.metadata_collection;
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
                        serializer.NullValueHandling = NullValueHandling.Ignore;

                        Monitor.Instance.Push("Write file: metadata-index.json");
                        using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "metadata-index.json")))
                        using (JsonWriter writer = new JsonTextWriter(sw))
                        {
                            serializer.Serialize(writer, x);
                        }

                        HitomiData.Instance.LoadMetadataJson();
                        HitomiData.Instance.LoadHiddendataJson();
                        Monitor.Instance.Push("Write file: metadata-noptimized.json");
                        using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "metadametadata-noptimizedta.json")))
                        using (JsonWriter writer = new JsonTextWriter(sw))
                        {
                            serializer.Serialize(writer, HitomiData.Instance.metadata_collection);
                        }
                    }
                    break;

                case 10:

                    {
                        foreach (var x in HitomiIndex.Instance.tagdata_collection.artist)
                            Console.Instance.Write(x.Tag + ", ");
                    }

                    break;
            }
        }

    }
}
