﻿/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Koromo_Copy.Script.SRCAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// Script 콘솔 옵션입니다.
    /// </summary>
    public class ScriptConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--test", CommandType.OPTION, Pipe = true, DefaultArgument = true)]
        public bool Test;
    }

    /// <summary>
    /// </summary>
    public class ScriptConsole : IConsole
    {
        /// <summary>
        /// Script 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            ScriptConsoleOption option = CommandLineParser<ScriptConsoleOption>.Parse(arguments);

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
            else if (option.Test)
            {
                ProcessTest();
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
                "Script Console\r\n" +
                "\r\n"
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
        /// 
        /// </summary>
        static void ProcessTest()
        {
            var danbooru = Encoding.UTF8.GetString(Convert.FromBase64String("IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIERhbmJvb3J1IERvd25sb2FkZXINCiMjDQoNCiMjDQojIyBBdHRyaWJ1dGVzDQojIw0KJFNjcmlwdE5hbWUgPSAiZGFuYm9vcnUtcGFnZXMiDQokU2NyaXB0VmVyc2lvbiA9ICIwLjEiDQokU2NyaXB0QXV0aG9yID0gImRjLWtvcm9tbyINCiRTY3JpcHRGb2xkZXJOYW1lID0gImRhbmJvb3J1Ig0KJFNjcmlwdFJlcXVlc3ROYW1lID0gImRhbmJvb3J1Ig0KJFVSTFNwZWNpZmllciA9ICJodHRwczovL2RhbmJvb3J1LmRvbm1haS51cy8iDQokVXNpbmdEcml2ZXIgPSAwDQoNCiMjDQojIyBQcm9jZWR1cmUNCiMjDQpyZXF1ZXN0X3VybCA9ICRSZXF1ZXN0VVJMDQptYXhfcGFnZSA9ICRJbmZpbml0eQ0KDQpsb29wIChpID0gMSB0byBtYXhfcGFnZSkgWw0KICAgICRMb2FkUGFnZShjb25jYXQocmVxdWVzdF91cmwsICImcGFnZT0iLCBpKSkNCiAgICBzdWJfdXJscyA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzNdL2RpdlsxXS9zZWN0aW9uWzFdL2RpdlszXS9kaXZbMV0vYXJ0aWNsZVt7MStpKjF9XS9hWzFdLCAjYXR0cltocmVmXSwgI2Zyb250W2h0dHBzOi8vZGFuYm9vcnUuZG9ubWFpLnVzXSIpDQoNCiAgICBmb3JlYWNoIChzdWJfdXJsIDogc3ViX3VybHMpIFsNCiAgICAgICAgJExvYWRQYWdlKHN1Yl91cmwpDQogICAgICAgIGltYWdlX3VybCA9ICIiDQogICAgICAgIGlmIChlcXVhbChjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbMV0vc3BhblsxXS9hWzFdLCAjYXR0cltpZF0iKVswXSwgImltYWdlLXJlc2l6ZS1saW5rIikpIFsNCiAgICAgICAgICAgIGltYWdlX3VybCA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzNdL2RpdlsxXS9zZWN0aW9uWzFdL2RpdlsxXS9zcGFuWzFdL2FbMV0sICNhdHRyW2hyZWZdIilbMF0NCiAgICAgICAgXSBlbHNlIGlmIChlcXVhbChjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbMl0vc3BhblsxXS9hWzFdLCAjYXR0cltpZF0iKVswXSwgImltYWdlLXJlc2l6ZS1saW5rIikpIFsNCiAgICAgICAgICAgIGltYWdlX3VybCA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzNdL2RpdlsxXS9zZWN0aW9uWzFdL2RpdlsyXS9zcGFuWzFdL2FbMV0sICNhdHRyW2hyZWZdIilbMF0NCiAgICAgICAgXSBlbHNlIFsNCiAgICAgICAgICAgIGltYWdlX3VybCA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzNdL2RpdlsxXS9zZWN0aW9uWzFdL3NlY3Rpb25bMV0vaW1nWzFdLCAjYXR0cltzcmNdIilbMF0NCiAgICAgICAgXQ0KICAgICAgICBmaWxlX25hbWUgPSBzcGxpdChpbWFnZV91cmwsICIvIilbLTFdDQogICAgICAgICRBcHBlbmRJbWFnZShpbWFnZV91cmwsIGZpbGVfbmFtZSkNCiAgICBdDQoNCiAgICBpZiAoZXF1YWwoJExhdGVzdEltYWdlc0NvdW50LCAwKSkgWyANCiAgICAgICAgJEV4aXRMb29wKCkNCiAgICBdDQoNCiAgICAkQ2xlYXJlSW1hZ2VzQ291bnQoKQ0KXQ0KDQokUmVxdWVzdERvd25sb2FkKCk="));
            var mangashowme_series = Encoding.UTF8.GetString(Convert.FromBase64String("IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIE1hbmdhc2hvd21lIFNlcmllcyBEb3dubG9hZGVyDQojIw0KDQojIw0KIyMgQXR0cmlidXRlcw0KIyMNCiRTY3JpcHROYW1lID0gIm1hbmdhc2hvd21lLXNlcmllcyINCiRTY3JpcHRWZXJzaW9uID0gIjAuMSINCiRTY3JpcHRBdXRob3IgPSAiZGMta29yb21vIg0KJFNjcmlwdEZvbGRlck5hbWUgPSAibWFuZ2FzaG93bWUiDQokU2NyaXB0UmVxdWVzdE5hbWUgPSAibWFuZ2FzaG93bWUiDQokVVJMU3BlY2lmaWVyID0gImh0dHBzOi8vbWFuZ2FzaG93Lm1lL2Jicy9wYWdlLnBocCINCiRVc2luZ0RyaXZlciA9IDANCg0KIyMNCiMjIFByb2NlZHVyZQ0KIyMNCnJlcXVlc3RfdXJsID0gJFJlcXVlc3RVUkwNCm1heF9wYWdlID0gJEluZmluaXR5DQoNCnRpdGxlID0gY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0iKVswXQ0Kc3ViX3VybHMgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlsyXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzJdL2RpdlsxXS9kaXZbMV0vZGl2W3sxK2kqMX1dL2FbMV0sICNhdHRyW2hyZWZdIikNCnN1Yl90aXRsZXMgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlsyXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzJdL2RpdlsxXS9kaXZbMV0vZGl2W3sxK2kqMX1dL2FbMV0vZGl2WzFdLCAjaHRleHQiKQ0KDQpsb29wIChpID0gMCB0byBhZGQoY291bnQoc3ViX3VybHMpLCAtMSkpIFsNCiAgICAkTG9hZFBhZ2Uoc3ViX3VybHNbaV0pDQogICAgaW1hZ2VzID0gY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbMV0vZm9ybVsxXS9kaXZbMV0vZGl2W3sxK2kqMX1dL2RpdlsxXSwgI2F0dHJbc3R5bGVdLCAjcmVnZXhbaHR0cHM6Ly9bXlxcKV0qXSIpDQogICAgZm9yZWFjaCAoaW1hZ2UgOiBpbWFnZXMpIFsNCiAgICAgICAgZmlsZW5hbWUgPSBzcGxpdChpbWFnZSwgIi8iKVstMV0NCiAgICAgICAgJEFwcGVuZEltYWdlKGltYWdlLCBjb25jYXQodGl0bGUsICIvIiwgc3ViX3RpdGxlc1tpXSwgIi8iLCBmaWxlbmFtZSkpDQogICAgXQ0KXQ0KDQokUmVxdWVzdERvd25sb2FkKCk="));
            var raw_script = mangashowme_series.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
                ).ToList();

            //SRCALParser parser = new SRCALParser();
            //Monitor.SerializeObject(parser.Parse(raw_script));
            SRCALEngine engine = new SRCALEngine(null);
            engine.ParseScript(raw_script);
            engine.RunScript("https://mangashow.me/bbs/page.php?hid=manga_detail&manga_name=%EC%99%95%20%EA%B2%8C%EC%9E%84%20%EC%A2%85%EA%B7%B9");
        }
    }
}
