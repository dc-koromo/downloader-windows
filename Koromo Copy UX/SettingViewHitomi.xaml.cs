﻿/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// SettingViewHitomi.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewHitomi : UserControl
    {
        public SettingViewHitomi()
        {
            InitializeComponent();

            AddLanguages();
            Language.Text = HitomiLegalize.DeLegalizeLanguage(Settings.Instance.Hitomi.Language);
            Path.Text = Settings.Instance.Hitomi.Path;
            ExclusiveTags.Text = string.Join(", ",Settings.Instance.Hitomi.ExclusiveTag);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var value = (sender as TextBox).Text;

            if (string.IsNullOrWhiteSpace((value ?? "").ToString()))
            {
                Error.Text = "경로는 비어있으면 안됩니다.";
                Error.Visibility = Visibility.Visible;
                return;
            }

            var ss = "";
            foreach (var s in value.ToString().Split('\\'))
                if (s.Contains("{")) break;
                else ss += s + "\\";
            if (!Directory.Exists(ss))
            {
                Error.Text = "유효하지 않은 경로입니다.";
                Error.Visibility = Visibility.Visible;
                return;
            }

            if (!(value.ToString().ToLower().Contains("{id}") || value.ToString().ToLower().Contains("{title}")))
            {
                Error.Text = "{Id} 또는 {Title}를 반드시 하나이상 포함해야합니다.";
                Error.Visibility = Visibility.Visible;
                return;
            }

            var valid_tokens = new string[]
            {
                "{title}",
                "{artists}",
                "{id}",
                "{type}",
                "{date}",
                "{series}",
                "{search}"
            };
            var regex = Regex.Matches(value.ToString(), @"(\{.*?\})");
            if (!regex.OfType<Match>().All(x => valid_tokens.Contains(x.Value.ToLower())))
            {
                Error.Text = "유효하지 않은 토큰이 있습니다. " + string.Join(", ", regex.OfType<Match>().Where(x => !valid_tokens.Contains(x.Value.ToLower())));
                Error.Visibility = Visibility.Visible;
                return;
            }

            Error.Visibility = Visibility.Collapsed;
            Settings.Instance.Hitomi.Path = value;
            Settings.Instance.Save();
        }

        public void AddLanguages()
        {
            var langs = new string[] {
                "모든 언어",
                "N/A",
                "한국어",
                "日本語",
                "English",
                "Español",
                "ไทย",
                "Deutsch",
                "中文",
                "Português",
                "Français",
                "Tagalog",
                "Русский",
                "Italiano",
                "polski",
                "tiếng việt",
                "magyar",
                "Čeština",
                "Bahasa Indonesia",
                "العربية"
            };
            langs.ToList().ForEach(lang => Language.Items.Add(new ComboBoxItem { Content = lang }));
        }

        private void Language_DropDownClosed(object sender, EventArgs e)
        {
            Settings.Instance.Hitomi.Language = HitomiLegalize.LegalizeLanguage(Language.Text);
            Settings.Instance.Save();
            HitomiIndex.Instance.RebuildTagData();
        }

        private void ExclusiveTags_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Hitomi.ExclusiveTag = ExclusiveTags.Text.Split(',').Select(x=>x.Trim()).ToArray();
            Settings.Instance.Save();
        }
    }
}
