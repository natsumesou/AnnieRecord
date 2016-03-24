using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public class Server
    {
        public static readonly String HOST = "localhost";
        public static readonly String PORT = "55555";

        private HttpListener listener;
        private Replay replay;
        private Thread thread;

        private bool isLastChunkReqeust = false;
        private bool isLastKeyFrameReqauest = false;

        public Server(Replay r)
        {
            replay = r;
            listener = new HttpListener();
            listener.Prefixes.Add(String.Format("http://{0}:{1}/", HOST, PORT));
            listener.Start();
        }

        public void run()
        {
            thread = new Thread(runServer);
            thread.IsBackground = true;
            thread.Start();
        }

        private void runServer()
        {
            while (listener.IsListening)
            {
                var context = listener.GetContext();
                var response = context.Response;

                byte[] buffer;
                try
                {
                    System.Diagnostics.Debug.WriteLine(context.Request.Url.AbsoluteUri);

                    // ここのコードはanrファイルを再生成するといらなくなるはず？
                    //getGameMetaDataの{gameId}/1/tokenの1が謎の数字になるときがあるのでそこだけ対応必要かも？
                    if (context.Request.RawUrl.IndexOf("getGameMetaData") >= 0)
                    {
                        buffer = replay.data["GET /observer-mode/rest/consumer/getGameMetaData/JP1/116763611/1/token"];
                    }
                    else if (context.Request.RawUrl.IndexOf("getLastChunkInfo") >= 0)
                    {
                        buffer = Encoding.ASCII.GetBytes("{\"chunkId\":9,\"availableSince\":9198,\"nextAvailableChunk\":0,\"keyFrameId\":2,\"nextChunkId\":9,\"endStartupChunkId\":2,\"startGameChunkId\":2,\"endGameChunkId\":9,\"duration\":30000}");
                        //{"chunkId":80,"availableSince":31079,"nextAvailableChunk":0,"keyFrameId":39,"nextChunkId":80,"endStartupChunkId":2,"startGameChunkId":4,"endGameChunkId":80,"duration":13419}
                    }
                    else
                    {
                        buffer = replay.getData(context.Request);
                    }

                    if (context.Request.RawUrl.IndexOf("version") >= 0)
                    {
                        response.AddHeader("Content-Type", "text/plain");
                    }
                    else if (context.Request.RawUrl.IndexOf("getGameMetaData") >= 0)
                    {
                        response.AddHeader("Content-Type", "application/json");
                    }
                    else
                    {
                        response.AddHeader("Content-Type", "application/octet-stream");
                    }

                    var reqStr = context.Request.toSerializableString();
                    if (reqStr.Equals(replay.lastChunkPath))
                        isLastChunkReqeust = true;
                    if (context.Request.toSerializableString().Equals(replay.lastkeyFramePath))
                        isLastKeyFrameReqauest = true;
                }
                catch (KeyNotFoundException)
                {
                    buffer = Encoding.ASCII.GetBytes("404");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                response.ContentLength64 = buffer.Length;

                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                if (isLastKeyFrameReqauest && isLastChunkReqeust)
                    break;
            }
        }
    }
}
