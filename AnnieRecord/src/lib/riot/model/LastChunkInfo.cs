using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public partial class LastChunkInfo : BaseModel
    {
        public int chunkId
        {
            get;
            private set;
        }
        public int nextAvailableChunk
        {
            get;
            private set;
        }
        public int keyFrameId
        {
            get;
            private set;
        }
        public int endGameChunkId
        {
            get;
            private set;
        }

        public bool isLastChunk()
        {
            return endGameChunkId != 0 && chunkId >= endGameChunkId;
        }
    }
}
