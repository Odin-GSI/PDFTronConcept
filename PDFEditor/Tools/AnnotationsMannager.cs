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
                DataContractSerializer deserializer = new DataContractSerializer(toType, new List<Type> { typeof(XMLHighlight),typeof(StickyNote)});
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

        public static Rect NormalizeRect(Rect rect)
        {
            double x1 = Math.Min(rect.x1, rect.x2);
            double y1 = Math.Min(rect.y1, rect.y2);
            double x2 = Math.Max(rect.x1, rect.x2);
            double y2 = Math.Max(rect.y1, rect.y2);

            return new Rect(x1, y1, x2, y2);
        }
    }
}
