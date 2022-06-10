using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskTopTimer.WebProcess
{
    //不同网站的response定义

    public class PixivSeSe
    {
        /// <summary>
        /// 作品 pid
        /// </summary>
        public int pid { set; get; }
        /// <summary>
        /// 作品所在页
        /// </summary>
        public int p { set; get; }
        /// <summary>
        /// 作者 uid
        /// </summary>
        public int uid { set; get; }
        /// <summary>
        /// 作品标题
        /// </summary>
        public string? title { set; get; }
        /// <summary>
        /// 作者名（入库时，并过滤掉 @ 及其后内容）
        /// </summary>
        public string? author { set; get; }
        /// <summary>
        /// 是否 R18（在库中的分类，不等同于作品本身的 R18 标识）
        /// </summary>
        public bool r18 { set; get; }
        /// <summary>
        /// 原图宽度 px
        /// </summary>
        public int width { set; get; }
        /// <summary>
        /// 原图高度 px
        /// </summary>
        public int height { set; get; }
        /// <summary>
        /// 作品标签，包含标签的中文翻译（有的话）
        /// </summary>
        public List<string?>? tags { set; get; }

        /// <summary>
        /// 图片扩展名
        /// </summary>
        public string? ext { set; get; }
        /// <summary>
        /// 作品上传日期；时间戳，单位为毫秒
        /// </summary>
        public ulong uploadDate { set; get; }
        /// <summary>
        /// 包含了所有指定size的图片地址
        /// </summary>
        public Dictionary<string, string>? urls { set; get; }

    }


    public class PixivResponse
    {
        public string? error { set; get; }
        public List<PixivSeSe?>? data { set; get; }
    }

    public class WallHavenCategories
    {
        public bool none = true;
        public bool general = false;
        public bool anime = false;
        public bool people = false;
    }
    public class WallHavenPurity
    {
        public bool none = true;
        public bool sfw = false;
        public bool sketchy = false;
        public bool nsfw = false;
    }
    public enum WallHavenSorting
    {
        date_added,
        relevance,
        random,
        views,
        favorites,
        toplist
    }
    public static class MyEnumExtensions
    {
        public static string ToDescriptionString(this WallHavenColors val)
        {

            DescriptionAttribute[] attributes = (DescriptionAttribute[])(val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false));
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
    public enum WallHavenColors
    {

        [Description("660000")]
        lonestar,
        [Description("990000")]
        red_berry,
        [Description("cc000")]
        guardsman_red,
        [Description("cc3333")]
        persian_red,
        [Description("ea4c88")]
        french_rose,
        [Description("993399")]
        plum,
        [Description("663399")]
        royal_purple,
        [Description("333399")]
        sapphire,
        [Description("0066cc")]
        science_blue,
        [Description("0099cc")]
        pacific_blue,
        [Description("66cccc")]
        downy,
        [Description("77cc33")]
        atlantis,
        [Description("669900")]
        limeade,
        [Description("336600")]
        verdun_green,
        [Description("666600")]
        verdun_green_2,
        [Description("999900")]
        olive,
        [Description("cccc33")]
        earls_green,
        [Description("ffff00")]
        yellow,
        [Description("ffcc33")]
        sunglow,
        [Description("ff9900")]
        orange_peel,
        [Description("ff6600")]
        blaze_orange,
        [Description("cc6633")]
        tuscany,
        [Description("996633")]
        potters_clay,
        [Description("663300")]
        nutmeg_wood_finish,
        [Description("000000")]
        black,
        [Description("999999")]
        dusty_gray,
        [Description("cccccc")]
        silver,
        [Description("ffffff")]
        white,
        [Description("424153")]
        gun_powder,
    }

    public class WallhavenRequestQueryCore
    {
        /// <summary>
        /// 模糊搜索一个tag或者关键字
        /// </summary>
        public string? tagname { set; get; }
        /// <summary>
        /// 不包含某个关键字
        /// </summary>
        public string? excludeTagName { set; get; }
        /// <summary>
        /// 必须包含的tags
        /// </summary>
        public List<string>? addTags { set; get; }
        /// <summary>
        /// 不包含的tags
        /// </summary>
        public List<string>? excludeTags { set; get; }
        /// <summary>
        /// 指定某个用户上传的内容
        /// </summary>
        public string? userName { set; get; }
        /// <summary>
        /// 指定某个具体tag id,不可以拼接
        /// </summary>
        public string? id { set; get; }
        /// <summary>
        /// 指定格式
        /// </summary>
        public string? type { set; get; }
        /// <summary>
        /// 近似搜索(wallpagaerID)
        /// </summary>
        public string? likeID { set; get; }
    }

    public static class ToQueryExtension
    {
        public static string ToQuery(this WallhavenRequestQuery curQuery)
        {
            StringBuilder sber = new StringBuilder();
            sber.Append("?");
            if (!curQuery.catagories.none)
            {
                sber.Append($"categories={(curQuery.catagories.general ? "1" : "0")}{(curQuery.catagories.anime ? "1" : "0")}{(curQuery.catagories.people ? "1" : "0")}");
            }
            if (!curQuery.purity.none)
            {
                sber.Append($"&purity={(curQuery.purity.sfw ? "1" : "0")}{(curQuery.purity.sketchy ? "1" : "0")}{(curQuery.purity.nsfw ? "1" : "0")}");
            }

            sber.Append($"&sorting={Enum.GetName(curQuery.sorting)?.ToLower()}");
            sber.Append($"&order={curQuery.order}");
            if (curQuery.sorting == WallHavenSorting.toplist)
            {
                sber.Append($"&topRange={curQuery.topRange}");
            }
            if (!string.IsNullOrEmpty(curQuery.atleast))
            {
                sber.Append($"&atleast={curQuery.atleast}");
            }
            if (!string.IsNullOrEmpty(curQuery.resolutions))
            {
                sber.Append($"&resolutions={curQuery.resolutions}");
            }
            if (!string.IsNullOrEmpty(curQuery.ratios))
            {
                sber.Append($"&ratios={curQuery.ratios}");
            }


            if (curQuery?.colors != null && curQuery.colors.Count > 0)
            {
                var str = string.Join(" ", curQuery.colors.ToArray());
                sber.Append($"&colors={str}");
            }

            sber.Append($"&page={curQuery?.page}");

            if (!string.IsNullOrEmpty(curQuery?.seed))
                sber.Append($"&seed={curQuery.seed}");

            if (curQuery?.queryCore != null)
            {
                sber.Append("&q=");
                var core = curQuery.queryCore;
                if (!string.IsNullOrEmpty(core.tagname))
                    sber.Append($"{core.tagname} ");
                if (!string.IsNullOrEmpty(core.excludeTagName))
                    sber.Append($"-{core.excludeTagName} ");
                if (core.addTags != null && core.addTags.Count > 0)
                {
                    core.addTags.ForEach(x =>
                    {
                        sber.Append($"+{x} ");
                    });
                }
                if (core.excludeTags != null && core.excludeTags.Count > 0)
                {
                    core.excludeTags.ForEach(x =>
                    {
                        sber.Append($"-{x} ");
                    });
                }
                if (!string.IsNullOrEmpty(core.userName))
                {
                    sber.Append($"@{core.userName} ");
                }
                if (!string.IsNullOrEmpty(core.id))
                {
                    sber.Append($"id:{core.id} ");
                }
                if (!string.IsNullOrEmpty(core.type))
                    sber.Append($"type:{core.type} ");
                if (!string.IsNullOrEmpty(core.likeID))
                    sber.Append($"like:{core.likeID}");
            }

            return sber.ToString();
        }
    }

    public class WallhavenRequestQuery
    {
        public WallhavenRequestQueryCore? queryCore { set; get; }
        public WallHavenCategories catagories { set; get; } = new WallHavenCategories();
        public WallHavenPurity purity { set; get; } = new WallHavenPurity();
        public WallHavenSorting sorting { set; get; } = WallHavenSorting.date_added;
        public string order { set; get; } = "desc";//asc
        public string topRange { set; get; } = "1d";//1d, 3d, 1w,1M*, 3M, 6M, 1y//sorting需要在toplist下生效
        public string atleast { set; get; } = "1920x1080";//miniumresolution
        public string resolutions { set; get; } = "1920x1080,1920x1200";
        public string ratios { set; get; } = "16x9,16x10";
        public List<WallHavenColors>? colors { set; get; }
        public long page { set; get; } = 1;
        public string? seed { set; get; } //[a-zA-Z0-9]{6}

    
    }

    public class WallhavenQuery
    {
        public long id { set; get; }
        public string? tag { set; get; }
    }
    public class WallhavenMeta
    {
        public long current_page { set; get; }
        public long last_page { set; get; }
        public long per_page { set; get; }
        public long total { set; get; }
        public string? query { set; get; }
    }
    public class WallhavenData
    {
        public string? id { set; get; }
        public string? url { set; get; }
        public string? short_url { set; get; }
        public long? views { set; get; }
        public long? favorites { set; get; }
        public string? purity { set; get; }
        public string? category { set; get; }
        public long? dimension_x { set; get; }
        public long? dimension_y { set; get; }
        public string? resolution { set; get; }
        public string? ratio { set; get; }
        public long file_size { set; get; }
        public string? file_type { set; get; }
        public string? created_at { set; get; }
        public List<string?>? colors { set; get; }
        public string? path { set; get; }
    }

    public class WallhavenResponse
    {
        public List<WallhavenData?>? data { set; get; }
        public WallhavenMeta? meta { set; get; }
        public string? seed { set; get; }
    }
}
