using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using Flurl;
using Flurl.Http;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace DeskTopTimer.ChatRoom.ViewModels
{
    public class ChatRoomRequests
    {
        #region UrlConfigure
        private static string SECRET_KEY = "123456";

        private string _baseAddress="";
        private string? _secret_key = SECRET_KEY;
        #endregion

        #region Initilization

        public void InitNetWork(string baseAddress, string? secret_key = null)
        {
            _baseAddress = baseAddress;
            _secret_key = secret_key;
        }
        #endregion

        #region BasicDefination

        public class Response
        {
            public long code { set; get; }
            public string? message { set; get; }

            public bool IsError => code != 0;
        }
        public class Response<T>
        {
            public long code { set; get; }
            public string? message { set; get; }
            public T? result { set; get; }

            public bool IsError => code != 0;
        }


        public enum MessageType
        {
            MESSAGE_CONTENT_TYPE_UNKNOWN = 0,
            MESSAGE_CONTENT_TYPE_TEXT=1,
            MESSAGE_CONTENT_TYPE_SOUND=2,
            MESSAGE_CONTENT_TYPE_IMAGE=3,
            MESSAGE_CONTENT_TYPE_LOCATION=4,
            MESSAGE_CONTENT_TYPE_FILE=5,
            MESSAGE_CONTENT_TYPE_VIDEO=6,
            MESSAGE_CONTENT_TYPE_STICKER=7,
            MESSAGE_CONTENT_TYPE_IMAGETEXT=8,
            MESSAGE_CONTENT_TYPE_RECALL=80,
            MESSAGE_CONTENT_TYPE_TIP=90,
            MESSAGE_CONTENT_TYPE_TYPING=91,
            MESSAGE_CONTENT_TYPE_CREATE_GROUP=104,
            MESSAGE_CONTENT_TYPE_ADD_GROUP_MEMBER=105,
            MESSAGE_CONTENT_TYPE_KICKOF_GROUP_MEMBER=106,
            MESSAGE_CONTENT_TYPE_QUIT_GROUP=107,
            MESSAGE_CONTENT_TYPE_DISMISS_GROUP=108,
            MESSAGE_CONTENT_TYPE_TRANSFER_GROUP_OWNER=109,
            MESSAGE_CONTENT_TYPE_CHANGE_GROUP_NAME = 110,
            MESSAGE_CONTENT_TYPE_MODIFY_GROUP_ALIAS = 111,
            MESSAGE_CONTENT_TYPE_CHANGE_GROUP_PORTRAIT = 112,
            VOIP_CONTENT_TYPE_START = 400,
            VOIP_CONTENT_TYPE_ACCEPT = 401,
            VOIP_CONTENT_TYPE_END = 402,
            VOIP_CONTENT_TYPE_SIGNAL = 403,
            VOIP_CONTENT_TYPE_MODIFY = 404
        }

        public enum ConversationType
        {
            Single=0,
            Group=1,
            Chatroom=2,
            Channel=3,
            SecretChat=5
        }

        public class MessagePayload
        {
            /// <summary>
            /// 消息内容类型<seealso cref="MessageType"/>
            /// </summary>
            public int type { set;get;}
            /// <summary>
            /// 消息可搜索内容
            /// </summary>
            public string? searchableContent { set;get;}
            /// <summary>
            /// 消息推送内容
            /// </summary>
            public string? pushContent { set;get;}
            /// <summary>
            /// 消息推送数据
            /// </summary>
            public string? pushData { set;get;}
            /// <summary>
            /// 消息内容
            /// </summary>
            public string? content { set;get;}
            /// <summary>
            /// 消息二进制内容，base64编码
            /// </summary>
            public string? base64edData { set;get;}
            /// <summary>
            /// 媒体消息类型
            /// </summary>
            public int? mediaType { set;get;}
            /// <summary>
            /// 媒体内容链接
            /// </summary>
            public string? remoteMediaUrl { set;get;}
            /// <summary>
            /// 消息过期时间
            /// </summary>
            public long? expireDuration { set;get;}
            /// <summary>
            /// 消息提醒类型
            /// </summary>
            public int? mentionedType { set;get;}
            /// <summary>
            /// 	消息提醒对象列表
            /// </summary>
            public List<string?>? mentionedTarget { set;get;}
        }

        public class Conversation
        {
            /// <summary>
            /// 会话类型 <seealso cref="ConversationType"/>
            /// </summary>
            public int type { set;get;}
            /// <summary>
            /// 会话目标
            /// </summary>
            public string Target { set;get;} = "";
            /// <summary>
            /// 会话线路，缺省为0
            /// </summary>
            public int? line { set;get;}
        }


        public class UserInfo
        {
            /// <summary>
            /// 用户ID，在创建时可以为空，如果传空，系统会自动生成一个用户id。其它情况必须携带用户id。必须保证唯一性。
            /// </summary>
            public string? userID { set; get; }
            /// <summary>
            /// 帐号名，必须保证唯一性。
            /// </summary>
            public string? name { set; get; }
            /// <summary>
            /// 显示名字
            /// </summary>
            public string? displayName { set; get; }
            /// <summary>
            /// 用户头像
            /// </summary>
            public string? portrait { set; get; }
            /// <summary>
            /// 用户手机号码
            /// </summary>
            public string? mobile { set; get; }
            /// <summary>
            /// 用户邮箱
            /// </summary>
            public string? email { set; get; }
            /// <summary>
            /// 用户地址
            /// </summary>
            public string? address { set; get; }
            /// <summary>
            /// 用户公司
            /// </summary>
            public string? company { set; get; }
            /// <summary>
            /// 社交信息
            /// </summary>
            public string? social { set; get; }
            /// <summary>
            /// 附加信息
            /// </summary>
            public string? extra { set; get; }
        }

        public class GroupMember
        {
            /// <summary>
            /// 群成员的用户ID
            /// </summary>
            public string member_id { set;get;}="";
            /// <summary>
            /// 群成员的群名片
            /// </summary>
            public string? alias { set;get;}
            /// <summary>
            /// 群成员类型，0 普通成员, 1 管理员, 2 群主， 3 禁言，4 已经移除的成员，当修改群成员信息时，只能取值0/1，其他值由其他接口实现，暂不支持3
            /// </summary>
            public int? type { set;get;}
        }

        public class Group
        {
            /// <summary>
            /// 群组ID
            /// </summary>
            public string group_info { set;get;} = "";
            /// <summary>
            /// 	群组成员列表
            /// </summary>
            public List<string?>? members { set;get;}
        }

        public class GroupInfo
        {
            /// <summary>
            /// 群组ID，创建群组时为可选参数，获取群组信息时是必填项
            /// </summary>
            public string? target_id { set;get;}
            /// <summary>
            /// 群组名称
            /// </summary>
            public string? name { set;get;}
            /// <summary>
            /// 群组头像
            /// </summary>
            public string? portrait { set;get;}
            /// <summary>
            /// 群主用户ID
            /// </summary>
            public string owner { set;get;}="";
            /// <summary>
            /// 群类型，0 weixin 风格群组；2 qq 风格群组。移动端demo使用的是2，建议使用2.
            /// </summary>
            public int type { set;get;}=2;
            /// <summary>
            /// 群的extra信息供客户扩展使用
            /// </summary>
            public string? extra { set;get;}
            /// <summary>
            /// 是否全员禁言，0 不禁言；1 全员禁言。
            /// </summary>
            public int? mute { set;get;}
            /// <summary>
            /// 加入群权限，0 所有人可以加入；1 群成员可以拉人；2 群管理员或群组可以拉人
            /// </summary>
            public int? join_type { set;get;}
            /// <summary>
            /// 是否禁止私聊，0 允许群成员发起私聊；1 不允许群成员发起私聊。
            /// </summary>
            public int? private_chat { set;get;}
            /// <summary>
            /// 是否允许查看群成员查看加入群之前的历史消息，0 不允许；1 是允许。
            /// </summary>
            public int? history_message { set;get;}
        }

        #endregion

        #region BasicRequestMethods

        public IFlurlRequest WithRequestHeader(string url)
        {
          
            var randomNumer = new Random().Next(20000);
            
            var TimeStamp = (DateTime.Now.ToLocalTime() - new DateTime(1970,1,1,0,0,0)).TotalMilliseconds;

            var shaSign = SHA1.Create(randomNumer+"|"+ _secret_key + "|"+TimeStamp)?.ToString();

            var curRequestUrl = (_baseAddress + "\\" + url)
                .WithHeader("nonce",randomNumer)
                .WithHeader("timestamp",TimeStamp)
                .WithHeader("sign",shaSign)
                .WithHeader("Content-Type", "application/json; charset=utf-8");

            return curRequestUrl;
        }

        public async Task<Response?> PostJsonRequest(IFlurlRequest request, string json, CancellationTokenSource? cancellation = null)
        {
            var res = await request.PostJsonAsync(json, cancellation == null ? default(CancellationToken) : cancellation.Token);
            if (res == null)
            {
                return null;
            }
            return await res.GetJsonAsync<Response?>();
        }

        public async Task<Response<T>?> PostJsonRequest<T>(IFlurlRequest request,string json, CancellationTokenSource? cancellation = null)
        {
            var res = await request.PostJsonAsync(json, cancellation == null ? default(CancellationToken) : cancellation.Token);
            if (res == null)
            {
                return null;
            }
            return await res.GetJsonAsync<Response<T>?>();
        }

        #endregion

        #region User

        public class ChatUserToken
        {
            public string? userId { set;get;}
            public string? token { set;get;}
        }
        /// <summary>
        /// 获取用户Token
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="ClientID"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<Response<ChatUserToken>?> User_GetUserToken(string UserID,string ClientID,CancellationTokenSource? cancellation = null)
        {
            var request = WithRequestHeader("admin/user/get_token");
            var JOb = new JObject();
            JOb.Add("userId",UserID);
            JOb.Add("clientId",ClientID);
            JOb.Add("platform",3);
            return await PostJsonRequest<ChatUserToken>(request,JOb.ToString(),cancellation);
        }
        /// <summary>
        /// 注册/更新用户
        /// </summary>
        /// <param name="name"></param>
        /// <param name="disPlayName"></param>
        /// <param name="UserId"></param>
        /// <param name="portrait"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<Response<string>?> User_CreateAccount(string name,string disPlayName,string UserId="",string portrait="",CancellationTokenSource? cancellation = null)
        {
            var request = WithRequestHeader("admin/user/create");
            JObject JOb = new JObject();
            JOb.Add("userId",UserId);
            JOb.Add("name",name);
            JOb.Add("displayName",disPlayName);
            JOb.Add("portrait",portrait);
            return await PostJsonRequest<string>(request, JOb.ToString(), cancellation);
        }
        [Flags]
        public enum UserInfoUpdateFlag
        {
            userNickName=1,
            userPortait= 2,
            userSex = 4,
            userTelephone=8,
            userEmail=16,
            userAddress=32,
            userCompany=64,
            userSocial=128,
            userExtra=256
        }
        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="updateFlag"></param>
        /// <param name="updateInfo"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<Response?> User_UpdateUserInfo(UserInfoUpdateFlag updateFlag, UserInfo updateInfo, CancellationTokenSource? cancellation = null)
        {
            var request = WithRequestHeader("admin/user/update");
            JObject JOb = JObject.FromObject(updateInfo);
            return await PostJsonRequest(request, JOb.ToString(), cancellation);
        }
        /// <summary>
        /// 获取用户信息(三个参数必须且只能存在一个)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="mobile"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<Response<UserInfo>?> User_GetUserInfo(string userId = "",string name="",string mobile="", CancellationTokenSource? cancellation = null)
        {
            var request = WithRequestHeader("admin/user/get_info");
            JObject JOb = new JObject();
            if(!string.IsNullOrEmpty(userId))
                JOb.Add("userId",userId);
            else if(!string.IsNullOrEmpty(name))
                JOb.Add("name",name);
            else if(!string.IsNullOrEmpty(mobile))
                JOb.Add("mobile",name);
            else
                return null;
            return await PostJsonRequest<UserInfo>(request, JOb.ToString(), cancellation);
        }

        public class Session
        {
            /// <summary>
            /// 客户端ID
            /// </summary>
            public string clientId { set;get;}="";
            /// <summary>
            /// UserId
            /// </summary>
            public string userId { set;get;}="";
            /// <summary>
            /// 平台
            /// </summary>
            public int platform { set;get;}
            /// <summary>
            /// 0 online, 1 have session offline
            /// </summary>
            public int status { set;get;}
            /// <summary>
            /// 最后一次可见时间
            /// </summary>
            public long lastSeen { set;get;}
        }

        public async Task<Response<List<Session>>?> User_GetOnlineStatus(string userId, CancellationTokenSource? cancellation = null)
        {
            var request = WithRequestHeader("admin/user/onlinestatus");
            JObject JOb = new JObject();
            JOb.Add("userId",userId);
            return await PostJsonRequest<List<Session>>(request, JOb.ToString(), cancellation);
        }

        #endregion

        #region Friends
        /// <summary>
        /// 设置用户好友关系
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="targetId"></param>
        /// <param name="AreWeFirends"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<Response?> Friends_SetFriends(string userId, string targetId, bool AreWeFirends, CancellationTokenSource? cancellation = null)
        {
            var request = WithRequestHeader("admin/friend/status");
            JObject JOb = new JObject();
            JOb.Add("userId",userId);
            JOb.Add("friendUid",targetId);
            JOb.Add("status",AreWeFirends?0:1);
            return await PostJsonRequest(request,JOb.ToString(),cancellation);
        }

        
        #endregion
    }
}
