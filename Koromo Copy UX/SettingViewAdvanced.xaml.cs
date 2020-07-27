﻿/***

   Copyright (C) 2018-2020. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// SettingViewAdvanced.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewAdvanced : UserControl
    {
        public SettingViewAdvanced()
        {
            InitializeComponent();

            DataContext = new Domain.SettingAdvancedViewModel();
        }
    }
}
