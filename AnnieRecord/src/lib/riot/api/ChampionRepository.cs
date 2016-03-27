using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    /// <summary>
    /// チャンピオンのデータを取得する
    /// 初回やチャンピオンのデータが更新された場合はローカルのデータに書き込み処理をする
    /// </summary>
    public partial class Champion
    {
        private static readonly String FILENAME = "champions.anr";

        /// <summary>
        /// チャンピオン情報の取得
        /// </summary>
        /// <param name="championId">チャンピオンのID</param>
        /// <param name="dir">データを格納しておくディレクトリ。champions.anrというファイル名で保存される。デフォルトで実行中のディレクトリが指定される。新しいチャンピオンが追加されると自動で更新される。</param>
        /// <returns></returns>
        public static Champion find(long championId, String dir = "")
        {
            var targetDir = dir;
            if (dir.Equals(""))
            {
                targetDir = Environment.CurrentDirectory;
            }
            var champions = find(targetDir);

            try {
                return champions[championId];
            }
            catch (KeyNotFoundException)
            {
                champions = findFromAPI();
                create(champions, targetDir);
            }
            return champions[championId];
        }

        private static Dictionary<long, Champion> find(String dir)
        {
            var champions = findFromLocal(dir);
            if (champions.Count == 0)
            {
                champions = findFromAPI();
                create(champions, dir);
            }
            return champions;
        }

        private static void create(Dictionary<long, Champion> champions, String dir)
        {
            var bf = new BinaryFormatter();
            using(var ms = new MemoryStream())
            {
                bf.Serialize(ms, champions);
                File.WriteAllBytes(dir + "\\" + FILENAME, ms.ToArray());
            }
        }

        private static Dictionary<long, Champion> findFromAPI()
        {
            var request = Riot.Instance.buildRequest("/api/lol/static-data/{region}/{version}/champion");
            request.AddUrlSegment("version", Riot.Instance.globalApiClientVersion);
            request.AddUrlSegment("region", Riot.Instance.region.type.ToString());
            request.RootElement = "data";
            var response = Riot.Instance.apiClient.Execute<Dictionary<string, Champion>>(request);

            var champions = new Dictionary<long, Champion>();
            foreach (var champion in response.Data)
            {
                champions.Add(champion.Value.id, champion.Value);
            }
            return champions;
        }

        private static Dictionary<long, Champion> findFromLocal(String dir)
        {
            byte[] bytes;
            try {
                bytes = File.ReadAllBytes(dir + "\\" + FILENAME);
            }
            catch (FileNotFoundException)
            {
                return new Dictionary<long, Champion>();
            }
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(bytes))
            {
                return (Dictionary<long, Champion>)bf.Deserialize(ms);
            }
        }
    }
}
