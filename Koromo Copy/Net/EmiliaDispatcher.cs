﻿/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koromo_Copy.Net
{
    /// <summary>
    /// 파일단위의 세그먼트입니다.
    /// </summary>
    public class EmiliaFileSegment
    {
        /// <summary>
        /// 다운로드할 파일의 고유번호입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 파일이 속한 아티클의 고유번호입니다.
        /// </summary>
        public int ArticleIndex;

        /// <summary>
        /// 다운로드 Url입니다.
        /// </summary>
        public string Url;

        /// <summary>
        /// 다운로드할 Url의 부가정보입니다.
        /// </summary>
        public SemaphoreExtends Extends;

        /// <summary>
        /// 다운로드할 파일의 이름입니다.
        /// </summary>
        public string FileName;

        /// <summary>
        /// 다운로드할 파일의 크기입니다.
        /// </summary>
        public long Size;
    }

    public class EmilieFileStatusSegment
    {
        /// <summary>
        /// 다운로드할 파일의 고유번호입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 파일이 속한 아티클의 고유번호입니다.
        /// </summary>
        public int ArticleIndex;

        /// <summary>
        /// 다운로드한 크기입니다.
        /// 이 크기는 파일의 전체크기가 아닌 다운로드 블록당 다운로드한 크기입니다.
        /// </summary>
        public long DownloadSize;
    }

    /// <summary>
    /// 아티클단위의 세그먼트입니다.
    /// </summary>
    public class EmiliaArticleSegment
    {
        /// <summary>
        /// 다운로드할 아티클의 고유번호입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 아티클이 속한 시리즈의 고유번호입니다.
        /// </summary>
        public int SereisIndex;

        /// <summary>
        /// 아티클의 폴더이름입니다.
        /// </summary>
        public string FolderName;

        /// <summary>
        /// 아티클의 제목입니다.
        /// </summary>
        public string Name;

        /// <summary>
        /// 다운로드할 파일들의 정보입니다.
        /// </summary>
        public List<EmiliaFileSegment> Files;
    }

    /// <summary>
    /// 시리즈단위의 세그먼트입니다.
    /// </summary>
    public class EmiliaSeriesSegment
    {
        /// <summary>
        /// 다운로드할 시리즈의 고유번호입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 시리즈를 다운로드할 최상위 폴더입니다.
        /// </summary>
        public string Path;

        /// <summary>
        /// 시리즈의 제목입니다.
        /// </summary>
        public string Title;

        /// <summary>
        /// 아티클들의 정보입니다.
        /// </summary>
        public List<EmiliaArticleSegment> Articles;
    }

    /// <summary>
    /// 디스패쳐에 맡길 정보들 입니다.
    /// </summary>
    public class DispatchInformation
    {
        /// <summary>
        /// 다운로드할 파일의 사이즈를 얻었을 때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> DownloadSize;

        /// <summary>
        /// 다운로드한 바이트 수를 알립니다.
        /// </summary>
        public event EventHandler<EmilieFileStatusSegment> DownloadStatus;

        /// <summary>
        /// 재시도한 파일을 알립니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> DownloadRetry;

        /// <summary>
        /// 어떤 파일의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> CompleteFile;

        /// <summary>
        /// 어떤 아티클의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaArticleSegment> CompleteArticle;

        /// <summary>
        /// 시리즈 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler CompleteSeries;
    }

    /// <summary>
    /// 단일 시리즈에 대한 다운로드관리를 지원하는 다운로드 큐 관리자 입니다.
    /// </summary>
    public class EmiliaDispatcher : ILazy<DownloadGroup>
    {
        ISemaphore queue;
        int mutex_count;

        object add_lock = new object();
        object job_lock = new object();

        public ISemaphore Queue { get { return queue; } }

        public EmiliaDispatcher()
        {
            if (!Settings.Instance.Net.UseEmiliaQueue)
            {
                queue = new DownloadQueue(downloadSizeCallback, downloadStatusCallback, downloadRetryCallback);
            }
            else
            {
                queue = new EmiliaQueue(downloadSizeCallback, downloadStatusCallback, downloadRetryCallback);
            }
        }

        Dictionary<int, EmiliaSeriesSegment> series_dictionary;
        Dictionary<int, DispatchInformation> dispatcher_dictionary;

        #region Global Event

        /// <summary>
        /// 다운로드할 파일의 사이즈를 얻었을 때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> DownloadSize;

        /// <summary>
        /// 다운로드한 바이트 수를 알립니다.
        /// </summary>
        public event EventHandler<EmilieFileStatusSegment> DownloadStatus;

        /// <summary>
        /// 재시도한 파일을 알립니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> DownloadRetry;

        /// <summary>
        /// 어떤 파일의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> CompleteFile;

        /// <summary>
        /// 어떤 아티클의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaArticleSegment> CompleteArticle;

        /// <summary>
        /// 어떤 시리즈 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaSeriesSegment> CompleteSeries;

        /// <summary>
        /// 모든 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler DownloadComplete;

        #endregion

        private void downloadSizeCallback(string uri, long size, object obj)
        {
        }

        private void downloadStatusCallback(string uri, int size, object obj)
        {
        }

        private void downloadRetryCallback(string uri, object obj)
        {
        }

        private void downloadCallback(string url, string filename, object obj)
        {
        }

        /// <summary>
        /// 큐를 일시정지합니다.
        /// </summary>
        public void Preempt()
        {
            queue.Preempt();
            Interlocked.Increment(ref mutex_count);
        }

        /// <summary>
        /// 큐를 재활성화합니다.
        /// </summary>
        public void Reactivation()
        {
            if (Interlocked.Decrement(ref mutex_count) == 0)
                queue.Reactivation();
        }

        /// <summary>
        /// 모든 작업을 취소하고, 다운로드 중인 파일을 삭제합니다.
        /// </summary>
        public void Abort()
        {
            queue.Abort();
        }

        /// <summary>
        /// 특정 작업을 취소합니다.
        /// </summary>
        public void Abort(string url)
        {
            queue.Abort(url);
        }

        /// <summary>
        /// 시리즈를 다운로드하는 새 작업을 추가합니다.
        /// </summary>
        /// <param name="series"></param>
        /// <param name="dispatcher"></param>
        public void Add(EmiliaSeriesSegment series, DispatchInformation dispatcher)
        {
            lock (job_lock)
            {
                series_dictionary.Add(series.Index, series);
                dispatcher_dictionary.Add(series.Index, dispatcher);
            }

            lock (add_lock)
            {
                foreach (var article in series.Articles)
                {
                    var article_folder = Path.Combine(series.Path, article.FolderName);
                    foreach (var file in article.Files)
                    {
                        var file_path = Path.Combine(article_folder, file.FileName);
                        queue.Add(file.Url, file_path, file, downloadCallback, file.Extends);
                    }
                }
            }
        }
    }
}
