using FFmpeg.AutoGen;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace DeskTopTimer
{

    public static class ExtensionsMethods
    {
        #region StringRelated
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string obj)
        {
            return string.IsNullOrEmpty(obj);
        }
        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFileExist(this string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }
        /// <summary>
        /// 读入指定的文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? ReadText(this string path)
        {
            if (path.IsFileExist())
            {
                return FileStrReader.Read(path,Encoding.UTF8);
            }
            return string.Empty;
        }
        /// <summary>
        /// 写入指定内容
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="content"></param>
        public static void WriteText(this string Path, string content)
        {
            using (var Stream = File.Open(Path, FileMode.OpenOrCreate))
            {
                var curBytes = Encoding.UTF8.GetBytes(content);
                Stream.Write(curBytes, 0, curBytes.Length);
            }
        }
        /// <summary>
        /// 序列化到json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string? ToJson<T>(this T obj) where T : class
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// json反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T? DeSerialize<T>(this string str) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
                return null;
            }
        }
        /// <summary>
        /// 文件夹是否存在
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="shouldCreateIfNotExsit"></param>
        /// <returns></returns>
        public static bool IsDirctoryExist(this string Path, bool shouldCreateIfNotExsit = false)
        {
            bool res = Directory.Exists(Path);
            if (shouldCreateIfNotExsit)
                Directory.CreateDirectory(Path);
            return shouldCreateIfNotExsit ? true : res;
        }
        /// <summary>
        /// 路径拼接
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="combinedPath"></param>
        /// <returns></returns>
        public static string PathCombine(this string srcPath, string combinedPath)
        {
            return Path.Combine(srcPath, combinedPath);
        }
        /// <summary>
        /// 获取文件名称
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getName(this string path)
        {
            return Path.GetFileName(path);
        }
        /// <summary>
        /// 获取拓展名
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string getExtension(this string ex)
        {
            return Path.GetExtension(ex);
        }
        /// <summary>
        /// 获取不带拓展名的文件名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string getNameWithOutEx(this string name)
        {
            return Path.GetFileNameWithoutExtension(name);
        }

        /// <summary>
        /// 集合打印
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="Splitter"></param>
        /// <returns></returns>
        public static string SourcesToPrintString<T>(this IEnumerable<T> objs, char Splitter = ' ')
        {
            StringBuilder builder = new StringBuilder();
            foreach (var obj in objs)
            {
                builder.Append(obj?.ToString());
                builder.Append(Splitter);
            }
            builder.Remove(builder.Length - 1, 1);//remove last
            return builder.ToString();
        }
        /// <summary>
        /// sha转换
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string? SHA1(this string obj)
        {
            if (obj.IsNullOrEmpty())
            {
                return null;
            }
            byte[] cleanBytes = Encoding.Default.GetBytes(obj);
            byte[] hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");

        }

        /// <summary>
        /// md5转换
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string? MD5(this string obj)
        {
            if (obj.IsNullOrEmpty())
            {
                return null;
            }
            byte[] cleanBytes = Encoding.Default.GetBytes(obj);
            byte[] hashedBytes = System.Security.Cryptography.MD5.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");

        }
        /// <summary>
        /// 是否包含中文
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool ContainChinese(this string input)
        {
            string pattern = "[\u4e00-\u9fbb]";
            return Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// 判断是否为gif
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsGif(this string source)
        {
            if(!source.IsFileExist())
                return false;
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var buffer = new byte[4];
            using(Stream stream = File.OpenRead(source))
            {
                stream.Read(buffer, 0, buffer.Length);
                var res= gif.SequenceEqual(buffer.Take(gif.Length));
                stream.Close();
                return res;
            }

        }
        #endregion


        #region AttachedProperty
        /// <summary>
        /// 对象hash,对象弱引用,实际Key,实际value
        /// 对象hash解决初步索引问题，对象弱引用解决hash相同时问题
        /// </summary>
        private static ConcurrentDictionary<int, ConcurrentDictionary<CompareableWeakReference, ConcurrentDictionary<string, object?>>> AttachedRecoreds = new ConcurrentDictionary<int, ConcurrentDictionary<CompareableWeakReference, ConcurrentDictionary<string, object?>>>();

        /// <summary>
        /// 设置一个附加属性
        /// </summary>
        /// <typeparam name="T">对象值类型</typeparam>
        /// <typeparam name="P">附加值类型</typeparam>
        /// <param name="Key">存储的关键key，获取属性需要key相同</param>
        /// <param name="obj">需要附加的对象</param>
        /// <param name="property">存入字典的具体值</param>
        public static bool SetExProperty<T, P>(this T obj, string Key, P property) where T : class
        {
            if(obj==null)
                return false;
            var HashCode = obj?.GetHashCode();
            if (HashCode == null)
                return false;
            if (!AttachedRecoreds.ContainsKey(HashCode.Value))
            {
                if (!AttachedRecoreds.TryAdd(HashCode.Value, new ConcurrentDictionary<CompareableWeakReference, ConcurrentDictionary<string, object?>>()))
                {
                    Trace.WriteLine($"Write ExProperty faild [Src:{obj?.GetType()}][Key:{Key}][Value:{property?.GetType()}]");
                    return false;
                }
            }


            if (AttachedRecoreds[HashCode.Value].Count <= 0)
            {
                if (!AttachedRecoreds[HashCode.Value].TryAdd(new CompareableWeakReference(obj), new ConcurrentDictionary<string, object?>()))
                {
                    Trace.WriteLine($"Write ExProperty faild [Src:{obj?.GetType()}][Key:{Key}][Value:{property?.GetType()}]");
                    return false;
                }
            }

            var storeKeyPairs = AttachedRecoreds[HashCode.Value].FirstOrDefault(o => o.Key.GetHashCode() == obj?.GetHashCode());

            if (storeKeyPairs.Value == null)
                return false;

            if (storeKeyPairs.Value.ContainsKey(Key))
            {
                storeKeyPairs.Value[Key] = property;
            }
            else
            {
                if (!storeKeyPairs.Value.TryAdd(Key, property))
                {
                    Trace.WriteLine($"Write ExProperty faild [Src:{obj?.GetType()}][Key:{Key}][Value:{property?.GetType()}]");
                    return false;
                }
            }
            return true;

        }

        /// <summary>
        /// 获取一个附加属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="obj"></param>
        /// <param name="Property"></param>
        /// <returns></returns>
        public static bool TryGetExProperty<T, P>(this T obj, string Key, out P? Property) where T : class
        {
            Property = default(P);
            if(obj==null)
                return false;
            var HashCode = obj?.GetHashCode();
            if (HashCode == null)
                return false;
            if (!AttachedRecoreds.ContainsKey(HashCode.Value))
                return false;
            var Target = AttachedRecoreds[HashCode.Value].FirstOrDefault(x => x.Key.GetHashCode() == obj?.GetHashCode());

            if (Target.Key == null || Target.Value == null || !(Target.Value).ContainsKey(Key))
                return false;

            if (Target.Value.TryGetValue(Key, out object? data))
            {
                var con = TypeDescriptor.GetConverter(typeof(P));
                Property = (P?)con.ConvertTo(data, typeof(P));
                return true;
            }
            return false;
        }
        #endregion


        #region MarkRelated
        private static ConcurrentDictionary<object, int> _markRecords = new ConcurrentDictionary<object, int>();
        public static bool Mark<T>(this T obj) where T : class
        {
            if (!_markRecords.ContainsKey(obj))
                if (!_markRecords.TryAdd(obj, 0))
                {
                    Trace.WriteLine($"Add Key [{obj.GetType().ToString()}] failed");
                    return false;
                }
            ++_markRecords[obj];
            return true;
        }

        public static bool IsMarked<T>(this T obj) where T : class
        {
            return _markRecords.ContainsKey(obj) && _markRecords[obj] > 0;
        }

        public static int GetMarkCount<T>(this T obj) where T : class
        {
            if (!_markRecords.ContainsKey(obj))
                return -1;
            return _markRecords[obj];
        }

        public static void RemoveMarks<T>(this T obj, int removeCount = -1) where T : class
        {
            if (!_markRecords.ContainsKey(obj))
                return;
            if (removeCount == -1)
                _markRecords[obj] = 0;
            else
            {
                _markRecords[obj] -= removeCount;
            }
        }
        #endregion

        #region Actions
        /// <summary>
        /// 通过通过bool值真假执行下一步动作
        /// </summary>
        /// <param name="status"></param>
        /// <param name="ifTrue"></param>
        /// <param name="ifNot"></param>
        public static T IfDo<T>(this T status, Predicate<T> Predicate, Action<T>? ifTrue = null) where T : class
        {
            if (Predicate(status))
            {
                ifTrue?.Invoke(status);
            }
            return status;
        }

        /// <summary>
        /// 对于集合内的对象，为满足条件的执行动作，否则将其返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcs"></param>
        /// <param name="predicate"></param>
        /// <param name="ifTrue"></param>
        /// <returns></returns>
        public static IEnumerable<T> IfDoElseBack<T>(this IEnumerable<T> srcs, Predicate<T> predicate, Action<T>? ifTrue = null) where T : class
        {
            List<T> restOf = new List<T>();
            srcs?.IfDo(p => p != null, p =>
            {
                foreach (var o in p)
                {
                    (predicate(o) ? ifTrue : restOf.Add)?.Invoke(o);//(o);//取消直接调用避免空方法错误
                }
            });
            return restOf;
        }

        /// <summary>
        /// 可被break的foreach
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcs"></param>
        /// <param name="breakWhile"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> ForeachBreakable<T>(this IEnumerable<T> srcs, Predicate<T> breakWhile, Action<T>? action = null) where T : class
        {
            return srcs?.IfDo(o => o != null, o =>
            {
                foreach (var itr in o)
                {
                    if (breakWhile(itr))
                        break;
                    action?.Invoke(itr);
                }
            });
        }



        /// <summary>
        /// 获取一个包装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="src"></param>
        /// <param name="src2"></param>
        /// <returns></returns>
        public static Tuple<T, V> GetTupleWith<T, V>(this T src, V src2) where T : class where V : class
        {
            return new Tuple<T, V>(src, src2);
        }

        /// <summary>
        /// 获取对象的浅拷贝(同名属性拷贝，或指定属性CopyedPropertyAttribute)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dst"></param>
        /// <param name="src"></param> 
        /// <returns></returns>
        public static T GetMemberwiseCopy<T>(this T dst) where T : class
        {
            return MemberCopy.TransExp<T, T>(dst);
        }

        /// <summary>
        /// 参数传递调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="arg"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static TResult Pipe<T, TResult>(this T arg, Func<T, TResult> method)
            => method.Invoke(arg);
        #endregion

        #region DelayTask

        /// <summary>
        /// 超时异步方法
        /// </summary>
        /// <param name="proc">执行的方法</param>
        /// <param name="seconds">超时时长</param>
        /// <remarks>适用于不需要返回值的方法</remarks>
        /// <returns>是否超时</returns>
        public static async Task<bool> DelayTask(this Action proc, int millSecond)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())//考虑延时任务应该可以被提前取消
            {
                var delayTask = Task.Delay(millSecond, timeoutCancellationTokenSource.Token);
                if (await Task.WhenAny(Task.Run(proc), delayTask) == delayTask)
                {
                    return true;
                }
                timeoutCancellationTokenSource.Cancel();
                return false;
            }
        }

        /// <summary>
        /// 超时异步方法
        /// </summary>
        /// <param name="proc">执行的方法</param>
        /// <param name="seconds">超时时长</param>
        /// <returns>是否超时</returns>
        public static async Task<T> DelayTask<T>(this Func<T> proc, int millSecond)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())//考虑延时任务应该可以被提前取消
            {
                var delayTask = Task.Delay(millSecond, timeoutCancellationTokenSource.Token);
                var workTask = Task.Run(proc);
                if (await Task.WhenAny(workTask, delayTask) == delayTask)
                {
                    return await workTask;
                }
                timeoutCancellationTokenSource.Cancel();
                return default(T);
            }
        }

        #endregion

        #region SingleThreadTask

        private static ConcurrentDictionary<string, ConcurrentQueue<Action>> refTasks = new ConcurrentDictionary<string, ConcurrentQueue<Action>>();
        private static ConcurrentDictionary<string, CancellationTokenSource> taskMarks = new ConcurrentDictionary<string, CancellationTokenSource>();
        private static async void StartTask(string key)
        {
            if (!refTasks.ContainsKey(key))
                return;
            if (!taskMarks.ContainsKey(key) || taskMarks[key] == null)
            {
                taskMarks[key] = new CancellationTokenSource();
            }
            await Task.Run(() =>
            {
                var curTask = refTasks[key];
                var curToken = taskMarks[key];
                try
                {
                    while (curTask.Count <= 0 && !curToken.IsCancellationRequested)
                    {
                        if (curTask.TryDequeue(out var curAction))
                        {
                            curAction?.Invoke();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    taskMarks[key] = null;
                }

            });
        }
        /// <summary>
        /// 同类方法集中，单线程统一调度执行
        /// </summary>
        /// <param name="proc">要执行的方法</param>
        /// <param name="taskKey">分组key</param>
        /// <param name="reftoken">外部提前终止量</param>
        /// <returns></returns>
        public static void StartActionTask(this Action proc, string taskKey, CancellationTokenSource reftoken = null)
        {
            try
            {
                if (!refTasks.ContainsKey(taskKey))
                {
                    refTasks[taskKey] = new ConcurrentQueue<Action>();
                }
                refTasks[taskKey].Enqueue(proc);
                if (taskMarks.ContainsKey(taskKey) && taskMarks[taskKey] != null)//not null means task with taskKey not fished,just add one and exit
                    return;
                else//build a cancellationTokenSource if refToken is null
                {
                    if (reftoken != null)
                    {
                        taskMarks[taskKey] = reftoken;
                    }
                    else
                    {
                        taskMarks[taskKey] = new CancellationTokenSource();
                    }
                    StartTask(taskKey);
                }
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        #endregion



        #region Parallel
        /// <summary>
        /// 取得并行化序列
        /// </summary>
        /// <param name="act"></param>
        /// <param name="moreActs"></param>
        /// <returns></returns>
        public static ParallelQuery<T>? WithParllel<T>(this T act, params T[] moreActs) where T : class
        {
            if (act == null)
                return null;
            var actList = new List<T>();
            actList.Add(act);
            if (moreActs != null)
            {
                foreach (var itr in moreActs)
                {
                    actList.Add(itr);
                }
            }
            return actList.AsParallel();
        }
        /// <summary>
        /// 为并行化序列增加数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="act"></param>
        /// <param name="moreActs"></param>
        /// <returns></returns>
        public static ParallelQuery<T>? WithParllel<T>(this ParallelQuery<T> act, params T[] moreActs) where T : class
        {
            if (act == null)
                return null;
            var actList = act.ToList();
            if (moreActs != null)
            {
                foreach (var itr in moreActs)
                {
                    actList.Add(itr);
                }
            }
            return actList.AsParallel();
        }
        #endregion

        #region DateTime

        public static DateTime GetNowTime()
        {
            string ID = TimeZoneInfo.Local.Id;
            DateTime NowTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(ID));
            return NowTime;
        }
        /// <summary>
        /// 时间转时间戳
        /// </summary>
        /// <param name="_dataTime">时间</param>
        /// <param name="MilliTime">毫秒计时</param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime _dataTime, bool Millisecond = true)
        {
            string ID = TimeZoneInfo.Local.Id;
            DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            DateTime startTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(ID));
            DateTime NowTime = TimeZoneInfo.ConvertTime(_dataTime, TimeZoneInfo.FindSystemTimeZoneById(ID));
            long timeStamp;
            if (Millisecond)
                timeStamp = (long)(NowTime - startTime).TotalMilliseconds; // 相差毫秒数
            else
                timeStamp = (long)(NowTime - startTime).TotalSeconds; // 相差秒数
            return timeStamp;
        }
        public static DateTime ToDateTime(this long stamp, bool Millisecond = true)
        {
            string ID = TimeZoneInfo.Local.Id;
            DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            DateTime startTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(ID));
            DateTime dt;
            if (Millisecond)
                dt = startTime.AddMilliseconds(stamp);
            else
                dt = startTime.AddSeconds(stamp);

            return dt;
        }

        #endregion
    }

    public class CompareableWeakReference
    {
        private int _hashCode;

        private WeakReference? innerData;

        public CompareableWeakReference(object? obj)
        {
            if (obj != null)
            {
                innerData = new WeakReference(obj);
                _hashCode = obj.GetHashCode();
            }
            else
            {
                _hashCode = -1;
            }
        }

        public object? GetTarget()
        {
            return innerData?.Target;
        }

        public bool IsAlive
        {
            get => innerData==null?false:innerData.IsAlive;
        }

        public override bool Equals(object? obj)
        {
            return (obj as CompareableWeakReference)?._hashCode == _hashCode;
        }


        public override int GetHashCode()
        {
            return _hashCode;
        }

    }

    public static class MemberCopy
    {

        private static Dictionary<string, object> _CacheDic = new Dictionary<string, object>();

        /// <summary>
        /// 将一个类对象复制为另一个具有同名字段的类对象,建议搭配CopyedPropertyAttribute使用(无指定转换名称时，按属性名称匹配)
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="tIn"></param>
        /// <returns></returns>
        public static TOut TransExp<TIn, TOut>(TIn tIn)
            where TIn : class
            where TOut : class
        {
            //缓存一个对象转换为另一个对象的关键字，若已经存在则不用表达式树重新编译
            string key = string.Format("trans_exp_{0}_{1}", typeof(TIn).FullName, typeof(TOut).FullName);
            if (!_CacheDic.ContainsKey(key))
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
                List<MemberBinding> memberBindingList = new List<MemberBinding>();

                foreach (var item in typeof(TOut).GetProperties())
                {
                    MemberExpression? property = null;
                    if (!item.IsDefined(typeof(CopyedPropertyAttribute), false))
                    {
                        var curProperty = typeof(TIn).GetProperty(item.Name);
                        if (curProperty == null)
                        {
                            Trace.WriteLine($"CopyProperty ignore{item.Name}");
                            continue;
                        }

                        property = Expression.Property(parameterExpression, curProperty);
                    }
                    else
                    {
                        var curProperty = item.GetCustomAttributes(typeof(CopyedPropertyAttribute), false).FirstOrDefault();
                        var IsCopyByDefault = (curProperty?.GetType()?.GetProperty("IsDefaultCopyMode")?.GetValue(curProperty)) as bool?;
                        var TargetName = curProperty?.GetType().GetProperty("TargetName")?.GetValue(curProperty) as string;
                        var SourceName = curProperty?.GetType().GetProperty("SourceName")?.GetValue(curProperty) as string;
                        if(SourceName==null)
                        {
                            Trace.WriteLine($"With CopyedPropertyAttribute try get sourceName failed");
                            continue;
                        }
                        var mem = IsCopyByDefault == true && !string.IsNullOrEmpty(SourceName) ? typeof(TIn).GetProperty(item.Name) : typeof(TIn).GetProperty(SourceName);
                        if (mem == null)
                        {
                            Trace.WriteLine($"[{typeof(TIn).FullName}=>{typeof(TOut).FullName} ]CopyProperty ignore [{item.Name}]");
                            continue;
                        }

                        property = Expression.Property(parameterExpression, mem);

                    }

                    if (property == null || item.PropertyType != property.Type)
                    {

                        //Expression.Convert
                        Trace.WriteLine($"[{typeof(TIn).FullName}=>{typeof(TOut).FullName} ]CopyProperty ignore [{item.Name}]");
                        //TODO:if we can trans some type
                        continue;//Ignore
                    }

                    MemberBinding memberBinding = Expression.Bind(item, property);
                    memberBindingList.Add(memberBinding);
                }

                MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
                Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new ParameterExpression[] { parameterExpression });
                Func<TIn, TOut> func = lambda.Compile();
                _CacheDic[key] = func;
            }
            return ((Func<TIn, TOut>)_CacheDic[key])(tIn);
        }

    }

    public static class FileStrReader
    {
        #region Public Methods
        /// <summary>
        /// 读取文件中的字符串
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string? Read(string filePath,Encoding encodingType)
        {
            string? str = null;
            filePath.IfDo(o => o.IsFileExist(), o => 
            {
                try
                {
                    str = File.ReadAllText(filePath, encodingType);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[{DateTime.Now.ToLocalTime()}]{ex}");
                }
            });
            return str;
        }
        #endregion
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CopyedPropertyAttribute : Attribute
    {
        private string targetName = string.Empty;
        /// <summary>
        /// 目标名称
        /// </summary>
        public string TargetName
        {
            get => targetName;
            set
            {
                targetName = value;
            }
        }
        private string sourceName = string.Empty;
        /// <summary>
        /// 源名称
        /// </summary>
        public string SourceName
        {
            get => sourceName;
            set => sourceName = value;
        }
        /// <summary>
        /// 是否为默认拷贝模式
        /// </summary>
        public bool IsDefaultCopyMode
        {
            get => TargetName == SourceName;
        }

        public CopyedPropertyAttribute(string TargetName = "", string SourceName = "")
        {
            this.targetName = TargetName;
            this.sourceName = SourceName;
        }

    }
    public static class ImageHelper
    {
        static ImageHelper()
        {
            lock (typeof(ImageHelper))
            {
                _mapping = GetImageFormatMapping();
            }
        }
        private static IDictionary<Guid, String> _mapping;
        private static IDictionary<Guid, String> GetImageFormatMapping()
        {
            var dic = new Dictionary<Guid, String>();
            var properties = typeof(ImageFormat).GetProperties(
                BindingFlags.Static | BindingFlags.Public
            );
            foreach (var property in properties)
            {
                var format = property.GetValue(null, null) as ImageFormat;
                if (format == null) continue;
                dic[format.Guid] = "." + property.Name.ToLower();
            }
            return dic;
        }

        public static bool IsImageExtension(this string path)
        {
            try
            {
                string extension = Path.GetExtension(path).ToLower();
                if (_mapping.Values.Contains(extension))
                {
                    return true;
                }
                if (extension == ".jpg")
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string GetImageExtension(this string path)
        {
            Image img = null;
            try
            {
                if (!path.IsFileExist())
                    return string.Empty;
                img = Image.FromFile(path);
                var format = img.RawFormat;
                if (_mapping.ContainsKey(format.Guid))
                {
                    return _mapping[format.Guid];
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return string.Empty;
            }
            finally
            {
                img?.Dispose();
            }
        }
    }
}
