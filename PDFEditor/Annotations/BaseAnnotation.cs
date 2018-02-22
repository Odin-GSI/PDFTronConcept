using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PDFEditorNS
{
    [DataContract]
    public class BaseAnnotation
    {
        [DataMember]
        private Guid _id;

        public BaseAnnotation()
        {
            this._id = Guid.NewGuid();
        }

        public Guid ID { get { return _id; } }
    }
}
