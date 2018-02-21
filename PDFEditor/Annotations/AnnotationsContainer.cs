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
        public List<XMLHighlight> userHighlights = new List<XMLHighlight>();

        public bool HasUnsavedAnnotations { get => hasUnsavedAnnotations; set => hasUnsavedAnnotations = value; }

        public bool RemoveAnnotation(Rect rect)
        {
            foreach(XMLHighlight hl in userHighlights)
                if ((hl.rectArea.x1 == rect.x1) &&(hl.rectArea.y1 == rect.y1) &&(hl.rectArea.x2 == rect.x2) &&(hl.rectArea.y2== rect.y2))
                {
                    userHighlights.Remove(hl);
                    HasUnsavedAnnotations = true;
                    return true;
                }

            return false;
        }
    }
}
