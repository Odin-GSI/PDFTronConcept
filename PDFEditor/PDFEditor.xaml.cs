using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using pdftron.PDF;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using pdftron.PDF.Annots;
using static pdftron.PDF.Annot;
using PDFEditor.Controls;
using System.Linq;

namespace PDFEditorNS
{
    public partial class PDFEditor : UserControl
    {
        #region Global vars
        private PDFViewWPF _viewer;
        private Options _activeOption = Options.NONE;
        private AnnotationsContainer _userAnnots = new AnnotationsContainer();
        private System.Windows.Point _lastDoubleClick;
        private string currentLoadedAnnotsFileName = Environment.CurrentDirectory + "\\annotations.xml";
        //private System.Windows.Controls.Primitives.Popup pp;
        private Window popupsOwner;
        //private bool dialogOpen;
        #endregion Global vars

        #region Constructors
        public PDFEditor()
        {
            InitializeComponent();

            _viewer = new PDFViewWPF();
            PDFViewerBorder.Child = _viewer;

            _viewer.MouseDown += Viewer_MouseDown;
            _viewer.MouseUp += Viewer_MouseUp;

            _viewer.MouseDoubleClick += _viewer_MouseDoubleClick;
        }

        
        #endregion Constructors

        #region Current Doc Dependecy Property Logic

        // We need this to be called from the OnCurrentDocPropertyChanged static method
        public PDFViewWPF Viewer { get => _viewer; }

        public static readonly DependencyProperty currentDocProperty = DependencyProperty.Register("CurrentDoc", typeof(string), typeof(PDFEditor), new PropertyMetadata(OnCurrentDocPropertyChanged));

        // Dependency Property for the PDF document
        public string CurrentDoc
        {
            get
            {
                return (string)GetValue(currentDocProperty);
            }
            set
            {
                SetValue(currentDocProperty, value);
                // More logic in OnCurrentDocPropertyChanged
            }
        }
        
        public Window PopupsOwner { get => popupsOwner; set => popupsOwner = value; }

        // This code is necessary for in the CurrentDoc property Setter, SetValue() is called directly - any extra code won't be executed when called by Binding
        private static void OnCurrentDocPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            PDFEditor editor = ((PDFEditor)source);

            if (!String.IsNullOrEmpty((string)e.NewValue))
            {
                var viewer = editor.Viewer;
                viewer.SetPagePresentationMode(PDFViewWPF.PagePresentationMode.e_single_continuous);
                viewer.SetPageViewMode(PDFViewWPF.PageViewMode.e_fit_width);
                //viewer.Height = 750;
                PDFDoc docToLoad = new PDFDoc((string)e.NewValue);
                viewer.SetDoc(docToLoad);
                editor._userAnnots.userAnnotations.Clear();
                editor._userAnnots.HasUnsavedAnnotations = false;

                //string fileAnnots = "Annotations\\" + AnnotationsMannager.getFileName((string)e.NewValue) + ".xml";
                //if (File.Exists(fileAnnots))
                //{
                //    // SWITCH for different Annotations
                //    AnnotationsContainer annotsCont = (AnnotationsContainer)AnnotationsMannager.Deserialize(File.ReadAllText(fileAnnots), typeof(AnnotationsContainer));
                //    foreach (XMLHighlight hl in annotsCont.userHighlights)
                //    {
                //        // Set the Highlights in the current Viewer
                //        ((PDFEditor)source).setHighlight(hl);
                //        // Save the Highlights in the current Container
                //        editor._userAnnots.userHighlights.Add(hl);
                //    }
                //}
                editor.tbCurrentPage.Text = "1";
            }
        }

        #endregion Current Doc Dependecy Property Logic

        #region Annotations Handling
        private void createAnnotation(double x1,double y1,double x2,double y2, bool fromViewer = false)
        {
            PDFDoc temp = _viewer.GetDoc();
            int currentPage = _viewer.CurrentPageNumber;

            //Need to convert coordinates
            if (!fromViewer)
            {
                //_viewer.ConvScreenPtToPagePt(ref x1, ref y1, currentPage);
                //_viewer.ConvScreenPtToPagePt(ref x2, ref y2, currentPage);
                AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, currentPage, ref x1,ref y1);
                AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, currentPage, ref x2,ref y2);
            }

            // Option selected in the Toolbar
            switch (_activeOption)
            {
                case Options.HIGHLIGHT:
                    XMLHighlight userHL = new XMLHighlight();
                    //userHL.color = new double[] { 0.7, 1, 0.7, 1,3};
                    userHL.page = currentPage;
                    userHL.rectArea = new pdftron.PDF.Rect(x1, y1, x2, y2);
                    _userAnnots.userAnnotations.Add(userHL);
                    _userAnnots.HasUnsavedAnnotations = true;
                    setHighlight(userHL);
                    break;
                case Options.COMMENT:
                    pdftron.PDF.Annots.Text txt = pdftron.PDF.Annots.Text.Create(temp, new pdftron.PDF.Rect(x1, y1, x2, y2), "");
                    //pdftron.PDF.Annots.Popup pop = pdftron.PDF.Annots.Popup.Create(temp, new pdftron.PDF.Rect(x1, y1, x2, y2));

                    StickyNote sn = new StickyNote();
                    sn.page = currentPage;
                    sn.rectArea = new pdftron.PDF.Rect(x1, y1, x2, y2);

                    TextPopup popup = new TextPopup();
                    popup.Text = txt.GetContents();
                    popup.Closed += (object closedSender, EventArgs eClosed) => { txt.SetContents(popup.Text); _viewer.Update(); sn.comment = popup.Text; };

                    popup.Owner = this.PopupsOwner;
                    popup.Show();

                    //pop.SetParent(txt);
                    //txt.SetPopup(pop);
                    txt.SetColor(new ColorPt(1, 0, 0));
                    txt.RefreshAppearance();

                    temp.GetPage(currentPage).AnnotPushBack(txt);
                    //temp.GetPage(currentPage).AnnotPushBack(pop);

                    _userAnnots.userAnnotations.Add(sn);
                    _userAnnots.HasUnsavedAnnotations = true;

                    break;
                case Options.NONE:
                    break;
                default:
                    break;
            }
            //temp.Save("modified.pdf", SDFDoc.SaveOptions.e_linearized);
            _viewer.SetCurrentPage(currentPage);
            _viewer.Update();
        }

        private void setHighlight(XMLHighlight xhl)
        {
            PDFDoc temp = _viewer.GetDoc();
            pdftron.PDF.Annots.Highlight hl = pdftron.PDF.Annots.Highlight.Create(temp.GetSDFDoc(), new pdftron.PDF.Rect(xhl.rectArea.x1, xhl.rectArea.y1, xhl.rectArea.x2, xhl.rectArea.y2));
            //hl.SetQuadPoint(0, new QuadPoint(new pdftron.PDF.Point(xDown, yUp), new pdftron.PDF.Point(xUp, yUp), new pdftron.PDF.Point(xUp, yDown), new pdftron.PDF.Point(xDown, yDown)));
            hl.SetColor(new ColorPt(0.7, 1, 0.7, 1), 3);
            temp.GetPage(xhl.page).AnnotPushBack(hl);
        }

        private void setStickyNote(StickyNote sn)
        {
            PDFDoc temp = _viewer.GetDoc();
            pdftron.PDF.Annots.Text t = pdftron.PDF.Annots.Text.Create(temp.GetSDFDoc(), new pdftron.PDF.Rect(sn.rectArea.x1, sn.rectArea.y1, sn.rectArea.x2, sn.rectArea.y2));
            t.SetContents(sn.comment);
            temp.GetPage(sn.page).AnnotPushBack(t);
        }

        #endregion Annotations Handling

        #region MouseClicks

        private void Viewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (xDown.HasValue && yDown.HasValue)
            {
                // No invalid MouseClick
                if ((xDown <= 0) && (yDown <= 0))
                    return;

                double xUp = e.GetPosition(_viewer).X;
                double yUp = e.GetPosition(_viewer).Y;

                // If it was a Click and not a Drag
                if ((xDown == xUp) && (yDown == yUp))
                    return;

                //Check for Annotation and Create it!
                createAnnotation(xDown.Value, yDown.Value, xUp, yUp);

            xDown = null;
            yDown = null;
            }
        }

        // Mouse Click
        double? xDown, yDown;
        private void Viewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            xDown = e.GetPosition(_viewer).X;
            yDown = e.GetPosition(_viewer).Y;
        }

        private void _viewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            xDown = null;
            yDown = null;
            this._lastDoubleClick = e.GetPosition(_viewer);
            var page = _viewer.GetDoc().GetPage(_viewer.GetCurrentPage());
            if (page.GetNumAnnots() > 0)
            {
                Annot annot;
                for (int i = 0; i < page.GetNumAnnots(); i++)
                {
                    annot = page.GetAnnot(i);
                    var rect = annot.GetRect();
                    double relX = _lastDoubleClick.X;
                    double relY = _lastDoubleClick.Y;
                    AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, page.GetIndex(), ref relX, ref relY);
                    if (relX >= rect.x1 && relX <= rect.x2 && relY >= rect.y1 && relY <= rect.y2)
                    {
                        switch (annot.GetType())
                        {
                            case Annot.Type.e_RichMedia:
                                break;
                            case Annot.Type.e_Projection:
                                break;
                            case Annot.Type.e_Redact:
                                break;
                            case Annot.Type.e_3D:
                                break;
                            case Annot.Type.e_Unknown:
                                break;
                            case Annot.Type.e_Watermark:
                                break;
                            case Annot.Type.e_PrinterMark:
                                break;
                            case Annot.Type.e_Movie:
                                break;
                            case Annot.Type.e_Sound:
                                break;
                            case Annot.Type.e_FileAttachment:
                                break;
                            case Annot.Type.e_Squiggly:
                                break;
                            case Annot.Type.e_Underline:
                                break;
                            case Annot.Type.e_TrapNet:
                                break;
                            case Annot.Type.e_Screen:
                                break;
                            case Annot.Type.e_Widget:
                                break;
                            case Annot.Type.e_Popup:
                                break;
                            case Annot.Type.e_Ink:
                                break;
                            case Annot.Type.e_Caret:
                                break;
                            case Annot.Type.e_Stamp:
                                break;
                            case Annot.Type.e_StrikeOut:
                                break;
                            case Annot.Type.e_Highlight:
                                
                                var border = new BorderStyle(BorderStyle.Style.e_solid, 3);  
                                
                                annot.SetBorderStyle(border);
                                page.AnnotRemove(i);
                                page.AnnotPushFront(annot);
                                _viewer.Update();
                                break;
                            case Annot.Type.e_Polyline:
                                break;
                            case Annot.Type.e_Polygon:
                                break;
                            case Annot.Type.e_Circle:
                                break;
                            case Annot.Type.e_Square:
                                break;
                            case Annot.Type.e_Line:
                                break;
                            case Annot.Type.e_FreeText:
                                break;
                            case Annot.Type.e_Link:
                                break;
                            case Annot.Type.e_Text:

                                var a = (StickyNote)_userAnnots.userAnnotations.FirstOrDefault(t => relX >= t.rectArea.x1 && relX <= t.rectArea.x2 && relY >= t.rectArea.y1 && relY <= t.rectArea.y2);

                                if (a == null)
                                    return;

                                TextPopup popup = new TextPopup();
                                popup.Text = annot.GetContents();
                                popup.Closed += (object closedSender, EventArgs eClosed) => { annot.SetContents(popup.Text); _viewer.Update(); a.comment = popup.Text; };

                                popup.Owner = this.PopupsOwner;
                                popup.Show();
                                break;
                            default:
                                break;
                        }
                        //page.AnnotRemove(i);
                        //_viewer.Update();
                        //_userAnnots.RemoveAnnotation(rect);
                    }
                }
            }
        }
        

        #endregion MouseClicks

        #region ToolbarEvents
        private void rbHighlight_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = Options.HIGHLIGHT;
        }
        private void rbNote_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = Options.COMMENT;
        }
        private void rbHighlight_Unchecked(object sender, RoutedEventArgs e)
        {
            _activeOption = Options.NONE;
        }
        private void rbNote_Unchecked(object sender, RoutedEventArgs e)
        {
            _activeOption = Options.NONE;
        }
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            //SaveFileDialog saveDialog = new SaveFileDialog();
            

            //string outputFilePath = "Annotations\\" + AnnotationsMannager.getFileName(_viewer.GetDoc().GetFileName()) + ".xml";
            //string xml = AnnotationsMannager.Serialize((XMLHighlight)userAnnots[0]);
            string xml = AnnotationsMannager.Serialize(_userAnnots);

            File.WriteAllText(currentLoadedAnnotsFileName, xml);
        }
        private void btPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (tbCurrentPage.Text != "1")
            {
                var newPage = int.Parse(tbCurrentPage.Text) - 1;
                _viewer.SetCurrentPage(newPage);
                tbCurrentPage.Text = newPage.ToString();
            }
        }
        private void btNext_Click(object sender, RoutedEventArgs e)
        {
            if (tbCurrentPage.Text != _viewer.GetPageCount().ToString())
            {
                var newPage = int.Parse(tbCurrentPage.Text) + 1;
                _viewer.SetCurrentPage(newPage);
                tbCurrentPage.Text = newPage.ToString();
            }
        }
        private void tbCurrentPage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+");

            return !regex.IsMatch(text);
        }

        private void delBtn_Click(object sender, RoutedEventArgs e)
        {
            var page = _viewer.GetDoc().GetPage(_viewer.GetCurrentPage());
            if (page.GetNumAnnots() > 0)
            {
                Annot annot;
                for (int i = 0; i < page.GetNumAnnots(); i++)
                {
                    annot = page.GetAnnot(i);
                    var rect = annot.GetRect();
                    double relX = _lastDoubleClick.X;
                    double relY = _lastDoubleClick.Y;
                    AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, page.GetIndex(), ref relX, ref relY);
                    if (relX >= rect.x1 && relX <= rect.x2 && relY >= rect.y1 && relY <= rect.y2)
                    {
                        page.AnnotRemove(i);
                        _viewer.Update();
                        _userAnnots.RemoveAnnotation(rect);
                    }
                }
            }
        }

        private void saveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.CheckPathExists = true;
            saveDialog.DefaultExt = ".xml";
            
           if(saveDialog.ShowDialog() == true)
            {                
                string xml = AnnotationsMannager.Serialize(_userAnnots);
                File.WriteAllText(saveDialog.FileName, xml);
                currentLoadedAnnotsFileName = saveDialog.FileName;
            }
        }

        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            setLoadedAnnotsFile();

            //string fileAnnots = "Annotations\\" + AnnotationsMannager.getFileName((string)e.NewValue) + ".xml";
            if (File.Exists(currentLoadedAnnotsFileName))
            {
                // SWITCH for different Annotations
                AnnotationsContainer annotsCont = (AnnotationsContainer)AnnotationsMannager.Deserialize(File.ReadAllText(currentLoadedAnnotsFileName), typeof(AnnotationsContainer));
                foreach (BaseAnnotation a in annotsCont.userAnnotations)
                {
                    // Aqui hay que hacer un Factory
                    if (a is XMLHighlight)
                        this.setHighlight((XMLHighlight)a);

                    if (a is StickyNote)
                        this.setStickyNote((StickyNote)a);

                    // Save the Annotation in the current Container
                    this._userAnnots.userAnnotations.Add(a);
                }
                _viewer.Update();
            }
        }

        private void setLoadedAnnotsFile()
        {
            
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            
            fileDialog.Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*";
            fileDialog.DefaultExt = ".xml";
            //dialogOpen = true;
            if (fileDialog.ShowDialog() == true)
            {
                this._userAnnots.userAnnotations.Clear();
                this._userAnnots.HasUnsavedAnnotations = false;

                for (int i = 1; i <= _viewer.GetDoc().GetPageCount(); i++)
                    for (int j = 0; j < _viewer.GetDoc().GetPage(i).GetNumAnnots(); j++)
                        _viewer.GetDoc().GetPage(i).AnnotRemove(j);

                currentLoadedAnnotsFileName = fileDialog.FileName;
            }
            //dialogOpen = false;
        }

        private void tbCurrentPage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewer != null && !String.IsNullOrEmpty(tbCurrentPage.Text))
            {
                var newPage = int.Parse(tbCurrentPage.Text);
                if (newPage >= 0 && newPage <= _viewer.GetPageCount())
                {
                    _viewer.SetCurrentPage(newPage);
                    tbCurrentPage.Text = newPage.ToString();
                }
                else
                    MessageBox.Show("Page number out of range");
                e.Handled = true;
            }
        }

        #endregion ToolbarEvents
    }
}
