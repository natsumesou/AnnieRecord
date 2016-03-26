using AnnieRecord.riot;
using AnnieRecord.riot.model;
using Newtonsoft.Json;
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

                    if (context.Request.RawUrl.Contains(SPECTATE_METHOD.version.ToString()))
                    {
                        response.AddHeader("Content-Type", "text/plain");
                        buffer = replay.version;
                    }
                    else if (context.Request.RawUrl.Contains(SPECTATE_METHOD.getGameMetaData.ToString()))
                    {
                        response.AddHeader("Content-Type", "application/json");
                        buffer = replay.metadata;
                    }
                    else if (context.Request.RawUrl.Contains(SPECTATE_METHOD.getLastChunkInfo.ToString()))
                    {
                        buffer = replay.getLastChunkInfo();
                    }
                    else if (context.Request.RawUrl.Contains(SPECTATE_METHOD.getGameDataChunk.ToString()))
                    {
                        response.AddHeader("Content-Type", "application/octet-stream");
                        buffer = replay.getChunk(context.Request);
                    }
                    else if(context.Request.RawUrl.Contains(SPECTATE_METHOD.getKeyFrame.ToString()))
                    {
                        response.AddHeader("Content-Type", "application/octet-stream");
                        buffer = replay.getKeyFrame(context.Request);
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }

                    var reqStr = context.Request.toSerializableString();
                    isLastChunkReqeust = replay.isLastChunk(reqStr);
                    isLastKeyFrameReqauest = replay.isLastKeyFrame(reqStr);
                }
                catch (KeyNotFoundException)
                {
                    System.Diagnostics.Debug.WriteLine("dont know: " + context.Request.RawUrl);

                    response.AddHeader("Content-Type", "text/plain");
                    buffer = Encoding.ASCII.GetBytes("404");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                response.ContentLength64 = buffer.Length;

                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                if (isLastKeyFrameReqauest && isLastChunkReqeust)
                {
                    System.Diagnostics.Debug.WriteLine("exit api server");
                    break;
                }
            }
        }
    }
}
