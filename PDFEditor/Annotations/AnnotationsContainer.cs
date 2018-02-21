using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PDFEditorNS
{
    [DataContract]
    public class AnnotationsContainer
    {
        [DataMember]
        public List<XMLHighlight> userHighlights = new List<XMLHighlight>();
    }
}
