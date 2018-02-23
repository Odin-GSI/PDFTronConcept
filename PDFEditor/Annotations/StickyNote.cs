using pdftron.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PDFEditorNS
{
    [KnownType(typeof(StickyNote))]
    public class StickyNote : BaseAnnotation
    {
        [DataMember]
        public int page { get; set; }

        [DataMember]
        public string comment { get; set; }
    }
}
