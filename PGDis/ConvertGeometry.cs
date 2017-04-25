using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGDis
{
    class ConvertGeometry
    {
        ///
        /// 将ESRIGeometry转换为WKB 仅支持简单几何对象
        ///
        ///
        ///
        public static byte[] ConvertGeometryToWKB(IGeometry geometry)
        {
            IWkb wkb = geometry as IWkb;
            

            ITopologicalOperator6 oper = geometry as ITopologicalOperator6;
            oper.SimplifyAsFeature();
            IGeometryFactory3 factory = new GeometryEnvironment() as IGeometryFactory3;
            byte[] b = factory.CreateWkbVariantFromGeometry(geometry) as byte[];

            return b;
        }

        public static IGeometry ConvertWKBToGeometry(byte[] wkb)
        {
            IGeometry geom;
            int countin = wkb.GetLength(0);
            IGeometryFactory3 factory = new GeometryEnvironment() as IGeometryFactory3;
            factory.CreateGeometryFromWkbVariant(wkb, out geom, out countin);
            return geom;
        }


        public static byte[] ConvertWKTToWKB(string wkt)
        {
            //WKBWriter writer = new WKBWriter();
            //WKTReader reader = new WKTReader();
            //return writer.Write(reader.Read(wkt));//出错
            return null;
        }

        public static string ConvertWKBToWKT(byte[] wkb)
        {
            //WKTWriter writer = new WKTWriter();
            //WKBReader reader = new WKBReader();
            //return writer.Write(reader.Read(wkb));
            return "";
        }

        public static string ConvertGeometryToWKT(IGeometry geometry)
        {
            byte[] b = ConvertGeometryToWKB(geometry);
            string wkt = ConvertWKBToWKT(b);
            return wkt;

        }


        ///
        /// 检核WKT有效性
        ///
        ///
        ///
        public static int CheckWKTValid(string wkt)
        {
            try
            {
                byte[] wkb = ConvertWKTToWKB(wkt);
                IGeometry pG = ConvertWKBToGeometry(wkb);
                ITopologicalOperator oper = pG as ITopologicalOperator;
                if (oper.IsSimple && !pG.IsEmpty)
                    return 1;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }


        ///
        /// 通过GeoAPI将Geometry转WKT
        ///
        ///
        ///
        //public static string ConvertGeometryToWKT(IGeometry geometry)
        //{
        //    byte[] b = ConvertGeometryToWKB(geometry);
        //    WKBReader reader = new WKBReader();
        //    GeoAPI.Geometries.IGeometry g = reader.Read(b);
        //    WKTWriter writer = new WKTWriter();
        //    return writer.Write(g);
        //}

        public static IGeometry ConvertWKTToGeometry(string wkt)
        {
            byte[] wkb = ConvertWKTToWKB(wkt);
            return ConvertWKBToGeometry(wkb);
        }



        //public static IGeometry ConvertGeoAPIToESRI(GeoAPI.Geometries.IGeometry geometry)
        //{
        //    WKBWriter writer = new WKBWriter();
        //    byte[] bytes = writer.Write(geometry);
        //    return ConvertWKBToGeometry(bytes);
        //}

        //public static GeoAPI.Geometries.IGeometry ConvertESRIToGeoAPI(IGeometry geometry)
        //{
        //    byte[] wkb = ConvertGeometryToWKB(geometry);
        //    WKBReader reader = new WKBReader();
        //    return reader.Read(wkb);
        //}
    }
}
