using PDF_Service.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PDF_Service.Controllers
{
    public class PDFController : ApiController
    {
        //TEST CDN
        public Task<HttpResponseMessage> GetPDFCDN()
        {
            var resourceUrl = @"C:\GSI\sample.pdf";
            var range = ExtractRange(this.Request.Headers);
            var request = new HttpRequestMessage(HttpMethod.Get, resourceUrl);
            if (null != range) request.Headers.Range = new RangeHeaderValue(range.Ranges.FirstOrDefault().From, range.Ranges.FirstOrDefault().To);
            var client = new HttpClient();
            var response = client.SendAsync(request).ContinueWith<HttpResponseMessage>(t => {
                var finalResp = new HttpResponseMessage(HttpStatusCode.OK);
                finalResp.Content = t.Result.Content;
                if (null != range) finalResp.StatusCode = HttpStatusCode.PartialContent;
                return finalResp;
            });
            return response;
        }
        private static RangeHeaderValue ExtractRange(HttpRequestHeaders headers)
        {
            if (null == headers) throw new ArgumentNullException("headers");
            const int readStreamBufferSize = 1024 * 1024;
            var hasRange = (null != headers.Range && headers.Range.Ranges.Any());
            var rangeHeader = hasRange ? headers.Range : new RangeHeaderValue(0, readStreamBufferSize);
            if (!hasRange) return rangeHeader;
            // it is better to limit the request to a specific range in order to do no have an out-of-memory exception
            var range = rangeHeader.Ranges.ElementAt(0);
            var from = range.From.GetValueOrDefault(0);
            rangeHeader = new RangeHeaderValue(@from, @from + range.To.GetValueOrDefault(readStreamBufferSize));
            return rangeHeader;
        }

        //TEST 2
        public IFileProvider FileProvider { get; set; }
        public PDFController()
        {
            FileProvider = new FileProvider();
        }
        public HttpResponseMessage GetPDF()
        {
            HttpResponseMessage response = Request.CreateResponse();
            string fileName = "sample.pdf";
            if (!FileProvider.Exists(fileName))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            //metaData.FileResponseMessage.IsExists = false;
            //metaData.FileResponseMessage.Content = string.Format("{0} file is not found !", fileName);
            FileInfo fileInfo = new FileInfo(FileProvider.DefaultFileLocation + fileName);
            response.Headers.AcceptRanges.Add("bytes");
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StreamContent(fileInfo.ReadStream());
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentLength = fileInfo.Length;

            return response;
        }

        //TEST BYTE[]
        public byte[] GetPDFBytes()
        {
            IEnumerable<string> range;
            var bytes = File.ReadAllBytes(@"C:\GSI\sample.pdf");
            if (Request.Headers.TryGetValues("Range", out range))
            {
                //hacer lo del rango
                var byteValuesArray = range.First().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                int start = int.Parse(byteValuesArray[0]);
                int end = int.Parse(byteValuesArray[1]);
                byte[] toReturn = new byte[end - start + 1];
                bytes.CopyTo(toReturn, start);
                return toReturn;
            }
            else
            {
                return bytes;
            }
        }

        //TEST STREAM
        //[ActionName("Sample")]
        [HttpGet]
        public HttpResponseMessage Sample()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new System.IO.FileStream(@"C:\GSI\sample.pdf", System.IO.FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            return response;
        }

        [Route("fileSample")]
        [HttpGet]
        public HttpResponseMessage GetFile()
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                var file = new System.IO.FileStream(@"C:\GSI\sample.pdf", System.IO.FileMode.Open);
                result.Content = new StreamContent(file);
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                var value = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                value.FileName = "sample";
                result.Content.Headers.ContentDisposition = value;
            }
            catch (Exception ex)
            {
                // log your exception details here
                result = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return result;
        }

        //TEST RANGE ---------------------THIS IS IT!!!!!
        private static readonly MediaTypeHeaderValue _mediaType = MediaTypeHeaderValue.Parse("application/pdf");
        [HttpGet]
        [HttpHead]
        [Route("somefile")]
        public HttpResponseMessage RangePDF()
        {
            string file = @"C:\GSI\BIG3.pdf";

            FileInfo fi = new FileInfo(file);

            if (Request.Method.Method == "HEAD")
            {
                HttpResponseMessage headresponse = new HttpResponseMessage(HttpStatusCode.OK);
                headresponse.Headers.Add("Accept-Ranges", "bytes");
                headresponse.Content = new StringContent("BIG3.pdf");
                headresponse.Content.Headers.Add("Content-Length", ""+fi.Length);

                return headresponse;
            }

            var content = File.ReadAllBytes(file);

            // A MemoryStream is seekable allowing us to do ranges over it. Same goes for FileStream.
            MemoryStream memStream = new MemoryStream(content);
  
             // Check to see if this is a range request (i.e. contains a Range header field)
             // Range requests can also be made conditional using the If-Range header field. This can be 
             // used to generate a request which says: send me the range if the content hasn't changed; 
             // otherwise send me the whole thing.
             if (Request.Headers.Range != null)
             {
                try
                 {
                     HttpResponseMessage partialResponse = Request.CreateResponse(HttpStatusCode.PartialContent);
                     partialResponse.Content = new ByteRangeStreamContent(memStream, Request.Headers.Range, _mediaType);
                     return partialResponse;
                 }
                 catch (InvalidByteRangeException invalidByteRangeException)
                 {
                     return Request.CreateErrorResponse(invalidByteRangeException);
                 }
             }
             else
             {
                 // If it is not a range request we just send the whole thing as normal
                 HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
                 fullResponse.Content = new StreamContent(memStream);
                 fullResponse.Content.Headers.ContentType = _mediaType;
                 return fullResponse;
             }
        }

        // TEST RANGE 2
        private const string MimeType = "application/pdf";
        private const string path = @"C:\GSI\";
        private const string logFile = @"C:\GSI\PDFRangeTest.log";
        [HttpGet]
        [HttpHead]
        [Route("byRangeFile")]
        public HttpResponseMessage DownloadFile()
        {
            if (Request.Method.Method == "HEAD")
                return new HttpResponseMessage(HttpStatusCode.OK);

            string fileName = "BIG2.pdf";
            this.LogRequestHttpHeaders(logFile, Request);

            HttpResponseMessage result = null;
            var fullFilePath = Path.Combine(path, fileName);

            if (Request.Headers.Range == null || Request.Headers.Range.Ranges.Count == 0 ||
                Request.Headers.Range.Ranges.FirstOrDefault().From.Value == 0)
            {
                // Get the complete file
                FileStream sourceStream = File.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BufferedStream bs = new BufferedStream(sourceStream);

                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(bs);

                result.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeType);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
            }
            else
            {
                // Get the partial part
                var item = Request.Headers.Range.Ranges.FirstOrDefault();
                if (item != null && item.From.HasValue)
                {
                    result = this.GetPartialContent(fileName, item.From.Value);
                }
            }

            this.LogResponseHttpHeaders(logFile, result);

            return result;
        }

        private HttpResponseMessage GetPartialContent(string fileName, long partial)
        {
            var fullFilePath = Path.Combine(path, fileName);
            FileInfo fileInfo = new FileInfo(fullFilePath);
            long startByte = partial;

            Action<Stream, HttpContent, TransportContext> pushContentAction = (outputStream, content, context) =>
            {
                try
                {
                    var buffer = new byte[65536];
                    using (var file = File.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var bytesRead = 1;
                        file.Seek(startByte, SeekOrigin.Begin);
                        int length = Convert.ToInt32((fileInfo.Length - 1) - startByte) + 1;

                        while (length > 0 && bytesRead > 0)
                        {
                            bytesRead = file.Read(buffer, 0, Math.Min(length, buffer.Length));
                            outputStream.Write(buffer, 0, bytesRead);
                            length -= bytesRead;
                        }
                    }
                }
                catch (HttpException ex)
                {

                    this.LogException(ex);
                }
                finally
                {
                    outputStream.Close();
                }
            };

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.PartialContent);
            result.Content = new PushStreamContent(pushContentAction, new MediaTypeHeaderValue(MimeType));
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return result;
        }

        private void LogRequestHttpHeaders(string logFile, HttpRequestMessage request)
        {
            string output = string.Concat("REQUEST INFORMATION (", request.Method, ")", Environment.NewLine);
            foreach (PropertyInfo info in request.Headers.GetType().GetProperties())
            {
                var name = info.Name;
                var value = info.GetValue(request.Headers);

                output += string.Format("{0}: {1}{2}", name, value, Environment.NewLine);
            }

            output += Environment.NewLine + Environment.NewLine;

            File.AppendAllText(logFile, output);
        }

        private void LogResponseHttpHeaders(string logFile, HttpResponseMessage response)
        {
            string output = string.Concat("RESPONSE INFORMATION (", response.StatusCode.ToString("d"), ")", Environment.NewLine);
            foreach (PropertyInfo info in response.Headers.GetType().GetProperties())
            {
                var name = info.Name;
                var value = info.GetValue(response.Headers);
                if (value == null)
                {
                    continue;
                }

                output += string.Format("{0}: {1}{2}", name, value, Environment.NewLine);
            }

            output += Environment.NewLine + Environment.NewLine;

            File.AppendAllText(logFile, output);
        }

        private void LogException(Exception exception)
        {
            string output = string.Format("=======> Error while processing previous request. {0}{1}", Environment.NewLine, exception.ToString());
            File.AppendAllText(logFile, output);
        }
    }
}
