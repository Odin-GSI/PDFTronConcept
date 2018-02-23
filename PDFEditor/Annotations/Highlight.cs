﻿using pdftron.PDF;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PDFEditorNS
{
    [KnownType(typeof(XMLHighlight))]
    public class XMLHighlight : BaseAnnotation
    {
        //[DataMember]
        //public double[] color { get; set; }

        [DataMember]
        public int page { get; set; }
    }
}
