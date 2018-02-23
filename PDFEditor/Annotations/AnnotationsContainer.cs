using pdftron.PDF;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PDFEditorNS
{
    [DataContract]
    public class AnnotationsContainer
    {
        private bool hasUnsavedAnnotations = false;

        [DataMember]
        public List<BaseAnnotation> userAnnotations = new List<BaseAnnotation>();

        public bool HasUnsavedAnnotations { get => hasUnsavedAnnotations; set => hasUnsavedAnnotations = value; }

        public bool RemoveAnnotation(Rect rect)
        {
            foreach(BaseAnnotation b in userAnnotations)
                if ((b.rectArea.x1 == rect.x1) &&(b.rectArea.y1 == rect.y1) &&(b.rectArea.x2 == rect.x2) &&(b.rectArea.y2== rect.y2))
                {
                    userAnnotations.Remove(b);
                    HasUnsavedAnnotations = true;
                    return true;
                }

            return false;
        }
    }
}
