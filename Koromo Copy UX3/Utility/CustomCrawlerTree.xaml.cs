﻿/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// CustomCrawlerTree.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomCrawlerTree : Window
    {
        HtmlNode root_node;
        List<HtmlNode> marking;

        public CustomCrawlerTree(HtmlNode node, List<HtmlNode> marking)
        {
            InitializeComponent();

            root_node = node;
            this.marking = new List<HtmlNode>();
            foreach (var snode in marking)
            {
                var tnode = snode;
                while (tnode != root_node)
                {
                    this.marking.Add(tnode);
                    tnode = tnode.ParentNode;
                }
            }
            Loaded += CustomCrawlerTree_Loaded;
        }

        bool load = false;
        private void CustomCrawlerTree_Loaded(object sender, RoutedEventArgs e)
        {
            if (load) return;
            load = true;
            
            var tvi = new TreeViewItem();
            tvi.Header = make_header2(root_node);
            recursive_append(tvi, root_node);
            Tree.Items.Add(tvi);
        }

        private string make_header(HtmlNode node)
        {
            var builder = new StringBuilder();
            if (marking.Contains(node))
                builder.Append('★');
            if (node.Name == "#text")
                return node.InnerText.Trim();
            if (node.Name == "#comment")
                return node.InnerHtml.Trim();
            else
                builder.Append($"<{node.Name} ");
            node.Attributes.ToList().ForEach(x => builder.Append($"{x.Name}=\"{x.Value}\" "));
            return builder.ToString().Trim() + ">";
        }

        private TextBlock make_header2(HtmlNode node)
        {
            var block = new TextBlock { TextWrapping = TextWrapping.Wrap };
            if (marking.Contains(node))
                block.Inlines.Add(new Run { Foreground = Brushes.Brown, Text = "★" });
            if (node.Name == "#text")
            {
                block.Text = node.InnerText.Trim();
                return block;
            }
            if (node.Name == "#comment")
            {
                block.Text = node.InnerHtml.Trim();
                block.Foreground = new SolidColorBrush(Color.FromRgb(0x05, 0x8B, 0x00));
                return block;
            }
            block.Inlines.Add(new Run { Foreground = Brushes.Black, Text = "<" });
            block.Inlines.Add(new Run { Foreground = new SolidColorBrush(Color.FromRgb(0x46, 0x40, 0xE8)), Text = node.Name });
            node.Attributes.ToList().ForEach(x =>
            {
                block.Inlines.Add(new Run { Foreground = Brushes.HotPink, Text = " "+x.Name });
                block.Inlines.Add(new Run { Foreground = Brushes.Black, Text = "=" });
                block.Inlines.Add(new Run { Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x3E, 0xB1)), Text = $"\"{x.Value}\""  });
            });
            block.Inlines.Add(new Run { Foreground = Brushes.Black, Text = ">" });
            return block;
        }

        private void recursive_append(TreeViewItem item, HtmlNode node)
        {
            foreach (var snode in node.ChildNodes)
            {
                var tvi = new TreeViewItem();
                var header = make_header(snode);
                if (string.IsNullOrEmpty(header))
                    continue;
                tvi.Header = make_header2(snode);//new TextBlock { Text = make_header(snode), TextWrapping = TextWrapping.Wrap };
                item.Items.Add(tvi);
                recursive_append(tvi, snode);
            }
        }
    }
}
