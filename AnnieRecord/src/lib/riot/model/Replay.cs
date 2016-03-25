using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public partial class Replay : BaseModel
    {
        public long gameId;
        public Region region;
        public String encryptionKey;
        public byte[] metadata;
        public byte[] version;
        public int chunkIndex;
        public int keyFrameIndex;
        public Dictionary<String, byte[]> chunks;
        public Dictionary<String, byte[]> keyFrames;

        private readonly String CHUNK_PATTERN = @"GET /observer-mode/rest/consumer/getGameDataChunk/(.*)/(.*)/(?<id>[0-9]+)/token";
        private readonly String KEYFRAME_PATTERN = @"GET /observer-mode/rest/consumer/getKeyFrame/(.*)/(.*)/(?<id>[0-9]+)/token";


        private int _firstChunkId = 0;
        public int firstChunkId
        {
            get
            {
                if(_firstChunkId == 0)
                {
                    var ids = initializeListData(chunks, CHUNK_PATTERN);
                    _firstChunkId = ids[0];
                    _lastChunkId = ids[1];
                }
                return _firstChunkId;
            }
        }
        private int _lastChunkId = 0;
        public int lastChunkId
        {
            get
            {
                if(_lastChunkId == 0)
                {
                    var ids = initializeListData(chunks, CHUNK_PATTERN);
                    _firstChunkId = ids[0];
                    _lastChunkId = ids[1];
                }
                return _lastChunkId;
            }
        }

        private int _firstKeyFrameId = 0;
        public int firstKeyFrameId
        {
            get
            {
                if(_firstKeyFrameId == 0)
                {
                    var ids = initializeListData(keyFrames, KEYFRAME_PATTERN);
                    _firstKeyFrameId = ids[0];
                    _lastKeyFrameId = ids[1];
                }
                return _firstKeyFrameId;
            }
        }

        private int _lastKeyFrameId = 0;
        public int lastKeyFrameId
        {
            get
            {
                if(_lastKeyFrameId == 0)
                {
                    var ids = initializeListData(keyFrames, KEYFRAME_PATTERN);
                    _firstKeyFrameId = ids[0];
                    _lastKeyFrameId = ids[1];
                }
                return _lastKeyFrameId;
            }
        }

        GameMetaData _metaData;
        private GameMetaData metaData
        {
            get
            {
                if (_metaData == null)
                {
                    var json = Encoding.ASCII.GetString(metadata);
                    _metaData = JsonConvert.DeserializeObject<GameMetaData>(json);
                }
                return _metaData; 
            }
        }

        private static readonly String FILENAME_FORMAT = "replay_{0}_{1}.anr";

        public String filename
        {
            get
            {
                return String.Format(FILENAME_FORMAT, gameId, region.platform);
            }
        }

        public static String filenameFromGame(Game game)
        {
            return String.Format(FILENAME_FORMAT, game.id, game.platformId);
        }

        public Replay(long id, String encryptionKeyStr, Region reg)
        {
            gameId = id;
            encryptionKey = encryptionKeyStr;
            region = reg;
            chunks = new Dictionary<string, byte[]>();
            keyFrames = new Dictionary<string, byte[]>();
        }

        public void buildEncryptionKey(String key)
        {
            encryptionKey = key;
        }

        public byte[] getLastChunkInfo()
        {
            var nextInterval = 30000;
            if (chunkIndex == 0)
            {
                chunkIndex = firstChunkId;
            }
            if (keyFrameIndex == 0)
            {
                keyFrameIndex = firstKeyFrameId;
            }
            if (chunkIndex == lastChunkId)
            {
                nextInterval = 0;
                keyFrameIndex = lastKeyFrameId;
            }

            var lastChunkInfo = String.Format("{{\"chunkId\":{0},\"availableSince\":30000,\"nextAvailableChunk\":{1},\"keyFrameId\":{2},\"nextChunkId\":{3},\"endStartupChunkId\":{4},\"startGameChunkId\":{5},\"endGameChunkId\":{6},\"duration\":30000}}",
                chunkIndex,
                nextInterval,
                keyFrameIndex,
                chunkIndex + 1,
                metaData.endStartupChunkId,
                metaData.startGameChunkId,
                lastChunkId
                );
            if (chunkIndex < lastChunkId)
                chunkIndex = chunkIndex + 1;
            if (keyFrameIndex < lastKeyFrameId)
            {
                keyFrameIndex = (int)Math.Floor((double)(chunkIndex / 2));
                if (keyFrameIndex < firstKeyFrameId)
                    keyFrameIndex = firstKeyFrameId;
            }
            else
            {
                keyFrameIndex = lastKeyFrameId;
            }
            return Encoding.ASCII.GetBytes(lastChunkInfo);
        }

        public byte[] getChunk(HttpListenerRequest request)
        {
            return chunks[request.toSerializableString()];
        }

        public byte[] getKeyFrame(HttpListenerRequest request)
        {
            return keyFrames[request.toSerializableString()];
        }

        public bool isLastChunk(String key)
        {
            int id = 0;
            foreach (Match m in Regex.Matches(key, CHUNK_PATTERN))
            {
                id = Int32.Parse(m.Groups["id"].Value);
            }
            return id == lastChunkId;
        }

        public bool isLastKeyFrame(String key)
        {
            int id = 0;
            foreach (Match m in Regex.Matches(key, KEYFRAME_PATTERN))
            {
                id = Int32.Parse(m.Groups["id"].Value);
            }
            return id == lastKeyFrameId;
        }

        private int[] initializeListData(Dictionary<String, byte[]> list, String pattern)
        {
            int maxId = 0;
            int minId = 10000;
            foreach (var entry in list)
            {
                int id = 0;
                foreach (Match m in Regex.Matches(entry.Key, pattern))
                {
                    id = Int32.Parse(m.Groups["id"].Value);
                    if (id > maxId)
                    {
                        maxId = id;
                    }
                    if (id < minId && id > 1)
                    {
                        minId = id;
                    }
                }
            }
            return new int[2] { minId, maxId };
        }
    }
}
