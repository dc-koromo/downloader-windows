﻿/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System.Collections.Generic;

namespace Koromo_Copy.Interface
{
    /// <summary>
    /// 한 작품을 나타내는 단위입니다.
    /// </summary>
    public interface IArticle
    {
        string Thumbnail { get; set; }
        string Title { get; set; }
        List<string> ImagesLink { get; set; }
    }

    /// <summary>
    /// 한 시리즈를 나타내는 단위입니다.
    /// </summary>
    public interface ISeries<T> where T : IArticle
    {
        string Thumbnail { get; set; }
        string Title { get; set; }
        string[] Archive { get; set; }
        List<T> Articles { get; set; }
    }
}
