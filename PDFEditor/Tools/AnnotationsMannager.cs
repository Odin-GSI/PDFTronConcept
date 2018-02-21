using pdftron.PDF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PDFEditorNS
{
    public class AnnotationsMannager
    {
        #region Serialization
        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static object Deserialize(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(toType);
                return deserializer.ReadObject(stream);
            }
        }
        #endregion Serialization

        public static string getFileName(string filePath)
        {
            string[] filenamePathParts = filePath.Split('\\');
            return filenamePathParts[filenamePathParts.Count() - 1];
        }

        public static void ConvertScreenPositionsToPagePositions(PDFViewWPF viewer, int currentPageIndex, ref double x, ref double y)
        {
            viewer.ConvScreenPtToPagePt(ref x, ref y, currentPageIndex);
        }
    }
}
