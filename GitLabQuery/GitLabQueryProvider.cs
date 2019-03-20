using System.IO;
using System.Text;
using System.Windows;
using GitLabApiClient;

namespace GitLabQuery
{
    public static class GitLabQueryProvider
    {
        private static GitLabClient _gitLabApiClient;

        private static readonly object Locker = new object();

        public static GitLabClient Query
        {
            get
            {
                lock (Locker)
                {
                    GetInitInfo();
                    return _gitLabApiClient ?? (_gitLabApiClient =
                               new GitLabClient(_gitLabAddress, _token));
                }
            }
        }

        /// <summary>
        /// 仓库在GitLab的项目Id
        /// </summary>
        public static string EasiNoteProjectId;

        /// <summary>
        /// token的具体生成规则见GitLab文档说明
        /// </summary>
        private static string _token;

        /// <summary>
        /// GitLab项目的地址
        /// </summary>
        private static string _gitLabAddress;

        private static void GetInitInfo()
        {
            string[] infoLines = File.ReadAllLines("Config.ini", Encoding.Default);
            foreach (string line in infoLines)
            {
                string[] result = line.Split('=');
                if (result.Length != 2)
                {
                    MessageBox.Show("Config.ini配置文件错误");
                }

                if (result[0] == "Token")
                {
                    _token = result[1];
                }

                if (result[0] == "ProjectId")
                {
                    EasiNoteProjectId = result[1];
                }

                if (result[0] == "GitLabAddress")
                {
                    _gitLabAddress = result[1];
                }
            }
        }
    }
}
