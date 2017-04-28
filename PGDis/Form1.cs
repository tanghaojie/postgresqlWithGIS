﻿using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;
using Npgsql;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.ObjectModel;
using System.IO;

namespace PGDis {
    //
    public partial class Form1 : Form {
        //Local gdb
        //readonly string EDGES_GdbPath = @"D:\Data\EDGES.gdb";
        //readonly string EDGES_FeatureClassName = "EDGES";
        static readonly string EDGES_GdbPath = ConfigurationManager.AppSettings["GdbPath"];
        static readonly string EDGES_FeatureClassName = ConfigurationManager.AppSettings["GdbFeatureClassName"];

        //my define show gdb
        static readonly string MyDefineGdbPath = ConfigurationManager.AppSettings["UserGdbPath"];
        static readonly string MyDefineGdbFcName = ConfigurationManager.AppSettings["UserGdbFcName"];

        //Pg
        //const string HOST = "192.168.1.100";
        //const int PORT = 5432;
        //const string USER = "postgres";
        //const string PASSWORD = "admin";
        //const string DB = "postgis_rcl";
        //const string TABLENAME = "edges";
        static readonly string HOST = ConfigurationManager.AppSettings["PgHost"];
        static readonly string PORT = ConfigurationManager.AppSettings["PgPort"];
        static readonly string USER = ConfigurationManager.AppSettings["PgUser"];
        static readonly string PASSWORD = ConfigurationManager.AppSettings["PgPass"];
        static readonly string DB = ConfigurationManager.AppSettings["PgDb"];
        static readonly string TABLENAME = ConfigurationManager.AppSettings["PgTableName"];

        //server
        static readonly string ServerLayerUrl = ConfigurationManager.AppSettings["ServerLayerUrl"];
        static readonly string ServerLayerName = ConfigurationManager.AppSettings["ServerName"];

        //Log Path
        static readonly string LogPath = ConfigurationManager.AppSettings["LogPath"];

        static string connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
                HOST, PORT, USER, PASSWORD, DB);

        string DEFAULT_GDBPATH = Application.StartupPath + @"\temp.gdb";
        const string DEFAULT_DRAWPOLYGONNAME = "DrawPolygon";
        const string DEFAULT_DRAWPOLYLINENAME = "DrawPolyline";
        string DEFAULT_TEMPGDBPATH = Application.StartupPath + @"\temp2.gdb";
        const string POLYGONNAME = "Polygon";
        const string POLYLINENAME = "Polyline";

        IFeatureLayer Fl_DrawPolygon;
        IFeatureLayer Fl_DrawPolyline;
        IWorkspaceEdit Wse_Draw;
        esriControlsMousePointer Default_Pointer;

        IWorkspaceFactory GDBWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
        DrawMode drawMode;
        private enum DrawMode {
            Polygon = 0,
            Polyline = 1
        }

        public Form1() {
            InitializeComponent();
            //server
            if (!string.IsNullOrEmpty(ServerLayerUrl) && !string.IsNullOrEmpty(ServerLayerName)) {
                OpenServer os = new OpenServer();
                ILayer layer = os.GetServerLyr(ServerLayerUrl, ServerLayerName, false);
                axMapControl.AddLayer(layer);
            }
            //myUS
            if (!string.IsNullOrEmpty(MyDefineGdbPath) && !string.IsNullOrEmpty(MyDefineGdbFcName)) {
                IFeatureClass us = OpenGdbFeatureClass(MyDefineGdbPath, MyDefineGdbFcName);
                IFeatureLayer fl_us = new FeatureLayerClass();
                fl_us.FeatureClass = us;
                axMapControl.AddLayer(fl_us);
            }
            //default draw polygon polyline gdb file
            IWorkspace ws = GDBWorkspaceFactory.OpenFromFile(DEFAULT_GDBPATH, 0);
            Wse_Draw = ws as IWorkspaceEdit;

            IFeatureClass Fc_DrawPolygon = (ws as IFeatureWorkspace).OpenFeatureClass(DEFAULT_DRAWPOLYGONNAME);
            Fl_DrawPolygon = new FeatureLayerClass();
            Fl_DrawPolygon.FeatureClass = Fc_DrawPolygon;
            axMapControl.AddLayer(Fl_DrawPolygon);

            IFeatureClass Fc_DrawPolyline = (ws as IFeatureWorkspace).OpenFeatureClass(DEFAULT_DRAWPOLYLINENAME);
            Fl_DrawPolyline = new FeatureLayerClass();
            Fl_DrawPolyline.FeatureClass = Fc_DrawPolyline;
            axMapControl.AddLayer(Fl_DrawPolyline);
            //default mouse pointer
            Default_Pointer = esriControlsMousePointer.esriPointerDefault;
            axMapControl.MousePointer = Default_Pointer;
            axMapControl.MouseIcon = Properties.Resources.EditingEditTool;
        }

        private void axMapControl_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e) {
            int btn = e.button;
            SetAxMapControlMousePointer(btn);
            Perform(e.mapX, e.mapY);
        }

        private void SetAxMapControlMousePointer(int btn) {
            if (btn == 1) {
                if (!Wse_Draw.IsBeingEdited()) {
                    axMapControl.MousePointer = esriControlsMousePointer.esriPointerCustom;
                }
            }
            else if (btn == 4) {
                axMapControl.MousePointer = esriControlsMousePointer.esriPointerPan;
            }
            else {
                axMapControl.MousePointer = Default_Pointer;
            }
        }

        private void Perform(double x, double y) {
            if (axMapControl.MousePointer == esriControlsMousePointer.esriPointerPan) {
                MapMove();
            }
            else if (axMapControl.MousePointer == esriControlsMousePointer.esriPointerPencil) {
                if (drawMode == DrawMode.Polygon) {
                    DrawPolygon();
                }
                else if (drawMode == DrawMode.Polyline) {
                    DrawPolyline();
                }
            }
            else if (axMapControl.MousePointer == esriControlsMousePointer.esriPointerCustom) {
                Select(x, y);
            }
        }

        private void MapMove() {
            axMapControl.Pan();
            axMapControl.MousePointer = Default_Pointer;
        }

        private void DrawPolygon() {
            IGeometry polygon = TrackPolygon();
            IFeature f = Fl_DrawPolygon.FeatureClass.CreateFeature();
            f.Shape = polygon;
            f.Store();
            axMapControl.MousePointer = Default_Pointer;
            axMapControl.Refresh();
        }

        private void DrawPolyline() {
            IGeometry polygon = TrackPolyline();
            IFeature f = Fl_DrawPolyline.FeatureClass.CreateFeature();
            f.Shape = polygon;
            f.Store();
            axMapControl.MousePointer = Default_Pointer;
            axMapControl.Refresh();
        }

        private void Select(double x, double y) {
            IGeometry geo = axMapControl.TrackRectangle();
            axMapControl.Map.SelectByShape(geo, null, true);
            axMapControl.Refresh();
        }

        private IGeometry TrackPolygon() {
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
            IGeometry geo = axMapControl.TrackPolygon();
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
            return geo;
        }

        private IGeometry TrackPolyline() {
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
            IGeometry geo = axMapControl.TrackLine();
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
            return geo;
        }

        private void btnEdit_Click(object sender, EventArgs e) {
            if (Wse_Draw.IsBeingEdited()) {
                Wse_Draw.StopEditOperation();
                Wse_Draw.StopEditing(true);
                btnEdit.Text = "开始编辑";
                btnDrawPolygon.Enabled = false;
                btnDrawPolyline.Enabled = false;
                delF.Enabled = false;
                Default_Pointer = esriControlsMousePointer.esriPointerDefault;
                axMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
                axMapControl.Map.ClearSelection();
            }
            else {
                Wse_Draw.StartEditing(false);
                Wse_Draw.StartEditOperation();
                btnEdit.Text = "结束编辑";
                btnDrawPolygon.Enabled = true;
                btnDrawPolyline.Enabled = true;
                delF.Enabled = true;
                Default_Pointer = esriControlsMousePointer.esriPointerCustom;
                axMapControl.MousePointer = esriControlsMousePointer.esriPointerCustom;
            }
        }

        private void btnSelect_Click(object sender, EventArgs e) {
            if (Wse_Draw.IsBeingEdited()) {
                btnDrawPolygon.Enabled = true;
                btnDrawPolyline.Enabled = true;
            }
            Default_Pointer = esriControlsMousePointer.esriPointerCustom;
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerCustom;
        }

        private void btnDrawPolygon_Click(object sender, EventArgs e) {
            btnDrawPolygon.Enabled = false;
            btnDrawPolyline.Enabled = true;
            Default_Pointer = esriControlsMousePointer.esriPointerPencil;
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
            drawMode = DrawMode.Polygon;
        }

        private void btnDrawPolyline_Click(object sender, EventArgs e) {
            btnDrawPolygon.Enabled = true;
            btnDrawPolyline.Enabled = false;
            Default_Pointer = esriControlsMousePointer.esriPointerPencil;
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
            drawMode = DrawMode.Polyline;
        }

        private void delF_Click(object sender, EventArgs e) {
            IFeature fea = GetSelectedFeature();
            if (fea != null) {
                fea.Delete();
            }

            axMapControl.Refresh();
        }

        private IFeature GetSelectedFeature() {
            IFeatureSelection fs = Fl_DrawPolygon as IFeatureSelection;
            IEnumIDs ids = fs.SelectionSet.IDs;
            int id = -1;
            while ((id = ids.Next()) > 0) {
                return Fl_DrawPolygon.FeatureClass.GetFeature(id);
            }

            fs = Fl_DrawPolyline as IFeatureSelection;
            ids = fs.SelectionSet.IDs;
            id = -1;
            while ((id = ids.Next()) > 0) {
                return Fl_DrawPolyline.FeatureClass.GetFeature(id);
            }
            return null;
        }

        private int SearchCount_ArcGIS(IFeatureClass incomeFc, ISpatialFilter sf) {
            IFeatureCursor featureCursor = incomeFc.Search(sf, false);
            IFeature feature = null;
            int count = 0;
            while ((feature = featureCursor.NextFeature()) != null) {
                count++;
            }
            return count;
        }

        private DataSet Execute_PG(string sql) {
            DataSet ds = new DataSet();
            using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) {
                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd)) {
                        da.Fill(ds);
                    }
                }
            }
            return ds;
        }

        private NpgsqlDataReader ExecuteReader_PG(string sql) {
            NpgsqlConnection conn = new NpgsqlConnection(connString);
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }

        private ISpatialFilter SetSpatialFilter(IFeature intersectF, esriSpatialRelEnum spatialRelEnum = esriSpatialRelEnum.esriSpatialRelIntersects) {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = intersectF.Shape;
            spatialFilter.GeometryField = (intersectF.Table as IFeatureClass).ShapeFieldName;
            spatialFilter.SpatialRel = spatialRelEnum;
            return spatialFilter;
        }

        private void btnIntersect_Click(object sender, EventArgs e) {
            try {
                dgv.Rows.Clear();
                IFeature fea = GetSelectedFeature();
                if (fea == null) {
                    MessageBox.Show("未选中任何要素");
                    return;
                }
                AddDgvSplitFlag();
                SetProgress("开始执行空间相交...", 10);
                AddDgvRow("空间相交", "", "");

                ISpatialFilter spatialFilter = SetSpatialFilter(fea);
                IGeometry geo = fea.Shape;
                string wkt = geo.ToWellKnownText();
                string sType = "";
                string sArcgis = "";
                string sPG = "";
                if (geo.GeometryType == esriGeometryType.esriGeometryPolygon) {
                    IArea area = geo as IArea;
                    if (area != null) {
                        double b = Math.Abs(area.Area);
                        sType = "相交要素面积";
                        sArcgis = sPG = b.ToString();
                    }
                }
                else if (geo.GeometryType == esriGeometryType.esriGeometryPolyline) {
                    IPolyline line = geo as IPolyline;
                    if (line != null) {
                        double b = line.Length;
                        sType = "相交要素长度";
                        sArcgis = sPG = b.ToString();
                    }
                }
                if (!string.IsNullOrEmpty(sType) && !string.IsNullOrEmpty(sArcgis) && !string.IsNullOrEmpty(sPG)) {
                    AddDgvRow(sType, sArcgis, sPG);
                }
                AddDgvRow("相交结果数", "", "");
                AddDgvRow("时间（秒）", "", "");

                IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

                Stopwatch sw = new Stopwatch();
                sw.Restart();
                //arcgis 
                SetProgress("ArcGIS空间相交，正在执行...", 40);
                int intersectCount_arcgis = SearchCount_ArcGIS(fc, spatialFilter);

                sw.Stop();
                long time_arcgis = sw.ElapsedMilliseconds;

                SetProgress("PG空间相交，正在执行...", 80);
                AppendText("相交结果数", intersectCount_arcgis.ToString(), null);
                AppendText("时间（秒）", (time_arcgis / 1000.0).ToString(".000"), null);
                sw.Restart();

                //pg
                //DataSet ds = Execute_PG(GetIntersectCountSql(wkt));
                //DataTable dt = null;
                //if (ds.Tables.Count > 0) {
                //    dt = ds.Tables[0];
                //    if (dt.Rows.Count > 0) {
                //        object obj = dt.Rows[0][0];
                //        if (obj != DBNull.Value && obj != null) {
                //            intersectCount_pg = int.Parse(obj.ToString());
                //        }
                //    }
                //}

                //int intersectCount_pg = 0;
                //DataSet ds = Execute_PG(GetIntersectSql(wkt));
                //DataTable dt = null;
                //if (ds.Tables.Count > 0) {
                //    dt = ds.Tables[0];
                //    intersectCount_pg = dt.Rows.Count;
                //}

                int intersectCount_pg = 0;
                NpgsqlDataReader reader = ExecuteReader_PG(GetIntersectSql(wkt));
                DataTable dt = new DataTable();
                DataColumn col;
                int fCount = reader.FieldCount;
                for (int i = 0; i < fCount; i++) {
                    col = new DataColumn();
                    col.ColumnName = reader.GetName(i);
                    col.DataType = reader.GetFieldType(i);
                    dt.Columns.Add(col);
                }
                DataRow row;
                string fName;
                while (reader.Read()) {
                    if (intersectCount_pg <= 50) {
                        row = dt.NewRow();
                        for (int i = 0; i < fCount; i++) {
                            fName = dt.Columns[i].ColumnName;
                            row[fName] = reader[fName];
                        }
                        dt.Rows.Add(row);
                    }
                    intersectCount_pg++;
                }
                reader.Close();

                sw.Stop();
                long time_pg = sw.ElapsedMilliseconds;
                AppendText("相交结果数", null, intersectCount_pg.ToString());
                AppendText("时间（秒）", null, (time_pg / 1000.0).ToString(".000"));

                SetProgress("空间相交执行完成", 100);
                AddDgvSplitFlag();
                if (dt != null) {
                    AttributeForm af = new AttributeForm(dt);
                    af.Show();
                }
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void AppendText(string funRowName, string arcgis, string pg) {
            foreach (DataGridViewRow row in dgv.Rows) {
                object value = row.Cells["C"].Value; ;
                if (value == null || value == DBNull.Value) {
                    continue;
                }
                string s = value.ToString();
                if (s.ToUpper() == funRowName.ToUpper()) {
                    if (!string.IsNullOrEmpty(arcgis)) {
                        row.Cells["CARCGIS"].Value = arcgis;
                    }
                    if (!string.IsNullOrEmpty(pg)) {
                        row.Cells["CPG"].Value = pg;
                    }
                }
            }
            Application.DoEvents();
        }

        private IFeatureClass OpenGdbFeatureClass(string gdbPath, string fcName) {
            IWorkspace ws = GDBWorkspaceFactory.OpenFromFile(gdbPath, 0);
            return (ws as IFeatureWorkspace).OpenFeatureClass(fcName);
        }

        private void Clip(IFeatureClass inFc, IFeatureClass clipFc, string savePath) {
            Geoprocessor g = new Geoprocessor();
            g.OverwriteOutput = true;
            ESRI.ArcGIS.AnalysisTools.Clip clip = new ESRI.ArcGIS.AnalysisTools.Clip(inFc, clipFc, savePath);
            g.Execute(clip, null);
        }

        private void ExportFeature(IFeatureClass fc, IFeature f) {
            IFeature newF = fc.CreateFeature();
            newF.Shape = f.Shape;
            IFields fields = f.Table.Fields;
            int count = fields.FieldCount;
            int index;
            for (int i = 0; i < count; i++) {
                string name = fields.Field[i].Name;
                if (name.ToUpper().Contains("OBJECTID") || name.ToUpper().Contains("SHAPE")) {
                    continue;
                }
                index = fc.FindField(name);
                if (index > 0) {
                    newF.Value[index] = f.Value[i];
                }
            }
            newF.Store();
        }

        private void ExportFeaturePG(IFeatureClass fc, DataSet ds) {
            DataTable dt = ds.Tables[0];
            int count = dt.Rows.Count;
            int fCount = dt.Columns.Count;
            for (int i = 0; i < count; i++) {
                DataRow row = dt.Rows[i];
                IFeature f = fc.CreateFeature();
                int index;
                for (int j = 0; j < fCount; j++) {
                    string cName = dt.Columns[j].ColumnName;
                    if (cName.ToUpper() == "WKT") {
                        object obj = row[cName];
                        if (obj != null && obj != DBNull.Value) {
                            string wkt = obj.ToString();
                            ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                            ISpatialReference spatialReference = spatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                            IGeometry geo = wkt.ToGeometry(spatialReference);
                            if (geo == null) {

                            }
                            else {
                                f.Shape = geo;
                            }

                        }
                        else {

                        }
                    }

                    index = fc.FindField(cName);
                    if (cName.ToUpper() == "ID") {
                        index = fc.FindField("XID");
                    }
                    if (index > 0) {
                        f.Value[index] = row[cName];
                    }
                }
                f.Store();
            }
        }

        private string GetIntersectCountSql(string wkt) {
            return "SELECT count(\"id\") FROM " + TABLENAME + " WHERE ST_Intersects(geom,ST_GeomFromText('" + wkt + "',4326))";
            //return "SELECT *,st_astext(geom) as wkt FROM EDGES_test WHERE ST_Intersects(geom,ST_GeomFromText('" + wkt + "',4326))";
        }

        private string GetIntersectSql(string wkt) {
            return "SELECT *,st_astext(geom) FROM " + TABLENAME + " WHERE ST_Intersects(geom,ST_GeomFromText('" + wkt + "',4326))";
        }

        private string GetIntersectionSql(string wkt) {
            return "SELECT *,ST_AsText(ST_Intersection(geom,ST_GeomFromText('" + wkt + "', 4326) )) AS clip FROM " + TABLENAME + " WHERE ST_Intersects( geom,ST_GeomFromText('" + wkt + "', 4326) )";
        }

        private string GetIntersectionCountAndSumLengthSql(string wkt) {
            return "SELECT count(*),sum(st_length(ST_AsText(ST_Intersection(geom,ST_GeomFromText('" + wkt + "', 4326) )))) FROM " + TABLENAME + " WHERE ST_Intersects( geom,ST_GeomFromText('" + wkt + "', 4326) )";
        }

        private string GetAllWhereSql(string whereSql) {
            return "SELECT * FROM " + TABLENAME + " WHERE " + whereSql;
        }

        private void btnClip_Click(object sender, EventArgs e) {
            try {
                dgv.Rows.Clear();

                IFeature fea = GetSelectedFeature();
                if (fea == null) {
                    MessageBox.Show("未选中任何要素");
                    return;
                }
                IGeometry geo = fea.Shape;
                string wkt = geo.ToWellKnownText();

                if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
                    MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
                    return;
                }
                string sArcgis = "";
                string sPG = "";
                IArea area = geo as IArea;
                if (area != null) {
                    double b = Math.Abs(area.Area);
                    sArcgis = sPG = b.ToString();
                }

                SetProgress("开始执行裁剪...", 10);
                AddDgvSplitFlag();
                AddDgvRow("裁剪统计", "", "");
                if (!string.IsNullOrEmpty(sArcgis) && !string.IsNullOrEmpty(sPG)) {
                    AddDgvRow("裁剪面积", sArcgis, sPG);
                }

                IFeatureClass clipFC = OpenGdbFeatureClass(DEFAULT_TEMPGDBPATH, POLYGONNAME);
                ClearFc(clipFC);
                IFeature clipF = clipFC.CreateFeature();
                clipF.Shape = geo;
                clipF.Store();

                IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

                Stopwatch sw = new Stopwatch();
                SetProgress("ArcGIS裁剪，正在执行...", 40);

                AddDgvRow("结果数", "", "");
                AddDgvRow("时间（秒）", "", "");
                AddDgvRow("裁剪结果总长", "", "");

                sw.Restart();
                //arcgis
                string saveName = "clip" + DateTime.Now.ToFileTime();
                Clip(fc, clipFC, DEFAULT_TEMPGDBPATH + @"\" + saveName);
                IFeatureClass saveFc = OpenGdbFeatureClass(DEFAULT_TEMPGDBPATH, saveName);
                double totalLength_arcgis = 0;
                IFeatureCursor fcursor = saveFc.Search(null, false);
                IFeature f = null;
                int index = saveFc.FindField("SHAPE_LENGTH");
                if (index > 0) {
                    while ((f = fcursor.NextFeature()) != null) {
                        object value = f.Value[index];
                        if (value != DBNull.Value && value != null) {
                            string sValue = value.ToString();
                            totalLength_arcgis += double.Parse(f.Value[index].ToString());
                        }
                    }
                }
                int count_arcgis = saveFc.FeatureCount(null);

                sw.Stop();
                long time_arcgis = sw.ElapsedMilliseconds;
                AppendText("结果数", count_arcgis.ToString(), null);
                AppendText("时间（秒）", (time_arcgis / 1000.0).ToString(".000"), null);
                AppendText("裁剪结果总长", totalLength_arcgis.ToString(), null);

                //pg
                SetProgress("PG裁剪，正在执行...", 80);
                sw.Restart();
                DataSet ds = Execute_PG(GetIntersectionCountAndSumLengthSql(wkt));
                int count_pg = 0;
                double totalLength_pg = 0;
                if (ds.Tables.Count > 0) {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0) {
                        count_pg = int.Parse(dt.Rows[0][0].ToString());
                        totalLength_pg = double.Parse(dt.Rows[0][1].ToString());
                    }
                }

                sw.Stop();
                long time_pg = sw.ElapsedMilliseconds;
                AppendText("结果数", "", count_pg.ToString());
                AppendText("时间（秒）", "", (time_pg / 1000.0).ToString(".000"));
                AppendText("裁剪结果总长", "", totalLength_pg.ToString());

                SetProgress("裁剪完成...", 100);
                AddDgvSplitFlag();
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void btnOnlyClip_Click(object sender, EventArgs e) {
            try {
                dgv.Rows.Clear();

                IFeature fea = GetSelectedFeature();
                if (fea == null) {
                    MessageBox.Show("未选中任何要素");
                    return;
                }
                IGeometry geo = fea.Shape;
                string wkt = geo.ToWellKnownText();

                if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
                    MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
                    return;
                }
                string sArcgis = "";
                string sPG = "";
                IArea area = geo as IArea;
                if (area != null) {
                    double b = Math.Abs(area.Area);
                    sArcgis = sPG = b.ToString();
                }

                SetProgress("开始执行裁剪...", 10);
                AddDgvSplitFlag();
                AddDgvRow("裁剪", "", "");
                if (!string.IsNullOrEmpty(sArcgis) && !string.IsNullOrEmpty(sPG)) {
                    AddDgvRow("裁剪面积", sArcgis, sPG);
                }

                IFeatureClass clipFC = OpenGdbFeatureClass(DEFAULT_TEMPGDBPATH, POLYGONNAME);
                ClearFc(clipFC);
                IFeature clipF = clipFC.CreateFeature();
                clipF.Shape = geo;
                clipF.Store();

                IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

                Stopwatch sw = new Stopwatch();
                SetProgress("ArcGIS裁剪，正在执行...", 40);

                AddDgvRow("结果数", "", "");
                AddDgvRow("时间（秒）", "", "");

                sw.Restart();
                //arcgis
                string saveName = "clip" + DateTime.Now.ToFileTime();
                Clip(fc, clipFC, DEFAULT_TEMPGDBPATH + @"\" + saveName);
                IFeatureClass saveFc = OpenGdbFeatureClass(DEFAULT_TEMPGDBPATH, saveName);
                //double totalLength_arcgis = 0;
                //IFeatureCursor fcursor = saveFc.Search(null, false);
                //IFeature f = null;
                //int index = saveFc.FindField("SHAPE_LENGTH");
                //if (index > 0) {
                //    while ((f = fcursor.NextFeature()) != null) {
                //        object value = f.Value[index];
                //        if (value != DBNull.Value && value != null) {
                //            string sValue = value.ToString();
                //            totalLength_arcgis += double.Parse(f.Value[index].ToString());
                //        }
                //    }
                //}
                int count_arcgis = saveFc.FeatureCount(null);

                sw.Stop();
                long time_arcgis = sw.ElapsedMilliseconds;
                AppendText("结果数", count_arcgis.ToString(), null);
                AppendText("时间（秒）", (time_arcgis / 1000.0).ToString(".000"), null);

                //pg
                SetProgress("PG裁剪，正在执行...", 80);
                sw.Restart();
                //DataSet ds = Execute_PG(GetIntersectionSql(wkt));
                //int count_pg = 0;
                //DataTable dt = null;
                //if (ds.Tables.Count > 0) {
                //    dt = ds.Tables[0];
                //    count_pg = dt.Rows.Count;
                //}

                int count_pg = 0;
                NpgsqlDataReader reader = ExecuteReader_PG(GetIntersectionSql(wkt));
                DataTable dt = new DataTable();
                DataColumn col;
                int fCount = reader.FieldCount;
                for (int i = 0; i < fCount; i++) {
                    col = new DataColumn();
                    col.ColumnName = reader.GetName(i);
                    col.DataType = reader.GetFieldType(i);
                    dt.Columns.Add(col);
                }
                DataRow row;
                string fName;
                while (reader.Read()) {
                    if (count_pg <= 50) {
                        row = dt.NewRow();
                        for (int i = 0; i < fCount; i++) {
                            fName = dt.Columns[i].ColumnName;
                            row[fName] = reader[fName];
                        }
                        dt.Rows.Add(row);
                    }
                    count_pg++;
                }
                reader.Close();

                sw.Stop();
                long time_pg = sw.ElapsedMilliseconds;
                AppendText("结果数", "", count_pg.ToString());
                AppendText("时间（秒）", "", (time_pg / 1000.0).ToString(".000"));

                SetProgress("裁剪完成...", 100);
                AddDgvSplitFlag();
                if (dt != null) {
                    AttributeForm af = new AttributeForm(dt);
                    af.Show();
                }
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void AddDgvSplitFlag() {
            int index = dgv.Rows.Add();
            dgv.Rows[index].DefaultCellStyle.BackColor = Color.LightGray;
        }

        private void AddDgvRow(string fun, string arcgis, string pg) {
            DataGridViewRow row = dgv.Rows[dgv.Rows.Add()];
            row.Cells["C"].Value = fun;
            row.Cells["CARCGIS"].Value = arcgis;
            row.Cells["CPG"].Value = pg;
            Application.DoEvents();
        }

        private void ClearFc(IFeatureClass fc) {
            IFeatureCursor fCursor = fc.Search(null, false);
            IFeature f = null;
            while ((f = fCursor.NextFeature()) != null) {
                f.Delete();
            }
        }

        private void SetProgress(string txt, int value) {
            toolStripStatusLabel2.Text = txt;
            toolStripProgressBar1.Value = value;
            Thread.Sleep(10);
            Application.DoEvents();
        }

        private void btnRefresh_Click(object sender, EventArgs e) {

            axMapControl.Refresh();
        }

        private void Left3kW() {
            IFeatureClass fc = OpenGdbFeatureClass(@"D:\Data\EDGES3kW.gdb", "EDGES");
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "ID > 30000000";
            (fc as ITable).DeleteSearchedRows(qf);

            MessageBox.Show("finish");
        }

        private void btnQuery_Click(object sender, EventArgs e) {
            try {
                QueryForm qf = new QueryForm();
                if (qf.ShowDialog() != DialogResult.OK) {
                    return;
                }
                string txt = qf.QueryTxt;
                string whereSql = "fullname = '" + txt + "'";

                dgv.Rows.Clear();

                SetProgress("开始查询'" + txt + "'...", 10);
                AddDgvSplitFlag();
                AddDgvRow("查询", "", "");

                IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

                Stopwatch sw = new Stopwatch();
                SetProgress("ArcGIS查询，正在执行...", 40);

                AddDgvRow("结果数", "", "");
                AddDgvRow("时间（秒）", "", "");

                sw.Restart();
                //arcgis
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = whereSql;
                double count_arcgis = 0;
                IFeatureCursor fcursor = fc.Search(queryFilter, false);
                IFeature f = null;
                while ((f = fcursor.NextFeature()) != null) {
                    count_arcgis++;
                }

                sw.Stop();
                long time_arcgis = sw.ElapsedMilliseconds;
                AppendText("结果数", count_arcgis.ToString(), null);
                AppendText("时间（秒）", (time_arcgis / 1000.0).ToString(".000"), null);

                //pg
                SetProgress("PG查询，正在执行...", 80);
                sw.Restart();
                //DataSet ds = Execute_PG(GetAllWhereSql(whereSql));
                //int count_pg = 0;
                //DataTable dt = null;
                //if (ds.Tables.Count > 0) {
                //    dt = ds.Tables[0];
                //    count_pg = dt.Rows.Count;
                //}

                int count_pg = 0;
                NpgsqlDataReader reader = ExecuteReader_PG(GetAllWhereSql(whereSql));
                DataTable dt = new DataTable();
                DataColumn col;
                int fCount = reader.FieldCount;
                for (int i = 0; i < fCount; i++) {
                    col = new DataColumn();
                    col.ColumnName = reader.GetName(i);
                    col.DataType = reader.GetFieldType(i);
                    dt.Columns.Add(col);
                }
                DataRow row;
                string fName;
                while (reader.Read()) {
                    if (dt.Rows.Count <= 50) {
                        row = dt.NewRow();
                        for (int i = 0; i < fCount; i++) {
                            fName = dt.Columns[i].ColumnName;
                            row[fName] = reader[fName];
                        }
                        dt.Rows.Add(row);
                    }
                    count_pg++;
                }
                reader.Close();

                sw.Stop();
                long time_pg = sw.ElapsedMilliseconds;
                AppendText("结果数", "", count_pg.ToString());
                AppendText("时间（秒）", "", (time_pg / 1000.0).ToString(".000"));

                SetProgress("查询完成...", 100);
                AddDgvSplitFlag();
                if (dt != null) {
                    AttributeForm af = new AttributeForm(dt);
                    af.Show();
                }
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                MessageBox.Show("测试错误，请重试");
            }
        }


        static void Log(string txt) {
            if (!string.IsNullOrEmpty(LogPath)) {
                Append(LogPath, txt + "\r\n");
            }
        }

        public static void Append(string path, string text) {
            try {
                if (!File.Exists(path)) {
                    Create(path);
                }
                if (text == null)
                    return;
                StreamWriter sw = File.AppendText(path);
                sw.Write(text);
                sw.Close();
            }
            catch {
                throw;
            }
        }

        public static void Create(string path) {
            try {
                System.IO.FileStream fs = new System.IO.FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                fs.Close();
            }
            catch {
                throw;
            }
        }

    }
}
