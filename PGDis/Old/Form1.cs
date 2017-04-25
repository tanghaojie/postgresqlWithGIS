using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data
{
    public partial class Form1 : Form
    {
        string LOG_PATH = "";
        public Form1()
        {
            InitializeComponent();
            SetLog();
        }

        private void SetLog()
        {
            LOG_PATH = txtLog.Text;
        }

        private long ReadCSV(string csvPath, string gdbPath, string fcName, long startLine, long endLine)
        {
            long total = 0;
            IWorkspaceFactory GDBWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace ws = GDBWorkspaceFactory.OpenFromFile(gdbPath, 0);
            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            IFeatureClass fc = fws.OpenFeatureClass(fcName);

            fields = fc.Fields;

            IFeatureClassLoad fcl = fc as IFeatureClassLoad;
            fcl.LoadOnlyMode = true;

            long lineNum = 0;
            long insertNum = 0;
            using (FileStream fs = new FileStream(csvPath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string strLine = "";
                    IFeatureCursor fCursor = fc.Insert(true);
                    IFeatureBuffer fb = fc.CreateFeatureBuffer();
                    string[] line;
                    //FLine_EDGES model_EDGES = new FLine_EDGES();
                    FLine_AREAWATER model_AREAWATER = new FLine_AREAWATER();

                    while ((strLine = sr.ReadLine()) != null)
                    {
                        lineNum++;
                        if (lineNum < startLine) { continue; }

                        //line = SplitAndCheckLine_EDGES(strLine, lineNum);
                        line = SplitAndCheckLine_AREAWATER(strLine, lineNum);

                        if (line == null) { continue; }
                        // model_EDGES = Line2Model(model_EDGES, line);
                        model_AREAWATER = Line2Model(model_AREAWATER, line);

                        //fb = SetFLine2Feature(fb, model_EDGES, lineNum);
                        fb = SetFLine2Feature(fb, model_AREAWATER, lineNum);
                        if (fb == null) { continue; }

                        fCursor.InsertFeature(fb);
                        insertNum++;

                        if (lineNum % 10000 == 0)
                        {
                            fCursor.Flush();
                            total = insertNum;
                        }
                        if (endLine > 0 && lineNum >= endLine)
                        {
                            fCursor.Flush();
                            total = insertNum;
                            break;
                        }
                    }
                }
            }

            fcl.LoadOnlyMode = false;

            Log("Sum:" + lineNum.ToString());
            return total;
        }

        public static void Create(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                fs.Close();
            }
            catch
            {
                throw;
            }
        }

        public static void Append(string path, string text)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Create(path);
                }
                if (text == null)
                    return;
                StreamWriter sw = File.AppendText(path);
                sw.Write(text);
                sw.Close();
            }
            catch
            {
                throw;
            }
        }

        private string[] SplitAndCheckLine_EDGES(string line, long lineNum)
        {
            string[] splits = line.Split('\t');
            if (splits.Length != 32)
            {
                Log("行号[" + lineNum.ToString() + "]，未能正确分割字符." + line);
                return null;
            }
            return splits;
        }

        private string[] SplitAndCheckLine_AREAWATER(string line, long lineNum)
        {
            string[] splits = line.Split('\t');
            if (splits.Length != 9)
            {
                Log("行号[" + lineNum.ToString() + "]，未能正确分割字符." + line);
                return null;
            }
            return splits;
        }

        private void Log(string txt)
        {
            Append(LOG_PATH, txt + "\r\n");
        }

        private FLine_EDGES Line2Model(FLine_EDGES FModel, string[] line)
        {
            FModel.WKT = line[0]; FModel.STATEFP = line[1]; FModel.COUNTYFP = line[2]; FModel.TLID = line[3]; FModel.TFIDL = line[4]; FModel.TFIDR = line[5]; FModel.MTFCC = line[6]; FModel.FULLNAME = line[7]; FModel.SMID = line[8]; FModel.LFROMADD = line[9]; FModel.LTOADD = line[10]; FModel.RFROMADD = line[11]; FModel.RTOADD = line[12]; FModel.ZIPL = line[13]; FModel.ZIPR = line[14]; FModel.FEATCAT = line[15]; FModel.HYDROFLG = line[16]; FModel.RAILFLG = line[17]; FModel.ROADFLG = line[18]; FModel.OLFFLG = line[19]; FModel.PASSFLG = line[20]; FModel.DIVROAD = line[21]; FModel.EXTTYP = line[22]; FModel.TTYP = line[23]; FModel.DECKEDROAD = line[24]; FModel.ARTPATH = line[25]; FModel.PERSIST = line[26]; FModel.GCSEFLG = line[27]; FModel.OFFSETL = line[28]; FModel.OFFSETR = line[29]; FModel.TNIDF = line[30]; FModel.TNIDT = line[31];
            return FModel;
        }

        private FLine_AREAWATER Line2Model(FLine_AREAWATER FModel, string[] line)
        {
            FModel.WKT = line[0];
            FModel.ANSICODE = line[1];
            FModel.HYDROID = line[2];
            FModel.FULLNAME = line[3];
            FModel.MTFCC = line[4];
            FModel.ALAND = line[5];
            FModel.AWATER = line[6];
            FModel.INTPTLAT = line[7];
            FModel.INTPTLON = line[8];
            return FModel;
        }


        //Stopwatch sw = new Stopwatch();
        object missing = Type.Missing;
        string[] data;
        string[] splitWKT_EDGES = new string[] { " (", ")" };
        string[] splitWKT_AREAWATER = new string[] { " ((", "))" };
        string type;
        string cood;
        string[] xys;
        IPoint from = new PointClass();
        IPoint to = new PointClass();
        ILine line = new LineClass();
        double x;
        double y;
        string[] coordi1;
        string[] coordi2;
        IFields fields;
        int fieldIndex = -1;
        Ring ring = new RingClass();

        ISegmentCollection pPath;
        IGeometryCollection pPolyline;

        private IFeatureBuffer SetFLine2Feature(IFeatureBuffer fb, FLine_EDGES model, long lineNum)
        {
            if (model.WKT.Contains("\""))
            {
                model.WKT = model.WKT.Replace("\"", "");
            }
            data = model.WKT.Split(splitWKT_EDGES, StringSplitOptions.None);
            if (data.Length != 3)
            {
                Log("行号[" + lineNum.ToString() + "]，WKT未能正确分割，结果为：" + data.ToString());
                return null;
            }
            type = data[0];
            cood = data[1];

            if (type.ToUpper() != "LINESTRING")
            {
                Log("行号[" + lineNum.ToString() + "]，WKT类型指示的值错误，错误值为：" + type);
                return null;
            }

            xys = cood.Split(',');
            if (xys.Length < 2)
            {
                Log("行号[" + lineNum.ToString() + "]，坐标点太少。" + xys.Length.ToString());
                return null;
            }

            string message = "";
            IGeometry geometry = WKTCoordinateInfo2PolyLine(cood, out message);
            if (geometry == null) {
                Log("行号[" + lineNum.ToString() + "]." + message + "----------" + model.WKT);
            }
            fb.Shape = geometry;

            #region
            fieldIndex = fields.FindField("ARTPATH");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.ARTPATH;
            }
            fieldIndex = fields.FindField("COUNTYFP");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.COUNTYFP;
            }
            fieldIndex = fields.FindField("DECKEDROAD");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.DECKEDROAD;
            }
            fieldIndex = fields.FindField("DIVROAD");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.DIVROAD;
            }
            fieldIndex = fields.FindField("EXTTYP");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.EXTTYP;
            }
            fieldIndex = fields.FindField("FEATCAT");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.FEATCAT;
            }
            fieldIndex = fields.FindField("FULLNAME");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.FULLNAME;
            }
            fieldIndex = fields.FindField("GCSEFLG");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.GCSEFLG;
            }
            fieldIndex = fields.FindField("HYDROFLG");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.HYDROFLG;
            }
            fieldIndex = fields.FindField("LFROMADD");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.LFROMADD;
            }
            fieldIndex = fields.FindField("LTOADD");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.LTOADD;
            }
            fieldIndex = fields.FindField("MTFCC");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.MTFCC;
            }
            fieldIndex = fields.FindField("OFFSETL");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.OFFSETL;
            }
            fieldIndex = fields.FindField("OFFSETR");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.OFFSETR;
            }
            fieldIndex = fields.FindField("OLFFLG");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.OLFFLG;
            }
            fieldIndex = fields.FindField("PASSFLG");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.PASSFLG;
            }
            fieldIndex = fields.FindField("PERSIST");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.PERSIST;
            }
            fieldIndex = fields.FindField("RAILFLG");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.RAILFLG;
            }
            fieldIndex = fields.FindField("RFROMADD");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.RFROMADD;
            }
            fieldIndex = fields.FindField("ROADFLG");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.ROADFLG;
            }
            fieldIndex = fields.FindField("RTOADD");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.RTOADD;
            }
            fieldIndex = fields.FindField("SMID");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.SMID;
            }
            fieldIndex = fields.FindField("STATEFP");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.STATEFP;
            }
            fieldIndex = fields.FindField("TFIDL");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.TFIDL;
            }
            fieldIndex = fields.FindField("TFIDR");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.TFIDR;
            }
            fieldIndex = fields.FindField("TLID");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.TLID;
            }
            fieldIndex = fields.FindField("TNIDF");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.TNIDF;
            }
            fieldIndex = fields.FindField("TNIDT");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.TNIDT;
            }
            fieldIndex = fields.FindField("TTYP");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.TTYP;
            }
            //index = f.Fields.FindField("WKT");
            //if (index >= 0)
            //{
            //    f.Value[index] = model.WKT;
            //}
            fieldIndex = fields.FindField("ZIPL");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.ZIPL;
            }
            fieldIndex = fields.FindField("ZIPR");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.ZIPR;
            }
            #endregion

            return fb;
        }

        private IFeatureBuffer SetFLine2Feature(IFeatureBuffer fb, FLine_AREAWATER model, long lineNum)
        {
            if (model.WKT.Contains("\""))
            {
                model.WKT = model.WKT.Replace("\"", "");
            }
            data = model.WKT.Split(splitWKT_AREAWATER, StringSplitOptions.None);
            if (data.Length != 3)
            {
                Log("行号[" + lineNum.ToString() + "]，WKT未能正确分割，结果为：" + model.WKT);
                return null;
            }
            type = data[0];
            cood = data[1];

            if (type.ToUpper() != "POLYGON")
            {
                Log("行号[" + lineNum.ToString() + "]，WKT类型指示的值错误，错误值为：" + model.WKT);
                return null;
            }
            string message;
            IGeometry geometry = WKTCoordinateInfo2Polygon(cood, out message);
            if (geometry == null)
            {
                Log("行号[" + lineNum.ToString() + "]." + message + "----------" + model.WKT);
                return null;
            }
            fb.Shape = geometry;

            #region
            fieldIndex = fields.FindField("ALAND");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.ALAND;
            }
            fieldIndex = fields.FindField("ANSICODE");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.ANSICODE;
            }
            fieldIndex = fields.FindField("AWATER");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.AWATER;
            }
            fieldIndex = fields.FindField("FULLNAME");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.FULLNAME;
            }
            fieldIndex = fields.FindField("HYDROID");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.HYDROID;
            }
            fieldIndex = fields.FindField("INTPTLAT");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.INTPTLAT;
            }
            fieldIndex = fields.FindField("INTPTLON");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.INTPTLON;
            }
            fieldIndex = fields.FindField("MTFCC");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = model.MTFCC;
            }
            #endregion

            return fb;
        }

        #region WKT坐标生成线、面

        string[] _fromStr;
        string[] _toStr;
        double _x;
        double _y;
        IPoint _fromPoint = new PointClass();
        IPoint _toPoint = new PointClass();
        ILine _line = new LineClass();
        private IGeometry WKTCoordinateInfo2PolyLine(string WKTCoor, out string message)
        {
            string[] split = WKTCoor.Split(',');
            if (split.Length < 2)
            {
                message = "坐标点太少." + WKTCoor;
                return null;
            }
            ISegmentCollection path = new PathClass();
            int len = xys.Length - 1;
            for (int i = 0; i < len; i += 2)
            {
                _fromStr = xys[i].Split(' ');
                _toStr = xys[i + 1].Split(' ');
                if (_fromStr.Length != 2 || _toStr.Length != 2)
                {
                    message = "坐标点不能正确分割." + WKTCoor;
                    return null;
                }
                if (!double.TryParse(_fromStr[0], out _x) || !double.TryParse(_fromStr[1], out _y))
                {
                    message = "坐标点不能转换为double." + WKTCoor;
                    return null;
                }
                _fromPoint.X = _x;
                _fromPoint.Y = _y;
                if (!double.TryParse(_toStr[0], out _x) || !double.TryParse(_toStr[1], out _y))
                {
                    message = "坐标点不能转换为double." + WKTCoor;
                    return null;
                }
                _toPoint.X = _x;
                _toPoint.Y = _y;
                _line.PutCoords(from, to);
                path.AddSegment((ISegment)_line, missing, missing);
            }
            IGeometryCollection polyLine = new PolylineClass();
            polyLine.AddGeometry((IGeometry)path, missing, missing);
            message = "";
            IGeometry geometry = polyLine as IGeometry; ;
            return geometry;
        }

        string[] _coorStr;
        IPoint _point = new PointClass();
        private IGeometry WKTCoordinateInfo2Polygon(string WKTCoor, out string message)
        {
            string[] polygons;
            if (WKTCoor.Contains("),("))
            {
                polygons = WKTCoor.Split(new string[] { "),(" }, StringSplitOptions.RemoveEmptyEntries);
            }
            else {
                polygons = new string[] { WKTCoor };
            }
            IGeometryCollection polygon = new PolygonClass();
            foreach (var item in polygons)
            {
                string[] split = item.Split(',');
                if (split.Length < 3)
                {
                    message = "坐标点太少." + WKTCoor;
                    return null;
                }
                Ring ring = new RingClass();
                int len = split.Length;
                for (int i = 0; i < len; i++)
                {
                    _coorStr = split[i].Split(' ');
                    if (_coorStr.Length != 2)
                    {
                        message = "坐标点不能正确分割." + WKTCoor;
                        return null;
                    }
                    if (!double.TryParse(_coorStr[0], out _x) || !double.TryParse(_coorStr[1], out _y))
                    {
                        message = "坐标点不能转换为double." + WKTCoor;
                        return null;
                    }
                    _point.PutCoords(x, y);
                    ring.AddPoint(_point, ref missing, ref missing);
                }
                polygon.AddGeometry(ring as IGeometry, ref missing, ref missing);
            }
            IPolygon poly = polygon as IPolygon;
            poly.SimplifyPreserveFromTo();
            IGeometry geometry = poly as IGeometry;
            message = "";
            return geometry;
        }

        #endregion






        private int InterSect(string incomeGDB, string incomeFNC, string intersectGDB, string intersectFNC, long intersectObjectID)
        {
            IWorkspaceFactory GDBWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace incomeWs = GDBWorkspaceFactory.OpenFromFile(incomeGDB, 0);
            IFeatureWorkspace incomeFws = incomeWs as IFeatureWorkspace;
            IFeatureClass incomeFC = incomeFws.OpenFeatureClass(incomeFNC);

            //IWorkspace intersectWs = GDBWorkspaceFactory.OpenFromFile(intersectGDB, 0);
            //IFeatureWorkspace intersectFws = intersectWs as IFeatureWorkspace;
            //IFeatureClass intersectFc = intersectFws.OpenFeatureClass(intersectFNC);
            //IFeature intersectF = intersectFc.GetFeature(int.Parse(intersectObjectID.ToString()));

            IWorkspaceFactory ShpWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace ws = ShpWorkspaceFactory.OpenFromFile(@"D:\Data\buffershp\buffershp", 0);

            IFeatureWorkspace intersectFws = ws as IFeatureWorkspace;
            IFeatureClass intersectFc_bufferline = intersectFws.OpenFeatureClass("buferline");
            IFeatureClass intersectFc_buffer2km = intersectFws.OpenFeatureClass("buf_2km");
            IFeatureClass intersectFc_buffer20km = intersectFws.OpenFeatureClass("buf_20km");

            IFeature intersectF_bufferLine = intersectFc_bufferline.GetFeature(0);
            IFeature intersectF_buffer2km = intersectFc_buffer2km.GetFeature(0);
            IFeature intersectF_buffer20km = intersectFc_buffer20km.GetFeature(0);

            Stopwatch sw = new Stopwatch();

            //==================
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = intersectF_bufferLine.Shape;
            spatialFilter.GeometryField = intersectFc_bufferline.ShapeFieldName;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            Log("buferline");
            sw.Restart();
            IFeatureCursor featureCursor = incomeFC.Search(spatialFilter, false);
            sw.Stop();
            Log("Search:" + sw.ElapsedMilliseconds.ToString());

            sw.Restart();
            IFeature feature = null;
            int count = 0;
            while ((feature = featureCursor.NextFeature()) != null)
            {
                count++;
            }
            sw.Stop();
            Log("Count:" + count.ToString() + "-----" + sw.ElapsedMilliseconds.ToString());

            //==================
            spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = intersectF_buffer2km.Shape;
            spatialFilter.GeometryField = intersectFc_buffer2km.ShapeFieldName;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            Log("buf_2km");
            sw.Restart();
            featureCursor = incomeFC.Search(spatialFilter, false);
            sw.Stop();
            Log("Search:" + sw.ElapsedMilliseconds.ToString());

            sw.Restart();
            feature = null;
            count = 0;
            while ((feature = featureCursor.NextFeature()) != null)
            {
                count++;
            }
            sw.Stop();
            Log("Count:" + count.ToString() + "-----" + sw.ElapsedMilliseconds.ToString());

            //==================
            spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = intersectF_buffer20km.Shape;
            spatialFilter.GeometryField = intersectFc_buffer20km.ShapeFieldName;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            Log("buf_20km");
            sw.Restart();
            featureCursor = incomeFC.Search(spatialFilter, false);
            sw.Stop();
            Log("Search:" + sw.ElapsedMilliseconds.ToString());

            sw.Restart();
            feature = null;
            count = 0;
            while ((feature = featureCursor.NextFeature()) != null)
            {
                count++;
            }
            sw.Stop();
            Log("Count:" + count.ToString() + "-----" + sw.ElapsedMilliseconds.ToString());

            return count;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetLog();
            DateTime startTime = DateTime.Now;
            string gdb = txtGDB.Text;
            string fcn = txtFCN.Text;
            long start = long.Parse(txtStart.Text);
            long end = long.Parse(txtEnd.Text);

            ReadCSV(@"E:\SpatialHadoop\AREAWATER.csv\AREAWATER.csv", gdb, fcn, start, end);

            DateTime endTime = DateTime.Now;
            MessageBox.Show("Finish。" + startTime.ToLongTimeString() + "   " + endTime.ToLongTimeString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string inComeGDBPath = txtIncomeGDBPath.Text;
            string inComeFNC = txtIncomeFCN.Text;

            string gdbPath = txtGDB2.Text;
            string FCN = txtFCN2.Text;
            long oid = long.Parse(txtObjectID.Text);
            SetLog();
            int count = InterSect(inComeGDBPath, inComeFNC, gdbPath, FCN, oid);
        }
    }

    public class FLine_EDGES
    {
        public string WKT { get; set; }
        public string STATEFP { get; set; }
        public string COUNTYFP { get; set; }
        public string TLID { get; set; }
        public string TFIDL { get; set; }
        public string TFIDR { get; set; }
        public string MTFCC { get; set; }
        public string FULLNAME { get; set; }
        public string SMID { get; set; }
        public string LFROMADD { get; set; }
        public string LTOADD { get; set; }
        public string RFROMADD { get; set; }
        public string RTOADD { get; set; }
        public string ZIPL { get; set; }
        public string ZIPR { get; set; }
        public string FEATCAT { get; set; }
        public string HYDROFLG { get; set; }
        public string RAILFLG { get; set; }
        public string ROADFLG { get; set; }
        public string OLFFLG { get; set; }
        public string PASSFLG { get; set; }
        public string DIVROAD { get; set; }
        public string EXTTYP { get; set; }
        public string TTYP { get; set; }
        public string DECKEDROAD { get; set; }
        public string ARTPATH { get; set; }
        public string PERSIST { get; set; }
        public string GCSEFLG { get; set; }
        public string OFFSETL { get; set; }
        public string OFFSETR { get; set; }
        public string TNIDF { get; set; }
        public string TNIDT { get; set; }
    }

    public class FLine_AREAWATER
    {
        public string WKT { get; set; }
        public string ANSICODE { get; set; }
        public string HYDROID { get; set; }
        public string FULLNAME { get; set; }
        public string MTFCC { get; set; }
        public string ALAND { get; set; }
        public string AWATER { get; set; }
        public string INTPTLAT { get; set; }
        public string INTPTLON { get; set; }

    }





}
