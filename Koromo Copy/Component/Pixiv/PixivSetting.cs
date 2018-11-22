﻿/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Pixiv
{
    public class PixivSetting
    {
        [JsonProperty]
        public string Path;

        [JsonProperty]
        public string Id;

        [JsonProperty]
        public string Password;
    }
}
