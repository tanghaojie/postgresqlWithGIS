using ESRI.ArcGIS.Carto;
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
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;

namespace PGDis {
    //
    public partial class Form1 : Form {
        //edges gdb 
        static readonly string EDGES_GdbPath = ConfigurationManager.AppSettings["GdbPath"];
        static readonly string EDGES_FeatureClassName = ConfigurationManager.AppSettings["GdbFeatureClassName"];

        //my define show gdb
        static readonly string MyDefineGdbPath = ConfigurationManager.AppSettings["UserGdbPath"];
        static readonly string MyDefineGdbFcName = ConfigurationManager.AppSettings["UserGdbFcName"];

        //Cluster
        static readonly string HOST = ConfigurationManager.AppSettings["PgHost"];
        static readonly string PORT = ConfigurationManager.AppSettings["PgPort"];
        static readonly string USER = ConfigurationManager.AppSettings["PgUser"];
        static readonly string PASSWORD = ConfigurationManager.AppSettings["PgPass"];
        static readonly string DB = ConfigurationManager.AppSettings["PgDb"];
        static readonly string TABLENAME = ConfigurationManager.AppSettings["PgTableName"];
        static string connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
        HOST, PORT, USER, PASSWORD, DB);
        //Alone
        static readonly string HOST_Alone = ConfigurationManager.AppSettings["PgHostAlone"];
        static readonly string PORT_Alone = ConfigurationManager.AppSettings["PgPortAlone"];
        static readonly string USER_Alone = ConfigurationManager.AppSettings["PgUserAlone"];
        static readonly string PASSWORD_Alone = ConfigurationManager.AppSettings["PgPassAlone"];
        static readonly string DB_Alone = ConfigurationManager.AppSettings["PgDbAlone"];
        static readonly string TABLENAME_Alone = ConfigurationManager.AppSettings["PgTableNameAlone"];
        static string connString_Alone = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
   HOST_Alone, PORT_Alone, USER_Alone, PASSWORD_Alone, DB_Alone);

        //arcgis server
        static readonly string AGSServerLayerUrl = ConfigurationManager.AppSettings["AGSServerLayerUrl"];
        static readonly string AGSServerLayerName = ConfigurationManager.AppSettings["ServerName"];

        //Wmts server
        static readonly string WmtsServerLayerUrl = ConfigurationManager.AppSettings["WmtsServerLayerUrl"];

        //Log Path
        static readonly string LogPath = ConfigurationManager.AppSettings["LogPath"];

        //static readonly string TEMP_GDBPATH = Application.StartupPath + @"\temp.gdb";
        //const string TEMP_DRAWPOLYGONNAME = "DrawPolygon";
        //const string TEMP_DRAWPOLYLINENAME = "DrawPolyline";
        static readonly string TEMP2_GDBPATH = Application.StartupPath + @"\temp2.gdb";
        const string TEMP2_POLYGONNAME = "Polygon";
        const string TEMP2_POLYLINENAME = "Polyline";

        Stopwatch sw = new Stopwatch();

        #region 空间相交

        const string IntersectName = "空间相交";
        const string IntersectArea = "相交面积";
        const string IntersectSum = "相交总数";
        const string IntersectTime = "时间（秒）";

        #endregion

        #region 裁剪

        const string IntersectionName = "裁剪";
        const string IntersectionArea = "裁剪面积";
        const string IntersectionSum = "结果数";
        const string IntersectionTime = "时间（秒）";

        #endregion

        #region 裁剪和统计

        const string IntersectionAndStatisticName = "裁剪统计";
        const string IntersectionAndStatisticArea = "裁剪面积";
        const string IntersectionAndStatisticSum = "结果数";
        const string IntersectionAndStatisticLength = "结果总长";
        const string IntersectionAndStatisticTime = "时间（秒）";

        #endregion

        #region 查询

        const string QueryName = "查询";
        const string QuerySum = "结果数";
        const string QueryTime = "时间（秒）";

        #endregion

        static readonly string DgvColumn1Name = ConfigurationManager.AppSettings["DgvName1"];
        static readonly string DgvColumn2Name = ConfigurationManager.AppSettings["DgvName2"];
        static readonly string DgvColumn3Name = ConfigurationManager.AppSettings["DgvName3"];
        static readonly string DgvColumn4Name = ConfigurationManager.AppSettings["DgvName4"];

        esriControlsMousePointer Default_Pointer;

        IWorkspaceFactory GDBWorkspaceFactory = new FileGDBWorkspaceFactoryClass();

        private enum ExecuteMode {
            None = 0,
            Intersect = 1,
            Intersection = 2,
            IntersectionAndStatistic = 3,
            Query = 4
        }
        ExecuteMode executeMode;

        NpgsqlConnection conn;
        NpgsqlConnection conn_Alone;
        private void ConnectionPg() {
            conn = new NpgsqlConnection(connString);
            conn.Open();

            if (dgv.Columns.Contains(DgvColumn3Name)) {
                conn_Alone = new NpgsqlConnection(connString_Alone);
                conn_Alone.Open();
            }
        }

        public Form1() {
            InitializeComponent();
            SetDgvColumns();
            ConnectionPg();
            Default_Pointer = esriControlsMousePointer.esriPointerPan;

            //arcgis server
            if (!string.IsNullOrEmpty(AGSServerLayerUrl) && !string.IsNullOrEmpty(AGSServerLayerName)) {
                OpenServer os = new OpenServer();
                ILayer layer = os.GetARGServerLyr(AGSServerLayerUrl, AGSServerLayerName, false);
                axMapControl.AddLayer(layer);
            }
            //wmts server
            if (!string.IsNullOrEmpty(WmtsServerLayerUrl)) {
                OpenServer os = new OpenServer();
                IWMTSLayer layer = os.GetWMTSServerLyr(WmtsServerLayerUrl);
                ILayer ll = layer as ILayer;
                axMapControl.AddLayer(ll);
            }
            //myUS
            if (!string.IsNullOrEmpty(MyDefineGdbPath) && !string.IsNullOrEmpty(MyDefineGdbFcName)) {
                IFeatureClass us = OpenGdbFeatureClass(MyDefineGdbPath, MyDefineGdbFcName);
                IFeatureLayer fl_us = new FeatureLayerClass();
                fl_us.FeatureClass = us;
                axMapControl.AddLayer(fl_us);
            }

            #region old draw polygon and polyline
            ////default draw polygon polyline gdb file
            //IWorkspace ws = GDBWorkspaceFactory.OpenFromFile(DEFAULT_GDBPATH, 0);
            //Wse_Draw = ws as IWorkspaceEdit;

            //IFeatureClass Fc_DrawPolygon = (ws as IFeatureWorkspace).OpenFeatureClass(DEFAULT_DRAWPOLYGONNAME);
            //Fl_DrawPolygon = new FeatureLayerClass();
            //Fl_DrawPolygon.FeatureClass = Fc_DrawPolygon;
            //axMapControl.AddLayer(Fl_DrawPolygon);

            //IFeatureClass Fc_DrawPolyline = (ws as IFeatureWorkspace).OpenFeatureClass(DEFAULT_DRAWPOLYLINENAME);
            //Fl_DrawPolyline = new FeatureLayerClass();
            //Fl_DrawPolyline.FeatureClass = Fc_DrawPolyline;
            //axMapControl.AddLayer(Fl_DrawPolyline);
            ////default mouse pointer
            //Default_Pointer = esriControlsMousePointer.esriPointerDefault;
            //axMapControl.MousePointer = Default_Pointer;
            //axMapControl.MouseIcon = Properties.Resources.EditingEditTool;
            #endregion
        }

        private void SetDgvColumns() {
            dgv.Columns.Clear();
            DataGridViewColumn dgvCol;
            dgvCol = dgv.Columns[dgv.Columns.Add(DgvColumn1Name, DgvColumn1Name)];
            dgvCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCol.ReadOnly = true;

            dgvCol = dgv.Columns[dgv.Columns.Add(DgvColumn2Name, DgvColumn2Name)];
            dgvCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCol.ReadOnly = true;

            if (!string.IsNullOrEmpty(DgvColumn3Name)) {
                dgvCol = dgv.Columns[dgv.Columns.Add(DgvColumn3Name, DgvColumn3Name)];
                dgvCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvCol.ReadOnly = true;
            }
            dgvCol = dgv.Columns[dgv.Columns.Add(DgvColumn4Name, DgvColumn4Name)];
            dgvCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCol.ReadOnly = true;
        }

        private void axMapControl_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e) {
            Perform(e.mapX, e.mapY);
        }

        private void Perform(double x, double y) {
            if (axMapControl.MousePointer == esriControlsMousePointer.esriPointerPan) {
                MapMove();
            }
            else if (axMapControl.MousePointer == esriControlsMousePointer.esriPointerPencil) {
                axMapControl.Tag = null;
                IGeometry geo = TrackPolygon();
                if (geo == null) {
                    dgv.Rows.Clear();
                    return;
                }
                IPolygonElement polygonElement = new PolygonElementClass();
                IElement element = polygonElement as IElement;
                element.Geometry = geo;
                IGraphicsContainer graphicsContainer = axMapControl.Map as IGraphicsContainer;
                graphicsContainer.AddElement((IElement)polygonElement, 0);
                //axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                axMapControl.Tag = geo;
                axMapControl.Refresh();
            }
            else {
                MapMove();
            }
            //else if (axMapControl.MousePointer == esriControlsMousePointer.esriPointerCustom) {
            //    Select(x, y);
            //}
        }

        private void MapMove() {
            axMapControl.Pan();
            axMapControl.MousePointer = Default_Pointer;
        }

        private void Select(double x, double y) {
            IGeometry geo = axMapControl.TrackRectangle();
            axMapControl.Map.SelectByShape(geo, null, true);
            axMapControl.Refresh();
        }

        private IGeometry TrackPolygon() {
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
            IGeometry geo = axMapControl.TrackPolygon();
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerPan;
            return geo;
        }

        private IGeometry TrackPolyline() {
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
            IGeometry geo = axMapControl.TrackLine();
            axMapControl.MousePointer = esriControlsMousePointer.esriPointerPan;
            return geo;
        }

        private void btnEdit_Click(object sender, EventArgs e) {
            //if (Wse_Draw.IsBeingEdited()) {
            //    Wse_Draw.StopEditOperation();
            //    Wse_Draw.StopEditing(true);
            //    btnEdit.Text = "开始编辑";
            //    btnDrawPolygon.Enabled = false;
            //    btnDrawPolyline.Enabled = false;
            //    delF.Enabled = false;
            //    Default_Pointer = esriControlsMousePointer.esriPointerDefault;
            //    axMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
            //    axMapControl.Map.ClearSelection();
            //}
            //else {
            //    Wse_Draw.StartEditing(false);
            //    Wse_Draw.StartEditOperation();
            //    btnEdit.Text = "结束编辑";
            //    btnDrawPolygon.Enabled = true;
            //    btnDrawPolyline.Enabled = true;
            //    delF.Enabled = true;
            //    Default_Pointer = esriControlsMousePointer.esriPointerCustom;
            //    axMapControl.MousePointer = esriControlsMousePointer.esriPointerCustom;
            //}
        }

        private void btnSelect_Click(object sender, EventArgs e) {
            //if (Wse_Draw.IsBeingEdited()) {
            //    btnDrawPolygon.Enabled = true;
            //    btnDrawPolyline.Enabled = true;
            //}
            //Default_Pointer = esriControlsMousePointer.esriPointerCustom;
            //axMapControl.MousePointer = esriControlsMousePointer.esriPointerCustom;
        }

        private void btnDrawPolygon_Click(object sender, EventArgs e) {
            //btnDrawPolygon.Enabled = false;
            //btnDrawPolyline.Enabled = true;
            //Default_Pointer = esriControlsMousePointer.esriPointerPencil;
            //axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
            //drawMode = DrawMode.Polygon;
        }

        private void btnDrawPolyline_Click(object sender, EventArgs e) {
            //btnDrawPolygon.Enabled = true;
            //btnDrawPolyline.Enabled = false;
            //Default_Pointer = esriControlsMousePointer.esriPointerPencil;
            //axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
            //drawMode = DrawMode.Polyline;
        }

        private void delF_Click(object sender, EventArgs e) {
            //IFeature fea = GetSelectedFeature();
            //if (fea != null) {
            //    fea.Delete();
            //}

            //axMapControl.Refresh();
        }

        private IFeature GetSelectedFeature() {
            //IFeatureSelection fs = Fl_DrawPolygon as IFeatureSelection;
            //IEnumIDs ids = fs.SelectionSet.IDs;
            //int id = -1;
            //while ((id = ids.Next()) > 0) {
            //    return Fl_DrawPolygon.FeatureClass.GetFeature(id);
            //}

            //fs = Fl_DrawPolyline as IFeatureSelection;
            //ids = fs.SelectionSet.IDs;
            //id = -1;
            //while ((id = ids.Next()) > 0) {
            //    return Fl_DrawPolyline.FeatureClass.GetFeature(id);
            //}
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

        private DataSet Execute_PgWithoutConn(string sql, NpgsqlConnection conn) {
            DataSet ds = new DataSet();
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) {
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd)) {
                    da.Fill(ds);
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

        private NpgsqlDataReader ExecuteReader_PgWithoutConn(string sql, NpgsqlConnection conn) {
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            return reader;
        }

        private ISpatialFilter SetSpatialFilter(IFeature intersectF, esriSpatialRelEnum spatialRelEnum = esriSpatialRelEnum.esriSpatialRelIntersects) {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = intersectF.Shape;
            spatialFilter.GeometryField = (intersectF.Table as IFeatureClass).ShapeFieldName;
            spatialFilter.SpatialRel = spatialRelEnum;
            return spatialFilter;
        }

        private ISpatialFilter SetSpatialFilter(IGeometry geo, esriSpatialRelEnum spatialRelEnum = esriSpatialRelEnum.esriSpatialRelIntersects) {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = geo;
            //spatialFilter.GeometryField = (intersectF.Table as IFeatureClass).ShapeFieldName;
            spatialFilter.SpatialRel = spatialRelEnum;
            return spatialFilter;
        }

        private void ClearElement() {
            axMapControl.Tag = null;
            IGraphicsContainer graphicsContainer = axMapControl.Map as IGraphicsContainer;
            graphicsContainer.DeleteAllElements();
            axMapControl.Refresh();
        }

        private void btnIntersect_Click(object sender, EventArgs e) {
            Intersect();
        }

        private void Intersect() {
            try {
                axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
                dgv.Rows.Clear();
                ClearElement();
                executeMode = ExecuteMode.Intersect;

                AddDgvSplitFlag();
                AddDgvRow(IntersectName, "", "", "");
                AddDgvRow(IntersectArea, "", "", "");
                AddDgvRow(IntersectSum, "", "", "");
                AddDgvRow(IntersectTime, "", "", "");
                AddDgvRowButton();
                AddDgvSplitFlag();
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void Intersect_old() {
            //try {
            //    dgv.Rows.Clear();
            //    IFeature fea = GetSelectedFeature();
            //    if (fea == null) {
            //        MessageBox.Show("未选中任何要素");
            //        return;
            //    }
            //    AddDgvSplitFlag();
            //    SetProgress("开始执行空间相交...", 10);
            //    AddDgvRow("空间相交", "", "");

            //    ISpatialFilter spatialFilter = SetSpatialFilter(fea);
            //    IGeometry geo = fea.Shape;
            //    string wkt = geo.ToWellKnownText();
            //    string sType = "";
            //    string sArcgis = "";
            //    string sPG = "";
            //    if (geo.GeometryType == esriGeometryType.esriGeometryPolygon) {
            //        IArea area = geo as IArea;
            //        if (area != null) {
            //            double b = Math.Abs(area.Area);
            //            sType = "相交要素面积";
            //            sArcgis = sPG = b.ToString();
            //        }
            //    }
            //    else if (geo.GeometryType == esriGeometryType.esriGeometryPolyline) {
            //        IPolyline line = geo as IPolyline;
            //        if (line != null) {
            //            double b = line.Length;
            //            sType = "相交要素长度";
            //            sArcgis = sPG = b.ToString();
            //        }
            //    }
            //    if (!string.IsNullOrEmpty(sType) && !string.IsNullOrEmpty(sArcgis) && !string.IsNullOrEmpty(sPG)) {
            //        AddDgvRow(sType, sArcgis, sPG);
            //    }
            //    AddDgvRow("相交结果数", "", "");
            //    AddDgvRow("时间（秒）", "", "");

            //    IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

            //    Stopwatch sw = new Stopwatch();
            //    sw.Restart();
            //    //arcgis 
            //    SetProgress("ArcGIS空间相交，正在执行...", 40);
            //    int intersectCount_arcgis = SearchCount_ArcGIS(fc, spatialFilter);

            //    sw.Stop();
            //    long time_arcgis = sw.ElapsedMilliseconds;

            //    SetProgress("PG空间相交，正在执行...", 80);
            //    AppendDgvText("相交结果数", intersectCount_arcgis.ToString(), null);
            //    AppendDgvText("时间（秒）", (time_arcgis / 1000.0).ToString(".000"), null);
            //    sw.Restart();

            //    //pg
            //    //DataSet ds = Execute_PG(GetIntersectCountSql(wkt));
            //    //DataTable dt = null;
            //    //if (ds.Tables.Count > 0) {
            //    //    dt = ds.Tables[0];
            //    //    if (dt.Rows.Count > 0) {
            //    //        object obj = dt.Rows[0][0];
            //    //        if (obj != DBNull.Value && obj != null) {
            //    //            intersectCount_pg = int.Parse(obj.ToString());
            //    //        }
            //    //    }
            //    //}

            //    //int intersectCount_pg = 0;
            //    //DataSet ds = Execute_PG(GetIntersectSql(wkt));
            //    //DataTable dt = null;
            //    //if (ds.Tables.Count > 0) {
            //    //    dt = ds.Tables[0];
            //    //    intersectCount_pg = dt.Rows.Count;
            //    //}

            //    int intersectCount_pg = 0;
            //    NpgsqlDataReader reader = ExecuteReader_PG(GetIntersectSql(wkt));
            //    DataTable dt = new DataTable();
            //    DataColumn col;
            //    int fCount = reader.FieldCount;
            //    for (int i = 0; i < fCount; i++) {
            //        col = new DataColumn();
            //        col.ColumnName = reader.GetName(i);
            //        col.DataType = reader.GetFieldType(i);
            //        dt.Columns.Add(col);
            //    }
            //    DataRow row;
            //    string fName;
            //    while (reader.Read()) {
            //        if (intersectCount_pg <= 50) {
            //            row = dt.NewRow();
            //            for (int i = 0; i < fCount; i++) {
            //                fName = dt.Columns[i].ColumnName;
            //                row[fName] = reader[fName];
            //            }
            //            dt.Rows.Add(row);
            //        }
            //        intersectCount_pg++;
            //    }
            //    reader.Close();

            //    sw.Stop();
            //    long time_pg = sw.ElapsedMilliseconds;
            //    AppendDgvText("相交结果数", null, intersectCount_pg.ToString());
            //    AppendDgvText("时间（秒）", null, (time_pg / 1000.0).ToString(".000"));

            //    SetProgress("空间相交执行完成", 100);
            //    AddDgvSplitFlag();
            //    if (dt != null) {
            //        AttributeForm af = new AttributeForm(dt);
            //        af.Show();
            //    }
            //}
            //catch (Exception ex) {
            //    Log(ex.Message + "-----" + ex.StackTrace);
            //    MessageBox.Show("测试错误，请重试");
            //}
        }

        private void AppendDgvText(string funRowName, string cluster, string alone, string arcgis) {
            foreach (DataGridViewRow row in dgv.Rows) {
                object value = row.Cells[DgvColumn1Name].Value;
                if (value == null || value == DBNull.Value) {
                    continue;
                }
                string s = value.ToString();
                if (s.ToUpper() == funRowName.ToUpper()) {
                    if (!string.IsNullOrEmpty(cluster)) {
                        row.Cells[DgvColumn2Name].Value = cluster;
                    }
                    if (!string.IsNullOrEmpty(alone) && dgv.Columns.Contains(DgvColumn3Name)) {
                        row.Cells[DgvColumn3Name].Value = alone;
                    }
                    if (!string.IsNullOrEmpty(arcgis)) {
                        row.Cells[DgvColumn4Name].Value = arcgis;
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
            ClipAndStatistic();
        }

        private void ClipAndStatistic() {
            try {
                axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
                dgv.Rows.Clear();
                ClearElement();
                executeMode = ExecuteMode.IntersectionAndStatistic;

                AddDgvSplitFlag();
                AddDgvRow(IntersectionAndStatisticName, "", "", "");
                AddDgvRow(IntersectionAndStatisticArea, "", "", "");
                AddDgvRow(IntersectionAndStatisticSum, "", "", "");
                AddDgvRow(IntersectionAndStatisticTime, "", "", "");
                AddDgvRow(IntersectionAndStatisticLength, "", "", "");
                AddDgvRowButton();
                AddDgvSplitFlag();
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void ClipAndStatistic_Old() {
            //try {
            //    dgv.Rows.Clear();

            //    IFeature fea = GetSelectedFeature();
            //    if (fea == null) {
            //        MessageBox.Show("未选中任何要素");
            //        return;
            //    }
            //    IGeometry geo = fea.Shape;
            //    string wkt = geo.ToWellKnownText();

            //    if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
            //        MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
            //        return;
            //    }
            //    string sArcgis = "";
            //    string sPG = "";
            //    IArea area = geo as IArea;
            //    if (area != null) {
            //        double b = Math.Abs(area.Area);
            //        sArcgis = sPG = b.ToString();
            //    }

            //    SetProgress("开始执行裁剪...", 10);
            //    AddDgvSplitFlag();
            //    AddDgvRow("裁剪统计", "", "");
            //    if (!string.IsNullOrEmpty(sArcgis) && !string.IsNullOrEmpty(sPG)) {
            //        AddDgvRow("裁剪面积", sArcgis, sPG);
            //    }

            //    IFeatureClass clipFC = OpenGdbFeatureClass(DEFAULT_TEMPGDBPATH, POLYGONNAME);
            //    ClearFc(clipFC);
            //    IFeature clipF = clipFC.CreateFeature();
            //    clipF.Shape = geo;
            //    clipF.Store();

            //    IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

            //    Stopwatch sw = new Stopwatch();
            //    SetProgress("ArcGIS裁剪，正在执行...", 40);

            //    AddDgvRow("结果数", "", "");
            //    AddDgvRow("时间（秒）", "", "");
            //    AddDgvRow("裁剪结果总长", "", "");

            //    sw.Restart();
            //    //arcgis
            //    string saveName = "clip" + DateTime.Now.ToFileTime();
            //    Clip(fc, clipFC, DEFAULT_TEMPGDBPATH + @"\" + saveName);
            //    IFeatureClass saveFc = OpenGdbFeatureClass(DEFAULT_TEMPGDBPATH, saveName);
            //    double totalLength_arcgis = 0;
            //    IFeatureCursor fcursor = saveFc.Search(null, false);
            //    IFeature f = null;
            //    int index = saveFc.FindField("SHAPE_LENGTH");
            //    if (index > 0) {
            //        while ((f = fcursor.NextFeature()) != null) {
            //            object value = f.Value[index];
            //            if (value != DBNull.Value && value != null) {
            //                string sValue = value.ToString();
            //                totalLength_arcgis += double.Parse(f.Value[index].ToString());
            //            }
            //        }
            //    }
            //    int count_arcgis = saveFc.FeatureCount(null);

            //    sw.Stop();
            //    long time_arcgis = sw.ElapsedMilliseconds;
            //    AppendDgvText("结果数", count_arcgis.ToString(), null);
            //    AppendDgvText("时间（秒）", (time_arcgis / 1000.0).ToString(".000"), null);
            //    AppendDgvText("裁剪结果总长", totalLength_arcgis.ToString(), null);

            //    //pg
            //    SetProgress("PG裁剪，正在执行...", 80);
            //    sw.Restart();
            //    DataSet ds = Execute_PG(GetIntersectionCountAndSumLengthSql(wkt));
            //    int count_pg = 0;
            //    double totalLength_pg = 0;
            //    if (ds.Tables.Count > 0) {
            //        DataTable dt = ds.Tables[0];
            //        if (dt.Rows.Count > 0) {
            //            count_pg = int.Parse(dt.Rows[0][0].ToString());
            //            totalLength_pg = double.Parse(dt.Rows[0][1].ToString());
            //        }
            //    }

            //    sw.Stop();
            //    long time_pg = sw.ElapsedMilliseconds;
            //    AppendDgvText("结果数", "", count_pg.ToString());
            //    AppendDgvText("时间（秒）", "", (time_pg / 1000.0).ToString(".000"));
            //    AppendDgvText("裁剪结果总长", "", totalLength_pg.ToString());

            //    SetProgress("裁剪完成...", 100);
            //    AddDgvSplitFlag();
            //}
            //catch (Exception ex) {
            //    Log(ex.Message + "-----" + ex.StackTrace);
            //    MessageBox.Show("测试错误，请重试");
            //}
        }

        private void btnOnlyClip_Click(object sender, EventArgs e) {
            OnlyClip();
        }

        private void OnlyClip() {
            try {
                axMapControl.MousePointer = esriControlsMousePointer.esriPointerPencil;
                dgv.Rows.Clear();
                ClearElement();
                executeMode = ExecuteMode.Intersection;

                AddDgvSplitFlag();
                AddDgvRow(IntersectionName, "", "", "");
                AddDgvRow(IntersectionArea, "", "", "");
                AddDgvRow(IntersectionSum, "", "", "");
                AddDgvRow(IntersectionTime, "", "", "");
                AddDgvRowButton();
                AddDgvSplitFlag();
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void OnlyClip_Old() {
            //try {
            //    dgv.Rows.Clear();

            //    IFeature fea = GetSelectedFeature();
            //    if (fea == null) {
            //        MessageBox.Show("未选中任何要素");
            //        return;
            //    }
            //    IGeometry geo = fea.Shape;
            //    string wkt = geo.ToWellKnownText();

            //    if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
            //        MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
            //        return;
            //    }
            //    string sArcgis = "";
            //    string sPG = "";
            //    IArea area = geo as IArea;
            //    if (area != null) {
            //        double b = Math.Abs(area.Area);
            //        sArcgis = sPG = b.ToString();
            //    }

            //    SetProgress("开始执行裁剪...", 10);
            //    AddDgvSplitFlag();
            //    AddDgvRow("裁剪", "", "");
            //    if (!string.IsNullOrEmpty(sArcgis) && !string.IsNullOrEmpty(sPG)) {
            //        AddDgvRow("裁剪面积", sArcgis, sPG);
            //    }

            //    IFeatureClass clipFC = OpenGdbFeatureClass(DEFAULT_TEMPGDBPATH, POLYGONNAME);
            //    ClearFc(clipFC);
            //    IFeature clipF = clipFC.CreateFeature();
            //    clipF.Shape = geo;
            //    clipF.Store();

            //    IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

            //    Stopwatch sw = new Stopwatch();
            //    SetProgress("ArcGIS裁剪，正在执行...", 40);

            //    AddDgvRow("结果数", "", "");
            //    AddDgvRow("时间（秒）", "", "");

            //    sw.Restart();
            //    //arcgis
            //    string saveName = "clip" + DateTime.Now.ToFileTime();
            //    Clip(fc, clipFC, DEFAULT_TEMPGDBPATH + @"\" + saveName);
            //    IFeatureClass saveFc = OpenGdbFeatureClass(DEFAULT_TEMPGDBPATH, saveName);
            //    //double totalLength_arcgis = 0;
            //    //IFeatureCursor fcursor = saveFc.Search(null, false);
            //    //IFeature f = null;
            //    //int index = saveFc.FindField("SHAPE_LENGTH");
            //    //if (index > 0) {
            //    //    while ((f = fcursor.NextFeature()) != null) {
            //    //        object value = f.Value[index];
            //    //        if (value != DBNull.Value && value != null) {
            //    //            string sValue = value.ToString();
            //    //            totalLength_arcgis += double.Parse(f.Value[index].ToString());
            //    //        }
            //    //    }
            //    //}
            //    int count_arcgis = saveFc.FeatureCount(null);

            //    sw.Stop();
            //    long time_arcgis = sw.ElapsedMilliseconds;
            //    AppendDgvText("结果数", count_arcgis.ToString(), null);
            //    AppendDgvText("时间（秒）", (time_arcgis / 1000.0).ToString(".000"), null);

            //    //pg
            //    SetProgress("PG裁剪，正在执行...", 80);
            //    sw.Restart();
            //    //DataSet ds = Execute_PG(GetIntersectionSql(wkt));
            //    //int count_pg = 0;
            //    //DataTable dt = null;
            //    //if (ds.Tables.Count > 0) {
            //    //    dt = ds.Tables[0];
            //    //    count_pg = dt.Rows.Count;
            //    //}

            //    int count_pg = 0;
            //    NpgsqlDataReader reader = ExecuteReader_PG(GetIntersectionSql(wkt));
            //    DataTable dt = new DataTable();
            //    DataColumn col;
            //    int fCount = reader.FieldCount;
            //    for (int i = 0; i < fCount; i++) {
            //        col = new DataColumn();
            //        col.ColumnName = reader.GetName(i);
            //        col.DataType = reader.GetFieldType(i);
            //        dt.Columns.Add(col);
            //    }
            //    DataRow row;
            //    string fName;
            //    while (reader.Read()) {
            //        if (count_pg <= 50) {
            //            row = dt.NewRow();
            //            for (int i = 0; i < fCount; i++) {
            //                fName = dt.Columns[i].ColumnName;
            //                row[fName] = reader[fName];
            //            }
            //            dt.Rows.Add(row);
            //        }
            //        count_pg++;
            //    }
            //    reader.Close();

            //    sw.Stop();
            //    long time_pg = sw.ElapsedMilliseconds;
            //    AppendDgvText("结果数", "", count_pg.ToString());
            //    AppendDgvText("时间（秒）", "", (time_pg / 1000.0).ToString(".000"));

            //    SetProgress("裁剪完成...", 100);
            //    AddDgvSplitFlag();
            //    if (dt != null) {
            //        AttributeForm af = new AttributeForm(dt);
            //        af.Show();
            //    }
            //}
            //catch (Exception ex) {
            //    Log(ex.Message + "-----" + ex.StackTrace);
            //    MessageBox.Show("测试错误，请重试");
            //}
        }

        private void AddDgvSplitFlag() {
            int index = dgv.Rows.Add();
            dgv.Rows[index].DefaultCellStyle.BackColor = Color.DarkGray;
        }

        private void AddDgvRow(string fun, string cluster, string alone, string arcgis) {
            DataGridViewRow row = dgv.Rows[dgv.Rows.Add()];
            row.Cells[DgvColumn1Name].Value = fun;
            row.Cells[DgvColumn2Name].Value = cluster;
            if (dgv.Columns.Contains(DgvColumn3Name)) {
                row.Cells[DgvColumn3Name].Value = alone;
            }
            row.Cells[DgvColumn4Name].Value = arcgis;
            Application.DoEvents();
        }

        private void AddDgvRowButton() {
            DataGridViewRow row = dgv.Rows[dgv.Rows.Add()];
            row.Cells[DgvColumn1Name].Value = "";
            DataGridViewButtonCell btnCellCluster = new DataGridViewButtonCell() { Value = "开始计算" };
            DataGridViewButtonCell btnCellAlone = new DataGridViewButtonCell() { Value = "开始计算" };
            DataGridViewButtonCell btnCellArcgis = new DataGridViewButtonCell() { Value = "开始计算" };

            row.Cells[DgvColumn2Name] = btnCellCluster;
            if (dgv.Columns.Contains(DgvColumn3Name)) {
                row.Cells[DgvColumn3Name] = btnCellAlone;
            }
            row.Cells[DgvColumn4Name] = btnCellArcgis;
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

            SetMainProgressBar(value);
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
            Query();
        }

        private void Query() {
            try {
                dgv.Rows.Clear();
                ClearElement();
                executeMode = ExecuteMode.Query;
                QueryForm qf = new QueryForm();
                if (qf.ShowDialog() != DialogResult.OK) {
                    return;
                }
                string txt = qf.QueryTxt;
                string whereSql = "fullname = '" + txt + "'";
                btnQuery.Tag = whereSql;

                AddDgvSplitFlag();
                AddDgvRow(QueryName, txt, txt, txt);
                AddDgvRow(QuerySum, "", "", "");
                AddDgvRow(QueryTime, "", "", "");
                AddDgvRowButton();
                AddDgvSplitFlag();
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void Query_Old() {
            //try {
            //    QueryForm qf = new QueryForm();
            //    if (qf.ShowDialog() != DialogResult.OK) {
            //        return;
            //    }
            //    string txt = qf.QueryTxt;
            //    string whereSql = "fullname = '" + txt + "'";

            //    dgv.Rows.Clear();

            //    SetProgress("开始查询'" + txt + "'...", 10);
            //    AddDgvSplitFlag();
            //    AddDgvRow("查询", "", "");

            //    IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

            //    Stopwatch sw = new Stopwatch();
            //    SetProgress("ArcGIS查询，正在执行...", 40);

            //    AddDgvRow("结果数", "", "");
            //    AddDgvRow("时间（秒）", "", "");

            //    sw.Restart();
            //    //arcgis
            //    IQueryFilter queryFilter = new QueryFilterClass();
            //    queryFilter.WhereClause = whereSql;
            //    double count_arcgis = 0;
            //    IFeatureCursor fcursor = fc.Search(queryFilter, false);
            //    IFeature f = null;
            //    while ((f = fcursor.NextFeature()) != null) {
            //        count_arcgis++;
            //    }

            //    sw.Stop();
            //    long time_arcgis = sw.ElapsedMilliseconds;
            //    AppendDgvText("结果数", count_arcgis.ToString(), null);
            //    AppendDgvText("时间（秒）", (time_arcgis / 1000.0).ToString(".000"), null);

            //    //pg
            //    SetProgress("PG查询，正在执行...", 80);
            //    sw.Restart();
            //    //DataSet ds = Execute_PG(GetAllWhereSql(whereSql));
            //    //int count_pg = 0;
            //    //DataTable dt = null;
            //    //if (ds.Tables.Count > 0) {
            //    //    dt = ds.Tables[0];
            //    //    count_pg = dt.Rows.Count;
            //    //}

            //    int count_pg = 0;
            //    NpgsqlDataReader reader = ExecuteReader_PG(GetAllWhereSql(whereSql));
            //    DataTable dt = new DataTable();
            //    DataColumn col;
            //    int fCount = reader.FieldCount;
            //    for (int i = 0; i < fCount; i++) {
            //        col = new DataColumn();
            //        col.ColumnName = reader.GetName(i);
            //        col.DataType = reader.GetFieldType(i);
            //        dt.Columns.Add(col);
            //    }
            //    DataRow row;
            //    string fName;
            //    while (reader.Read()) {
            //        if (dt.Rows.Count <= 50) {
            //            row = dt.NewRow();
            //            for (int i = 0; i < fCount; i++) {
            //                fName = dt.Columns[i].ColumnName;
            //                row[fName] = reader[fName];
            //            }
            //            dt.Rows.Add(row);
            //        }
            //        count_pg++;
            //    }
            //    reader.Close();

            //    sw.Stop();
            //    long time_pg = sw.ElapsedMilliseconds;
            //    AppendDgvText("结果数", "", count_pg.ToString());
            //    AppendDgvText("时间（秒）", "", (time_pg / 1000.0).ToString(".000"));

            //    SetProgress("查询完成...", 100);
            //    AddDgvSplitFlag();
            //    if (dt != null) {
            //        AttributeForm af = new AttributeForm(dt);
            //        af.Show();
            //    }
            //}
            //catch (Exception ex) {
            //    Log(ex.Message + "-----" + ex.StackTrace);
            //    MessageBox.Show("测试错误，请重试");
            //}
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

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) {
                return;
            }
            DataGridViewCell cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (!(cell is DataGridViewButtonCell)) {
                return;
            }
            string name = dgv.Columns[e.ColumnIndex].Name;
            if (name == DgvColumn2Name) {
                switch (executeMode) {
                    case ExecuteMode.Intersect:
                        Intersect_Cluster();
                        break;
                    case ExecuteMode.Intersection:
                        Intersection_Cluster();
                        break;
                    case ExecuteMode.IntersectionAndStatistic:
                        IntersectionAndStatistic_Cluster();
                        break;
                    case ExecuteMode.Query:
                        Query_Cluster();
                        break;
                    default:
                        return;
                }
            }
            else if (name == DgvColumn3Name) {
                switch (executeMode) {
                    case ExecuteMode.Intersect:
                        Intersect_Alone();
                        break;
                    case ExecuteMode.Intersection:
                        Intersection_Alone();
                        break;
                    case ExecuteMode.IntersectionAndStatistic:
                        IntersectionAndStatistic_Alone();
                        break;
                    case ExecuteMode.Query:
                        Query_Alone();
                        break;
                    default:
                        return;
                }
            }
            else if (name == DgvColumn4Name) {
                switch (executeMode) {
                    case ExecuteMode.Intersect:
                        Intersect_ArcGIS();
                        break;
                    case ExecuteMode.Intersection:
                        Intersection_ArcGIS();
                        break;
                    case ExecuteMode.IntersectionAndStatistic:
                        IntersectionAndStatistic_ArcGIS();
                        break;
                    case ExecuteMode.Query:
                        Query_ArcGIS();
                        break;
                    default:
                        return;
                }
            }
        }

        #region Intersect

        private void Intersect_Cluster() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行空间相交...", 40);
                IGeometry geo = tag as IGeometry;
                if (geo.GeometryType == esriGeometryType.esriGeometryPolygon) {
                    IArea area = geo as IArea;
                    if (area != null) {
                        double b = Math.Abs(area.Area);
                        double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                        AppendDgvText(IntersectArea, (meterArea / 1000000.0).ToString(), null, null);
                    }
                }
                else if (geo.GeometryType == esriGeometryType.esriGeometryPolyline) {
                    IPolyline line = geo as IPolyline;
                    if (line != null) {
                        double b = Math.Abs(line.Length);
                        double meterLength = GeographicCoordinateSystemUnitToMeter_Length(b, geo.SpatialReference as IGeographicCoordinateSystem);
                        AppendDgvText(IntersectArea, (meterLength / 1000.0).ToString(), null, null);
                    }
                }
                string wkt = geo.ToWellKnownText();
                DataTable dt = new DataTable();
                int intersectCount_pg = 0;

                sw.Restart();
                NpgsqlDataReader reader = ExecuteReader_PgWithoutConn(GetIntersectSql(wkt), conn);
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
                sw.Stop();
                reader.Close();

                long time_pg = sw.ElapsedMilliseconds;
                AppendDgvText(IntersectSum, intersectCount_pg.ToString(), null, null);
                AppendDgvText(IntersectTime, (time_pg / 1000.0).ToString(".000"), null, null);
                SetProgress("空间相交执行完成", 100);
                if (dt != null) {
                    AttributeForm af = new AttributeForm(dt);
                    af.Show();
                }
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void Intersect_Alone() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行空间相交...", 40);
                IGeometry geo = tag as IGeometry;
                if (geo.GeometryType == esriGeometryType.esriGeometryPolygon) {
                    IArea area = geo as IArea;
                    if (area != null) {
                        double b = Math.Abs(area.Area);
                        double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                        AppendDgvText(IntersectArea, null, (meterArea / 1000000.0).ToString(), null);
                    }
                }
                else if (geo.GeometryType == esriGeometryType.esriGeometryPolyline) {
                    IPolyline line = geo as IPolyline;
                    if (line != null) {
                        double b = Math.Abs(line.Length);
                        AppendDgvText(IntersectArea, null, b.ToString(), null);
                    }
                }
                string wkt = geo.ToWellKnownText();
                DataTable dt = new DataTable();
                int intersectCount_pg = 0;

                sw.Restart();
                NpgsqlDataReader reader = ExecuteReader_PgWithoutConn(GetIntersectSql(wkt), conn_Alone);
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
                sw.Stop();
                reader.Close();

                long time_pg = sw.ElapsedMilliseconds;
                AppendDgvText(IntersectSum, null, intersectCount_pg.ToString(), null);
                AppendDgvText(IntersectTime, null, (time_pg / 1000.0).ToString(".000"), null);
                SetProgress("空间相交执行完成", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void Intersect_ArcGIS() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行空间相交...", 30);
                IGeometry geo = tag as IGeometry;
                ISpatialFilter spatialFilter = SetSpatialFilter(geo);
                if (geo.GeometryType == esriGeometryType.esriGeometryPolygon) {
                    IArea area = geo as IArea;
                    if (area != null) {
                        double b = Math.Abs(area.Area);
                        double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                        AppendDgvText(IntersectArea, null, null, (meterArea / 1000000.0).ToString());
                    }
                }
                else if (geo.GeometryType == esriGeometryType.esriGeometryPolyline) {
                    IPolyline line = geo as IPolyline;
                    if (line != null) {
                        double b = Math.Abs(line.Length);
                        AppendDgvText(IntersectArea, null, null, b.ToString());
                    }
                }

                sw.Restart();

                IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);
                SetProgress("ArcGIS空间相交，正在执行...", 40);
                int intersectCount_arcgis = SearchCount_ArcGIS(fc, spatialFilter);

                sw.Stop();
                long time_arcgis = sw.ElapsedMilliseconds;

                AppendDgvText(IntersectSum, null, null, intersectCount_arcgis.ToString());
                AppendDgvText(IntersectTime, null, null, (time_arcgis / 1000.0).ToString(".000"));

                SetProgress("空间相交执行完成", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        #endregion

        #region Intersection

        private void Intersection_Cluster() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行裁剪...", 40);
                IGeometry geo = tag as IGeometry;
                if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
                    MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
                    return;
                }
                string wkt = geo.ToWellKnownText();
                IArea area = geo as IArea;
                if (area != null) {
                    double b = Math.Abs(area.Area);
                    double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                    AppendDgvText(IntersectionArea, (meterArea / 1000000.0).ToString(), null, null);
                }
                SetProgress("正在裁剪...", 60);
                int count_pg = 0;
                DataTable dt = new DataTable();
                DataColumn col;

                sw.Restart();
                NpgsqlDataReader reader = ExecuteReader_PgWithoutConn(GetIntersectionSql(wkt), conn);
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
                sw.Stop();
                reader.Close();

                long time_pg = sw.ElapsedMilliseconds;
                AppendDgvText(IntersectionSum, count_pg.ToString(), null, null);
                AppendDgvText(IntersectionTime, (time_pg / 1000.0).ToString(".000"), null, null);

                SetProgress("裁剪完成...", 100);
                if (dt != null) {
                    AttributeForm af = new AttributeForm(dt);
                    af.Show();
                }
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void Intersection_Alone() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行裁剪...", 40);
                IGeometry geo = tag as IGeometry;
                if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
                    MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
                    return;
                }
                string wkt = geo.ToWellKnownText();
                IArea area = geo as IArea;
                if (area != null) {
                    double b = Math.Abs(area.Area);
                    double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                    AppendDgvText(IntersectionArea, null, (meterArea / 1000000.0).ToString(), null);
                }
                SetProgress("正在裁剪...", 60);
                int count_pg = 0;
                DataTable dt = new DataTable();
                DataColumn col;

                sw.Restart();
                NpgsqlDataReader reader = ExecuteReader_PgWithoutConn(GetIntersectionSql(wkt), conn_Alone);
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
                sw.Stop();
                reader.Close();

                long time_pg = sw.ElapsedMilliseconds;
                AppendDgvText(IntersectionSum, null, count_pg.ToString(), null);
                AppendDgvText(IntersectionTime, null, (time_pg / 1000.0).ToString(".000"), null);

                SetProgress("裁剪完成...", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void Intersection_ArcGIS() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行裁剪...", 40);
                IGeometry geo = tag as IGeometry;
                if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
                    MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
                    return;
                }
                IArea area = geo as IArea;
                if (area != null) {
                    double b = Math.Abs(area.Area);
                    double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                    AppendDgvText(IntersectionArea, null, null, (meterArea / 1000000.0).ToString());
                }
                SetProgress("正在裁剪...", 60);

                sw.Restart();

                IFeatureClass clipFC = OpenGdbFeatureClass(TEMP2_GDBPATH, TEMP2_POLYGONNAME);
                ClearFc(clipFC);
                IFeature clipF = clipFC.CreateFeature();
                clipF.Shape = geo;
                clipF.Store();

                IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);

                SetProgress("ArcGIS裁剪，正在执行...", 60);
                string saveName = "clip" + DateTime.Now.ToFileTime();
                Clip(fc, clipFC, TEMP2_GDBPATH + @"\" + saveName);
                IFeatureClass saveFc = OpenGdbFeatureClass(TEMP2_GDBPATH, saveName);
                int count_arcgis = saveFc.FeatureCount(null);

                sw.Stop();
                long time_arcgis = sw.ElapsedMilliseconds;
                AppendDgvText(IntersectionSum, null, null, count_arcgis.ToString());
                AppendDgvText(IntersectionTime, null, null, (time_arcgis / 1000.0).ToString(".000"));
                SetProgress("裁剪完成...", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        #endregion

        #region IntersectionAndStatistic

        private void IntersectionAndStatistic_Cluster() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行裁剪...", 40);
                IGeometry geo = tag as IGeometry;
                if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
                    MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
                    return;
                }
                string wkt = geo.ToWellKnownText();
                IArea area = geo as IArea;
                if (area != null) {
                    double b = Math.Abs(area.Area);
                    double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                    AppendDgvText(IntersectionAndStatisticArea, (meterArea / 1000000.0).ToString(), null, null);
                }
                SetProgress("正在裁剪...", 60);

                sw.Restart();
                DataSet ds = Execute_PgWithoutConn(GetIntersectionCountAndSumLengthSql(wkt), conn);
                sw.Stop();

                int count_pg = 0;
                double totalLength_pg = 0;
                if (ds.Tables.Count > 0) {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0) {
                        count_pg = int.Parse(dt.Rows[0][0].ToString());
                        object ovalue = dt.Rows[0][1];
                        if (ovalue == null || ovalue == DBNull.Value) {
                            ovalue = 0;
                        }
                        totalLength_pg = double.Parse(ovalue.ToString());
                    }
                }

                long time_pg = sw.ElapsedMilliseconds;
                AppendDgvText(IntersectionAndStatisticSum, count_pg.ToString(), null, null);
                AppendDgvText(IntersectionAndStatisticTime, (time_pg / 1000.0).ToString(".000"), null, null);
                double meterLength = GeographicCoordinateSystemUnitToMeter_Length(totalLength_pg, geo.SpatialReference as IGeographicCoordinateSystem);
                AppendDgvText(IntersectionAndStatisticLength, (meterLength / 1000.0).ToString(), null, null);

                SetProgress("裁剪完成...", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void IntersectionAndStatistic_Alone() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行裁剪...", 40);
                IGeometry geo = tag as IGeometry;
                if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
                    MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
                    return;
                }
                string wkt = geo.ToWellKnownText();
                IArea area = geo as IArea;
                if (area != null) {
                    double b = Math.Abs(area.Area);
                    double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                    AppendDgvText(IntersectionAndStatisticArea, null, (meterArea / 1000000.0).ToString(), null);
                }
                SetProgress("正在裁剪...", 60);

                sw.Restart();
                DataSet ds = Execute_PgWithoutConn(GetIntersectionCountAndSumLengthSql(wkt), conn_Alone);
                sw.Stop();

                int count_pg = 0;
                double totalLength_pg = 0;
                if (ds.Tables.Count > 0) {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0) {
                        count_pg = int.Parse(dt.Rows[0][0].ToString());
                        object ovalue = dt.Rows[0][1];
                        if (ovalue == null || ovalue == DBNull.Value) {
                            ovalue = 0;
                        }
                        totalLength_pg = double.Parse(ovalue.ToString());
                    }
                }

                long time_pg = sw.ElapsedMilliseconds;
                AppendDgvText(IntersectionAndStatisticSum, null, count_pg.ToString(), null);
                AppendDgvText(IntersectionAndStatisticTime, null, (time_pg / 1000.0).ToString(".000"), null);
                double meterLength = GeographicCoordinateSystemUnitToMeter_Length(totalLength_pg, geo.SpatialReference as IGeographicCoordinateSystem);
                AppendDgvText(IntersectionAndStatisticLength, null, (meterLength / 1000.0).ToString(), null);

                SetProgress("裁剪完成...", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void IntersectionAndStatistic_ArcGIS() {
            try {
                object tag = axMapControl.Tag;
                if (tag == null || !(tag is IGeometry)) {
                    MessageBox.Show("未获得图形，请重试。");
                    return;
                }
                SetProgress("开始执行裁剪...", 40);
                IGeometry geo = tag as IGeometry;
                if (geo.GeometryType != esriGeometryType.esriGeometryPolygon) {
                    MessageBox.Show("裁剪要素类型错误。[请使用面要素]");
                    return;
                }
                IArea area = geo as IArea;
                if (area != null) {
                    double b = Math.Abs(area.Area);
                    double meterArea = GeographicCoordinateSystemUnitToMeter_Area(b, geo.SpatialReference as IGeographicCoordinateSystem);
                    AppendDgvText(IntersectionAndStatisticArea, null, null, (meterArea / 1000000.0).ToString());
                }
                SetProgress("正在裁剪...", 60);

                sw.Restart();

                IFeatureClass clipFC = OpenGdbFeatureClass(TEMP2_GDBPATH, TEMP2_POLYGONNAME);
                ClearFc(clipFC);
                IFeature clipF = clipFC.CreateFeature();
                clipF.Shape = geo;
                clipF.Store();
                IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);
                string saveName = "clip" + DateTime.Now.ToFileTime();
                Clip(fc, clipFC, TEMP2_GDBPATH + @"\" + saveName);
                SetProgress("正在统计总长...", 80);
                IFeatureClass saveFc = OpenGdbFeatureClass(TEMP2_GDBPATH, saveName);
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
                AppendDgvText(IntersectionAndStatisticSum, null, null, count_arcgis.ToString());
                AppendDgvText(IntersectionAndStatisticTime, null, null, (time_arcgis / 1000.0).ToString(".000"));
                double meterLength = GeographicCoordinateSystemUnitToMeter_Length(totalLength_arcgis, geo.SpatialReference as IGeographicCoordinateSystem);
                AppendDgvText(IntersectionAndStatisticLength, null, null, (meterLength / 1000.0).ToString());

                SetProgress("裁剪完成...", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        #endregion

        #region Query

        private void Query_Cluster() {
            try {
                object tag = btnQuery.Tag;
                if (tag == null || !(tag is string)) {
                    MessageBox.Show("未获得查询条件，请重试。");
                    return;
                }
                string whereSql = tag as string;

                SetProgress("开始查询...", 30);
                SetProgress("正在查询...", 50);
                int count_pg = 0;

                sw.Restart();
                NpgsqlDataReader reader = ExecuteReader_PgWithoutConn(GetAllWhereSql(whereSql), conn);
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
                sw.Stop();
                reader.Close();

                long time_pg = sw.ElapsedMilliseconds;
                AppendDgvText(QuerySum, count_pg.ToString(), null, null);
                AppendDgvText(QueryTime, (time_pg / 1000.0).ToString(".000"), null, null);

                SetProgress("查询完成...", 100);
                if (dt != null) {
                    AttributeForm af = new AttributeForm(dt);
                    af.Show();
                }
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void Query_Alone() {
            try {
                object tag = btnQuery.Tag;
                if (tag == null || !(tag is string)) {
                    MessageBox.Show("未获得查询条件，请重试。");
                    return;
                }
                string whereSql = tag as string;

                SetProgress("开始查询...", 30);
                SetProgress("正在查询...", 50);
                int count_pg = 0;

                sw.Restart();
                NpgsqlDataReader reader = ExecuteReader_PgWithoutConn(GetAllWhereSql(whereSql), conn_Alone);
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
                sw.Stop();
                reader.Close();

                long time_pg = sw.ElapsedMilliseconds;
                AppendDgvText(QuerySum, null, count_pg.ToString(), null);
                AppendDgvText(QueryTime, null, (time_pg / 1000.0).ToString(".000"), null);

                SetProgress("查询完成...", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        private void Query_ArcGIS() {
            try {
                object tag = btnQuery.Tag;
                if (tag == null || !(tag is string)) {
                    MessageBox.Show("未获得查询条件，请重试。");
                    return;
                }
                string whereSql = tag as string;

                SetProgress("开始查询...", 30);
                SetProgress("正在查询...", 50);

                sw.Restart();
                IFeatureClass fc = OpenGdbFeatureClass(EDGES_GdbPath, EDGES_FeatureClassName);
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
                AppendDgvText(QuerySum, null, null, count_arcgis.ToString());
                AppendDgvText(QueryTime, null, null, (time_arcgis / 1000.0).ToString(".000"));

                SetProgress("查询完成...", 100);
                SetMainProgressBar(-1);
            }
            catch (Exception ex) {
                Log(ex.Message + "-----" + ex.StackTrace);
                SetMainProgressBar(-1);
                MessageBox.Show("测试错误，请重试");
            }
        }

        #endregion

        private double GeographicCoordinateSystemUnitToMeter_Length(double length, IGeographicCoordinateSystem gcs) {
            if (gcs == null) {
                return length;
            }
            double datumMajor = gcs.Datum.Spheroid.SemiMajorAxis;
            double datumMinor = gcs.Datum.Spheroid.SemiMinorAxis;
            double average = (datumMajor + datumMinor) / 2;
            double conversionFactor = gcs.CoordinateUnit.ConversionFactor;
            double meterPerDegree = average * conversionFactor;

            return length * meterPerDegree;
        }

        private double GeographicCoordinateSystemUnitToMeter_Area(double area, IGeographicCoordinateSystem gcs) {
            if (gcs == null) {
                return area;
            }
            double datumMajor = gcs.Datum.Spheroid.SemiMajorAxis;
            double datumMinor = gcs.Datum.Spheroid.SemiMinorAxis;
            double average = (datumMajor + datumMinor) / 2;
            double conversionFactor = gcs.CoordinateUnit.ConversionFactor;
            double meterPerDegree = average * conversionFactor;

            return area * meterPerDegree * meterPerDegree;
        }

        private void SetMainProgressBar(int value) {
            if (value <= 0 || value >= 100) {
                progressBar.Visible = false;
            }
            else {
                progressBar.Value = value;
                int x = (this.Width - progressBar.Width) / 2;
                int y = (this.Height - progressBar.Height) / 2;
                progressBar.Location = new System.Drawing.Point(x, y);
                progressBar.BringToFront();
                progressBar.Visible = true;
            }
        }

    }
}
