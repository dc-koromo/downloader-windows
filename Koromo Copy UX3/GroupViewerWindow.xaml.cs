﻿/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX3.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// GroupInfoWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GroupViewerWindow : Window
    {
        public GroupViewerWindow()
        {
            InitializeComponent();

            DataContext = new Domain.ArtistDataGridViewModel();

            Task.Run(() =>
            {
                var result = HitomiDataParser.SearchAsync("artist:michiking").Result;
                _ = Task.Run(() => LoadThumbnail(result));
            });
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        public GroupViewerWindow(string group)
        {
            InitializeComponent();

            DataContext = new Domain.ArtistDataGridViewModel();
            Title += group;

            var dictionary = new Dictionary<string, int>();
            Task.Run(() =>
            {
                var result = HitomiDataParser.SearchAsync($"group:{group.ToLower().Replace(' ', '_')}").Result;
                _ = Task.Run(() => LoadThumbnail(result));

                foreach (var md in result)
                {
                    if (md.Tags != null)
                        foreach (var tag in md.Tags)
                            if (dictionary.ContainsKey(tag))
                                dictionary[tag] += 1;
                            else
                                dictionary.Add(tag, 1);
                }
            }).ContinueWith(t => {
                var vm = DataContext as Domain.ArtistDataGridViewModel;
                var list = dictionary.ToList();
                list.Sort((a, b) => b.Value.CompareTo(a.Value));
                foreach (var tag in list)
                    vm.Items.Add(new Domain.ArtistDataGridItemViewModel
                    {
                        항목 = tag.Key,
                        카운트 = tag.Value
                    });
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void LoadThumbnail(List<HitomiMetadata> md)
        {
            List<Task> task = new List<Task>();
            foreach (var metadata in md)
            {
                Task.Run(() => LoadThumbnail(metadata));
                Thread.Sleep(100);
            }
        }

        private void LoadThumbnail(HitomiMetadata md)
        {
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                // Put code that needs to run on the UI thread here
                var se = new SearchSimpleElements(HitomiLegalize.MetadataToArticle(md));
                ArticlePanel.Children.Add(se);
                Koromo_Copy.Monitor.Instance.Push("[AddSearchElements] Hitomi Metadata " + md.ID);
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // We know longer need to size to the contents.
            ClearValue(SizeToContentProperty);
            // We want our control to shrink/expand with the window.
            ArticlePanel.ClearValue(WidthProperty);
            ArticlePanel.ClearValue(HeightProperty);
            TagList.ClearValue(WidthProperty);
            TagList.ClearValue(HeightProperty);
            // Don't want our window to be able to get any smaller than this.
            SetValue(MinWidthProperty, this.Width);
            SetValue(MinHeightProperty, this.Height);
        }

        private void DataGridRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tags = TagList.SelectedItems.OfType<ArtistDataGridItemViewModel>().Select(x => x.항목);
            foreach (var control in ArticlePanel.Children.OfType<SearchSimpleElements>())
            {
                if (tags.All(x => (control.Article as HitomiArticle).Tags != null && (control.Article as HitomiArticle).Tags.Contains(x)))
                    control.Select = true;
                else
                {
                    control.Select = false;
                    control.Transparent();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (btn.Tag.ToString() == "DownloadAll")
            {
                int count = 0;
                ArticlePanel.Children.OfType<SearchSimpleElements>().ToList().ForEach(x =>
                {
                    var prefix = HitomiCommon.MakeDownloadDirectory(x.Article as HitomiArticle);
                    Directory.CreateDirectory(prefix);
                    DownloadSpace.Instance.RequestDownload(x.Article.Title,
                        x.Article.ImagesLink.Select(y => HitomiCommon.GetDownloadImageAddress((x.Article as HitomiArticle).Magic, y)).ToArray(),
                        x.Article.ImagesLink.Select(y => Path.Combine(prefix, y)).ToArray(),
                        Koromo_Copy.Net.SemaphoreExtends.Default, prefix, x.Article);
                    count++;
                });
                if (count > 0) MainWindow.Instance.FadeOut_MiddlePopup($"{count}개 항목 다운로드 시작...");
                MainWindow.Instance.Activate();
                MainWindow.Instance.FocusDownload();
                Close();
            }
            else if (btn.Tag.ToString() == "Download")
            {
                int count = 0;
                ArticlePanel.Children.OfType<SearchSimpleElements>().ToList().Where(x => x.Select).ToList().ForEach(x =>
                {
                    var prefix = HitomiCommon.MakeDownloadDirectory(x.Article as HitomiArticle);
                    Directory.CreateDirectory(prefix);
                    DownloadSpace.Instance.RequestDownload(x.Article.Title,
                        x.Article.ImagesLink.Select(y => HitomiCommon.GetDownloadImageAddress((x.Article as HitomiArticle).Magic, y)).ToArray(),
                        x.Article.ImagesLink.Select(y => Path.Combine(prefix, y)).ToArray(),
                        Koromo_Copy.Net.SemaphoreExtends.Default, prefix, x.Article);
                    count++;
                });
                if (count > 0) MainWindow.Instance.FadeOut_MiddlePopup($"{count}개 항목 다운로드 시작...");
                MainWindow.Instance.Activate();
                MainWindow.Instance.FocusDownload();
            }

        }

        public static RoutedCommand Command = new RoutedCommand();
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessCommand((e.Parameter as string)[0]);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProcessCommand((sender as MenuItem).Tag.ToString()[0]);
        }

        private void ProcessCommand(char c)
        {
            switch (c)
            {
                case 'A':
                    ArticlePanel.Children.OfType<SearchSimpleElements>().ToList().ForEach(x => x.Select = true);
                    break;

                case 'C':
                    ArticlePanel.Children.OfType<SearchSimpleElements>().ToList().ForEach(x => {
                        x.Select = false;
                        x.Transparent();
                    });
                    break;

                case 'R':
                    ArticlePanel.Children.OfType<SearchSimpleElements>().ToList().ForEach(x => {
                        x.Select = !x.Select;
                        if (!x.Select) x.Transparent();
                    });
                    break;

                case 'S':
                case 'G':
                    ArticlePanel.Children.OfType<SearchSimpleElements>().ToList().ForEach(x => {
                        if (HitomiLog.Instance.Contains((x.Article as HitomiArticle).Magic))
                        {
                            x.Select = false;
                            x.Transparent();
                        }
                    });
                    break;

                case 'B':
                case 'D':
                    break;
            }
        }
    }
}
