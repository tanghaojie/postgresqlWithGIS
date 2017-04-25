using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGDis
{
    public class Input
    {
        string LOG_PATH = @"E:\SpatialHadoop\EDGES.csv\Log_EDGES.gdb.txt";

        public long ReadCSV(string csvPath, string gdbPath, string fcName, long startLine, long endLine)
        {
            Log("StartTime:" + DateTime.Now.ToLongTimeString());
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
                    FLine_EDGES model_EDGES = new FLine_EDGES();

                    while ((strLine = sr.ReadLine()) != null)
                    {
                        lineNum++;
                        if (lineNum < startLine) { continue; }

                        line = SplitAndCheckLine_EDGES(strLine, lineNum);

                        if (line == null) { continue; }
                        model_EDGES = Line2Model(model_EDGES, line);

                        fb = SetFLine2Feature(fb, model_EDGES, lineNum);
                        if (fb == null) { continue; }

                        fCursor.InsertFeature(fb);
                        insertNum++;

                        if (lineNum % 10000 == 0)
                        {
                            fCursor.Flush();
                            total = insertNum;

                            Log("Flush["+ lineNum + "]:" + DateTime.Now.ToLongTimeString());
                        }
                        if (endLine > 0 && lineNum >= endLine)
                        {
                            fCursor.Flush();
                            total = insertNum;
                            break;
                        }
                    }
                    fCursor.Flush();
                    total = insertNum;
                }
            }

            fcl.LoadOnlyMode = false;

            Log("Sum:" + lineNum.ToString());
            Log("EndTime:" + DateTime.Now.ToLongTimeString());
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
            FModel.WKT = line[0].Trim('"'); FModel.STATEFP = line[1]; FModel.COUNTYFP = line[2]; FModel.TLID = line[3]; FModel.TFIDL = line[4]; FModel.TFIDR = line[5]; FModel.MTFCC = line[6]; FModel.FULLNAME = line[7]; FModel.SMID = line[8]; FModel.LFROMADD = line[9]; FModel.LTOADD = line[10]; FModel.RFROMADD = line[11]; FModel.RTOADD = line[12]; FModel.ZIPL = line[13]; FModel.ZIPR = line[14]; FModel.FEATCAT = line[15]; FModel.HYDROFLG = line[16]; FModel.RAILFLG = line[17]; FModel.ROADFLG = line[18]; FModel.OLFFLG = line[19]; FModel.PASSFLG = line[20]; FModel.DIVROAD = line[21]; FModel.EXTTYP = line[22]; FModel.TTYP = line[23]; FModel.DECKEDROAD = line[24]; FModel.ARTPATH = line[25]; FModel.PERSIST = line[26]; FModel.GCSEFLG = line[27]; FModel.OFFSETL = line[28]; FModel.OFFSETR = line[29]; FModel.TNIDF = line[30]; FModel.TNIDT = line[31];
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


        IFields fields;
        int fieldIndex = -1;

        static ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
        ISpatialReference spatialReference = spatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

        private IFeatureBuffer SetFLine2Feature(IFeatureBuffer fb, FLine_EDGES model, long lineNum)
        {
            IGeometry geo = model.WKT.ToGeometry(spatialReference);
            if (geo == null) {
                return null;
            }
            fb.Shape = geo;

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
            fieldIndex = fields.FindField("ID");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = lineNum;
            }
            #endregion

            return fb;
        }

        private IFeatureBuffer SetFLine2Feature(IFeatureBuffer fb, FLine_AREAWATER model, long lineNum)
        {
            
            fb.Shape = model.WKT.ToGeometry(spatialReference); ;

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
            fieldIndex = fields.FindField("ID");
            if (fieldIndex >= 0)
            {
                fb.Value[fieldIndex] = lineNum;
            }
            #endregion

            return fb;
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
