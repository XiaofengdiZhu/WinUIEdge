using System.Collections.Generic;

namespace Edge
{
    public class JsonData
    {
        public string Theme { get; set; }
        public string StartPageBehavior { get; set; }
        public string SpecificUri { get; set; }
        public bool ShowHomeButton { get; set; }

        public string SearchEngine { get; set; }
    }

    public class JsonDataList
    {
        public static List<string> ApplicationThemeList = ["Mica", "Mica Alt", "Acrylic", "None"];
        public static List<string> StartPageBehaviorList = ["���±�ǩҳ", "��ָ����ҳ��"];

        public static Dictionary<string, string> SearchEngineDictionary = new()
        {
            { "Bing", "https://bing.com/?q=" },
            { "Google", "https://www.google.com/search?q=" },
            { "�ٶ�", "https://www.baidu.com/s?ie={inputEncoding}&wd=" },
            { "�ѹ�", "https://www.sogou.com/web?ie={inputEncoding}&query=" },
            { "360", "https://www.so.com/s?ie={inputEncoding}&q=" },
            { "Github", "https://github.com/search?q=" },
            { "Gitee", "https://gitee.com/search?utf8=%E2%9C%93&q=" },
            { "Bilibili", "https://search.bilibili.com/all?keyword=" }
        };
    }
}