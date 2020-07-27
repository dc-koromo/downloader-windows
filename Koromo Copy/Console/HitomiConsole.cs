﻿/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 히토미 콘솔 옵션입니다.
    /// </summary>
    public class HitomiConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-article", CommandType.ARGUMENTS, Help = "use -article <Hitomi Number>")]
        public string[] Article;
        [CommandLine("-image", CommandType.ARGUMENTS, Help = "use -image <Hitomi Number> [-type=small | big]")]
        public string[] ImageLink;
        [CommandLine("-type", CommandType.EQUAL)]
        public string Type;

        [CommandLine("-downloadmetadata", CommandType.OPTION)]
        public bool DownloadMetadata;
        [CommandLine("-loadmetadata", CommandType.OPTION)]
        public bool LoadMetadata;

        [CommandLine("-downloadhidden", CommandType.OPTION)]
        public bool DownloadHidden;
        [CommandLine("-loadhidden", CommandType.OPTION)]
        public bool LoadHidden;

        [CommandLine("-sync", CommandType.OPTION)]
        public bool Sync;
        [CommandLine("-load", CommandType.OPTION)]
        public bool Load;

        [CommandLine("-all", CommandType.OPTION)]
        public bool ShowAllSearchList;
        [CommandLine("-search", CommandType.ARGUMENTS)]
        public string[] Search;
        [CommandLine("-setsearch", CommandType.ARGUMENTS)]
        public string[] SetSearchToken;

        [CommandLine("-syncdate", CommandType.OPTION)]
        public bool SyncDate;
        [CommandLine("-latest", CommandType.OPTION)]
        public bool Latest;
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
                "\r\n" +
                " -article <Hitomi Number> : Show article info.\r\n" +
                " -image <Hitomi Number> [-type=small | big]: Get Image Link.\r\n" +
                " -downloadmetadata, -loadmetadata, -downloadhidden, -loadhidden, -sync, -load : Manage Metadata.\r\n" +
                " -search <Search What> [-all] : Language Dependent metadata seraching.\r\n" +
                " -setsearch <Place What> : Fix specific search token.\r\n" +
                " -syncdate : Synchronize HitomiDate data."
                );
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
            Console.Instance.GlobalTask.Add(HitomiData.Instance.DownloadMetadata());
        }

        /// <summary>
        /// 메타데이터를 로드합니다..
        /// </summary>
        static void ProcessLoadMetadata()
        {
            HitomiData.Instance.LoadMetadataJson();
            
            if (HitomiData.Instance.metadata_collection != null)
            {
                Console.Instance.WriteLine($"Loaded metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
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
            Console.Instance.GlobalTask.Add(HitomiData.Instance.DownloadHiddendata());
        }

        /// <summary>
        /// 히든데이터를 로드합니다..
        /// </summary>
        static void ProcessLoadHidden()
        {
            HitomiData.Instance.LoadHiddendataJson();

            if (HitomiData.Instance.metadata_collection != null)
            {
                Console.Instance.WriteLine($"Loaded metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
            }
            else
            {
                Console.Instance.WriteErrorLine("'hidden.json' file does not exist or is a incorrect file.");
            }
        }

        /// <summary>
        /// 데이터를 동기화합니다.
        /// </summary>
        static void ProcessSync()
        {
            Console.Instance.GlobalTask.Add(HitomiData.Instance.Synchronization());
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
            if (HitomiData.Instance.metadata_collection == null)
            {
                Console.Instance.WriteErrorLine($"Please load metadatas before searching!.");
                return;
            }

            Console.Instance.GlobalTask.Add(Task.Run(async () =>
            {
                var result = await HitomiDataParser.SearchAsync(args[0] + " " + Instance.setter);
                result.Reverse();
                if (result.Count == 0)
                {
                    Console.Instance.WriteLine("No results were found for your search.");
                    return;
                }
                foreach (var metadata in result)
                {
                    if (show_all)
                    {
                        string artists = metadata.Artists != null ? string.Join(", ", metadata.Artists) : "N/A";
                        string tags = metadata.Tags != null ? string.Join(", ", metadata.Tags) : "";
                        string series = metadata.Parodies != null ? string.Join(", ", metadata.Parodies) : "";
                        string character = metadata.Characters != null ? string.Join(", ", metadata.Characters) : "";
                        string group = metadata.Groups != null ? string.Join(", ", metadata.Groups) : "";
                        string lang = metadata.Language != null ? metadata.Language : "";
                        string type = metadata.Type != null ? metadata.Type : "";

                        Console.Instance.WriteLine($"{metadata.ID.ToString().PadLeft(8)} | {artists.PadLeft(15)} | {metadata.Name} | {lang} | {type} | {series} | {character} | {group} | {tags}");
                    }
                    else
                    {
                        string artist = metadata.Artists != null ? metadata.Artists[0] : "N/A";
                        Console.Instance.WriteLine($"{metadata.ID.ToString().PadLeft(8)} | {artist.PadLeft(15)} | {metadata.Name}");
                    }
                }
                Console.Instance.WriteLine($"Found {result.Count} results.");
            }));
        }

        /// <summary>
        /// 가장 최근 작품의 업로드 시간을 가져옵니다.
        /// </summary>
        static void ProcessLatest()
        {
            Console.Instance.WriteLine($"{HitomiData.Instance.metadata_collection[0].ID}");

            using (var wc = new System.Net.WebClient())
            {
                string target = wc.DownloadString("https://hitomi.la/galleries/" + HitomiData.Instance.metadata_collection[0].ID + ".html");
                string date_text = Regex.Split(Regex.Split(target, @"<span class=""date"">")[1], @"</span>")[0];
                Console.Instance.WriteLine(DateTime.Parse(date_text).Ticks.ToString());
                Console.Instance.WriteLine(DateTime.Parse(date_text).ToString());
            }
        }
    }
}
