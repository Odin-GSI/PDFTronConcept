using pdftron.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PDFEditorNS
{
    public class BaseAnnotation
    {
        private Rect _rect;

        [DataMember]
        private Guid _id;

        public BaseAnnotation()
        {
            this._id = Guid.NewGuid();
        }

        public Guid ID { get { return _id; } }

        [DataMember]
        public Rect rectArea
        {
            get
            {
                return _rect;
            }
            set
            {
                //double x1 = Math.Min(value.x1, value.x2);
                //double y1 = Math.Max(value.y1, value.y2);
                //double x2 = Math.Max(value.x1,value.x2);
                //double y2 = Math.Min(value.y1, value.y2);

                //_rect = new Rect(x1, y1, x2, y2);

                _rect = value;
            }
        }
    }
}
