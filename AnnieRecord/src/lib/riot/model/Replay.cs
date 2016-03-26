using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
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
        public SortedDictionary<int, byte[]> chunks;
        public SortedDictionary<int, byte[]> keyFrames;

        private readonly int FIRST_SEVERAL_CHUNK_INTERVAL = 30000;
        private readonly int CHUNK_INTERVAL = 500;
        private readonly int SEVERAL_COUNT = 2;

        public int firstChunkId
        {
            get { return chunks.First().Key; }
        }
        public int lastChunkId
        {
            get { return chunks.Last().Key; }
        }

        public int firstKeyFrameId
        {
            get { return keyFrames.First().Key;  }
        }

        public int lastKeyFrameId
        {
            get { return keyFrames.Last().Key; }
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
            chunks = new SortedDictionary<int, byte[]>();
            keyFrames = new SortedDictionary<int, byte[]>();
        }

        public void buildEncryptionKey(String key)
        {
            encryptionKey = key;
        }

        public byte[] getLastChunkInfo()
        {
            int nextInterval;
            if(chunkIndex < SEVERAL_COUNT)
            {
                nextInterval = FIRST_SEVERAL_CHUNK_INTERVAL;
            } else
            {
                nextInterval = CHUNK_INTERVAL;
            }
            if (keyFrameIndex == 0)
            {
                //keyFrameIndex = keyFrameIndex + 1;
            }
            if (chunkIndex >= chunks.Count - 1)
            {
                nextInterval = 0;
                keyFrameIndex = keyFrames.Count - 1;
            }

            var chunkId = chunks.Skip(chunkIndex).First().Key;
            var keyFrameId = keyFrames.Skip(keyFrameIndex).First().Key;
            int nextChunkId;
            if (chunkIndex >= chunks.Count - 1)
            {
                nextChunkId = chunks.Skip(chunkIndex).First().Key;
            } else
            {
                nextChunkId = chunks.Skip(chunkIndex + 1).First().Key;
            }

            var lastChunkInfo = String.Format("{{\"chunkId\":{0},\"availableSince\":30000,\"nextAvailableChunk\":{1},\"keyFrameId\":{2},\"nextChunkId\":{3},\"endStartupChunkId\":{4},\"startGameChunkId\":{5},\"endGameChunkId\":{6},\"duration\":30000}}",
                chunkId,
                nextInterval,
                keyFrameId,
                nextChunkId,
                metaData.endStartupChunkId,
                metaData.startGameChunkId,
                lastChunkId
                );
            if (chunkIndex < chunks.Count - 1)
                chunkIndex = chunkIndex + 1;
            if (chunkIndex % 2 == 1 && keyFrameIndex < keyFrames.Count - 1)
                keyFrameIndex = keyFrameIndex + 1;
            return Encoding.ASCII.GetBytes(lastChunkInfo);
        }

        public byte[] getChunk(HttpListenerRequest request)
        {
            var id = Riot.getResourceIdByPath(request.toSerializableString());
            return chunks[id];
        }

        public byte[] getKeyFrame(HttpListenerRequest request)
        {
            var id = Riot.getResourceIdByPath(request.toSerializableString());
            return keyFrames[id];
        }

        public bool isLastChunk(HttpListenerRequest request)
        {
            var id = Riot.getResourceIdByPath(request.toSerializableString());
            return id == lastChunkId;
        }

        public bool isLastKeyFrame(HttpListenerRequest request)
        {
            var id = Riot.getResourceIdByPath(request.toSerializableString());
            return id == lastKeyFrameId;
        }
    }
}
