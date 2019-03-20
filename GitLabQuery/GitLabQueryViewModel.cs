using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using GitLabApiClient;
using GitLabApiClient.Models.MileStones.Request;
using GitLabApiClient.Models.MileStones.Responses;
using GitLabQuery.Annotations;
using GitLabQuery.Models;

namespace GitLabQuery
{
    public sealed class GitLabQueryViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// milestone 标签列表
        /// </summary>
        public readonly ObservableCollection<MileStoneInfo> MileStones = new ObservableCollection<MileStoneInfo>();

        /// <summary>
        /// 提交的详细信息
        /// </summary>
        private IList<MileStoneMergedInfo> _mergedDetailInfos = new List<MileStoneMergedInfo>();

        /// <summary>
        /// 提交的摘要信息
        /// </summary>
        public readonly ObservableCollection<string> MergedInfos = new ObservableCollection<string>();

        /// <summary>
        /// 查询的milestoneID
        /// </summary>
        public string MileStoneId { get; set; }

        /// <summary>
        /// 提交的状态
        /// </summary>
        public string MergeState { get; set; }

        /// <summary>
        /// Key Label
        /// Vaule 合并信息
        /// 用以按模块划分导出Excel时的数据模型
        /// </summary>
        private readonly Dictionary<string, List<MileStoneMergedInfo>> _mergedDetailInfosOrderByLabelDic =
            new Dictionary<string, List<MileStoneMergedInfo>>();

        public GitLabQueryViewModel()
        {
            GetMileStones();      
        }

        /// <summary>
        /// MileStone的状态，该值改变时，会获取该状态下的MileStone列表
        /// </summary>
        public MileStoneState MileStoneState
        {
            get => _mileStoneState;
            set
            {
                _mileStoneState = value;
                GetMileStones();
                OnPropertyChanged(nameof(MileStoneState));
            }
        }

        /// <summary>
        /// 获取提交代码的信息
        /// </summary>
        /// <returns></returns>
        public async Task GetCommitInfos()
        {
            MergedInfos.Clear();
            if (string.IsNullOrEmpty(MileStoneId))
            {
                MergedInfos.Add("请选择查询的MileStone");
                return;
            }
            MergedInfos.Add("正在查询中，请稍等...");
            await Task.Run(async () =>
            {
                //获取合并信息
                var mergedInfos = await GitLabQueryProvider.Query.MileStone.GetMileStoneMergedInfos(
                    GitLabQueryProvider.EasiNoteProjectId,
                    MileStoneId);
                //给当前状态赋值
                _currentQueriedMilestoneId = MileStoneId;
                _currentQueriedMergeState = MergeState;
                //对提交的状态进行筛选
                mergedInfos = mergedInfos.Where(s => s.State.Equals(MergeState)).ToList();
                //给导出的数据结构模型赋值
                _mergedDetailInfos = mergedInfos;
                SetMergedInfoDic(mergedInfos);
                //界面显示
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MergedInfos.Clear();
                    int index = 1;
                    MergedInfos.Add($"提交总数：{mergedInfos.Count}");
                    foreach (var info in mergedInfos)
                    {
                        MergedInfos.Add($"{index} {info.Opend.Name}:  {info.Title}");
                        index++;
                    }
                });
            });
        }

        public async Task ExportMergedInfos()
        {
            await EnSureStatues("正在导出，请稍等...");

            //导出操作
            ExportExcelByLabel();

            //导出完成
            MergedInfos.Clear();
            MergedInfos.Add($"导出完成，路径：{ExcelHelper.FilePath}");
            System.Diagnostics.Process.Start("Explorer.exe",ExcelHelper.FilePath);
        }

        private async Task EnSureStatues(string hint)
        {
            if (string.IsNullOrEmpty(MileStoneId))
            {
                return;
            }

            if (!string.Equals(MileStoneId, _currentQueriedMilestoneId) || !string.Equals(MergeState, _currentQueriedMergeState))
            {
                await GetCommitInfos();
            }

            MergedInfos.Clear();
            MergedInfos.Add(hint);
        }

        private async Task GetMileStones()
        {
            await Task.Run(async () =>
            {
                var milestones = await GitLabQueryProvider.Query.MileStone.GetMileStonesInfo(
                    GitLabQueryProvider.EasiNoteProjectId,
                    new MileStonesQueryOptions
                    {
                        State = MileStoneState
                    });
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MileStones.Clear();
                    foreach (var info in milestones)
                    {
                        MileStones.Add(info);
                    }
                });
            });
        }

        /// <summary>
        /// 导出excel，这里是重点
        /// </summary>
        private void ExportExcelByLabel()
        {
            ExcelHelper.DeleteFile();
            ExcelHelper.Write("序号,变动点,开发者,审查者,详细说明,影响模块,是否已自测,备注,测试人");

            int count = 1;
            foreach (string key in _mergedDetailInfosOrderByLabelDic.Keys)
            {
                var mergedInfos = _mergedDetailInfosOrderByLabelDic[key];
                foreach (var info in mergedInfos)
                {
                    //是否按模板填写
                    //不按模板填写的忽略
                    if (string.IsNullOrWhiteSpace(info.Description) 
                        || info.Description.LastIndexOf(CustomText.InfluenceFunction, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        continue;
                    }

                    //序号
                    int index = count++;

                    //变动点
                    string change = GetChange(info);

                    //开发者
                    string kaifa = GetKaiFa(info);

                    //审查者
                    string shencha = GetShenCha(info);

                    //详细说明
                    string detail = GetDetail(info);

                    //影响模块
                    string influence = GetInfluence(info);

                    //是否以自测
                    string zice = "";

                    //备注
                    string beizhu = "";

                    //测试人
                    string ceshiren = "";

                    string result = $"{index},{change},{kaifa},{shencha},{detail},{influence},{zice},{beizhu},{ceshiren}";

                    ExcelHelper.Write(result);
                }
            }
        }

        private string GetChange(MileStoneMergedInfo info)
        {
            return Get(info, 2);
        }

        private string GetKaiFa(MileStoneMergedInfo info)
        {
            return info.Opend.Name;
        }

        private string GetShenCha(MileStoneMergedInfo info)
        {
            return info.MergeBy.Name;
        }

        private string GetDetail(MileStoneMergedInfo info)
        {
            return Get(info, 4);
        }

        private string GetInfluence(MileStoneMergedInfo info)
        {
            string content = "";
            if (info.Labels.Count > 0)
            {
                content = "模块：";
                foreach (string label in info.Labels)
                {
                    content += "【" + label + "】";
                }
            }
            content += " 功能："+ Get(info, 6);
            return content;
        }

        private string Get(MileStoneMergedInfo info, int index)
        {
            string content = "";
            if (index > 6)
            {
                return content;
            }

            if (!string.IsNullOrWhiteSpace(info.Description) && info.Description.LastIndexOf(CustomText.InfluenceFunction, StringComparison.OrdinalIgnoreCase) != -1)
            {
                string[] contentlist = info.Description.Split(new string[] { "<<<", ">>>" }, StringSplitOptions.None);
                if (contentlist.Length >= 7)
                {
                    content += contentlist[index].Format();
                }
            }
            return content;
        }

        private void SetMergedInfoDic(IList<MileStoneMergedInfo> mergedInfos)
        {
            _mergedDetailInfosOrderByLabelDic.Clear();
            _mergedDetailInfosOrderByLabelDic.Add(NoLabel, new List<MileStoneMergedInfo>());
            foreach (var info in mergedInfos)
            {
                IgnoreSomeLabel(info);
                if (info.Labels.Count == 0)
                {
                    _mergedDetailInfosOrderByLabelDic[NoLabel].Add(info);
                }
                else
                {
                    bool isAdd = false;
                    foreach (string label in info.Labels)
                    {
                        if (_mergedDetailInfosOrderByLabelDic.ContainsKey(label))
                        {
                            _mergedDetailInfosOrderByLabelDic[label].Add(info);
                            isAdd = true;
                            //考虑到可能会有多个影响模块，我们只记录其中一个
                            break;
                        }
                    }

                    //现有便签里一个都没有该模块
                    if (!isAdd)
                    {
                        _mergedDetailInfosOrderByLabelDic.Add(info.Labels[0], new List<MileStoneMergedInfo> { info });
                    }
                }
            }
        }

        private void IgnoreSomeLabel(MileStoneMergedInfo info)
        {
            var ignoreLabels = new List<string>
            {
                "dev",
                "release",
                "Doing",
                "self",
                "special",
                "To Do",
                "wip_dev",
            };
            for (int i = 0; i < info.Labels.Count; i++)
            {
                if (ignoreLabels.Exists(s => s.Trim().Equals(info.Labels[i].Trim())))
                {
                    info.Labels.Remove(info.Labels[i]);
                }
            }
        }

        private MileStoneState _mileStoneState = MileStoneState.Active;
        private string _currentQueriedMilestoneId = "";
        private string _currentQueriedMergeState = "";
        private const string NoLabel = "not select module";
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region 临时用到的一些接口功能

        /// <summary>
        /// 导出到Excel，（不按模块划分）
        /// </summary>
        private void ExportExcel()
        {
            ExcelHelper.DeleteFile();
            ExcelHelper.Write("Author,Title,Influence");

            foreach (var info in _mergedDetailInfos)
            {
                string content = $"{info.Opend.Name},{info.Title.Format()},";
                content += GetInfluence(info);
                ExcelHelper.Write(content);
            }
        }

        /// <summary>
        /// 获取提交详情
        /// </summary>
        /// <param name="beginDay">统计Commit的开始日期 例：2018-01-01 00:00:00</param>
        /// <param name="endDay">统计Commit的结束日期 例：2019-01-01 00:00:00</param>
        /// <returns>期间commit的个数，commit每人提交个数</returns>
        public async Task<Tuple<int, Dictionary<string, int>>> GetCommitDetail(string beginDay, string endDay)
        {
            var commitList = await GitLabQueryProvider.Query.SimpleQuery.GetCommitCount(GitLabQueryProvider.EasiNoteProjectId);
            var dic = new Dictionary<string, int>();
            var sd1 = Convert.ToDateTime(beginDay);
            var sd2 = Convert.ToDateTime(endDay);
            int cout = 0;

            foreach (var commit in commitList)
            {
                var date = Convert.ToDateTime(commit.Date);
                if (date > sd1 && date < sd2)
                {
                    cout++;
                    if (dic.ContainsKey(commit.AuthorName))
                    {
                        dic[commit.AuthorName]++;
                    }
                    else
                    {
                        dic.Add(commit.AuthorName, 1);
                    }
                }
            }

            return new Tuple<int, Dictionary<string, int>>(cout, dic);
        }

        /// <summary>
        /// 创建label
        /// </summary>
        /// <param name="labels"></param>
        /// <param name="color"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task CreateLabels(List<LabelData> labels, string color, string projectId)
        {
            await GitLabQueryProvider.Query.SimpleQuery.CreateLabels(labels, color, projectId);
        }

        #endregion
    }

    public static class StringExtension
    {
        public static string Format(this string obj)
        {
            if (string.IsNullOrWhiteSpace(obj))
            {
                return string.Empty;
            }
            return obj
                .Replace(",", ".")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace(CustomText.Hint1, "")
                .Replace(CustomText.Hint2, "")
                .Replace(CustomText.Hint3,"")
                .Replace("   ", " ")
                .Replace("  ", " ");
        }
    }
}
