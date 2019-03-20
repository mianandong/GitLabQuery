using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using GitLabApiClient;
using GitLabApiClient.Models.MileStones.Request;
using GitLabApiClient.Models.MileStones.Responses;

namespace GitLabQuery
{
    public partial class GitLabQueryWindow : Window
    {
        private readonly GitLabQueryViewModel _viewModel;

        private readonly List<MileStoneSelector> _mileStoneSelectors = new List<MileStoneSelector>
        {
            new MileStoneSelector { Name = "Active", State = MileStoneState.Active},
            new MileStoneSelector { Name = "Closed", State = MileStoneState.Closed}
        };

        private readonly List<string> _mergeState = new List<string> { "merged", "closed", "opened" };

        public GitLabQueryWindow()
        {
            InitializeComponent();
            _viewModel = new GitLabQueryViewModel();
            ComboBox.ItemsSource = _mileStoneSelectors;
            ComboBox1.ItemsSource = _viewModel.MileStones;
            ListBox.ItemsSource = _viewModel.MergedInfos;
            MergeStateComboBox.ItemsSource = _mergeState;
        }

        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.MileStoneState = (MileStoneState)((ComboBox)sender).SelectedValue;
        }

        private void ComboBox1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.MileStoneId = ((MileStoneInfo)((ComboBox)sender).SelectedValue)?.Id;
        }

        private async void GetMergedInfosButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetAllButtonEnable(false);
            await _viewModel.GetCommitInfos();
            SetAllButtonEnable(true);
        }

        private async void Export_OnClick(object sender, RoutedEventArgs e)
        {
            SetAllButtonEnable(false);
            await _viewModel.ExportMergedInfos();
            SetAllButtonEnable(true);
        }

        private void MergeStateComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.MergeState = (string)((ComboBox)sender).SelectedValue;
        }

        private void SetAllButtonEnable(bool isEnable)
        {
            GetMergedInfosButton.IsEnabled = isEnable;
            ExportButton.IsEnabled = isEnable;
            TestButton.IsEnabled = isEnable;
        }

        private void TestButton_OnClick(object sender, RoutedEventArgs e)
        {
            string[] a = File.ReadAllLines("E:\\QQDownload\\labels.txt", Encoding.UTF8);
            var list = new List<LabelData>();
            foreach (string s in a)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    continue;
                }
                list.Add(new LabelData{Name = s});
            }
            //一次最大创建60个便签
            _viewModel.CreateLabels(list, "#ffc0cb", GitLabQueryProvider.EasiNoteProjectId);
        }
    }

    public class MileStoneSelector
    {
        public string Name { get; set; }
        public MileStoneState State { get; set; }
    }
}
